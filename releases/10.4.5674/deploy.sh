#!/usr/bin/env bash
# Deploys the CrossOver stabilization patches (see ../../CLAUDE.md) to a live eM Client
# CrossOver bottle. Always regenerates the patches fresh from whatever assemblies are actually
# installed -- deliberately never copies pre-built DLLs out of this repo, since eM Client updates
# regularly and a pre-built copy would silently drift stale (or worse, be deployed against a
# version it was never built for). Slower than a copy, but every deploy is built and verified
# against the exact bytes it's about to patch.
#
# This script is versioned per eM Client release: it lives at releases/<version>/deploy.sh and
# is built and tested against that exact version (see EXPECTED_FILE_VERSION below). When eM
# Client updates, don't edit this file in place -- copy this whole releases/<version>/ folder to
# a new releases/<new-version>/, retest by hand (same process as CLAUDE.md's "Investigation
# method"), and adjust whatever patch logic broke. The version gate below is what makes that
# safe: an old release script run against a newer install won't silently do the wrong thing --
# it warns and asks first.
#
# Usage:
#   ./deploy.sh                  interactive: lists bottles found, prompts for choice + confirms
#   ./deploy.sh --bottle NAME    skip bottle selection (bottle dir name under ~/.cxoffice/)
#   ./deploy.sh --list           list found bottles and their installed versions, then exit
#   ./deploy.sh -y|--yes         don't prompt on a version mismatch, continue automatically
#   ./deploy.sh --force          skip the "already patched, nothing to do" short-circuit
#   ./deploy.sh --install-fonts  install the vendored fonts/ without the license-consent prompt
#   ./deploy.sh --no-fonts       skip font installation without the license-consent prompt
#
# Safe to re-run: if the target's MailClient.dll already references the
# MailClient.Licensing.BouncyCastlePatch assembly (a marker only this pipeline could have
# created, so this holds regardless of eM Client version or file size), the script reports that
# and exits cleanly without touching anything.
#
# Fonts: if fonts/*.ttf exist in this repo (genuine Microsoft fonts -- Segoe UI, Tahoma, Calibri
# -- vendored by whoever holds a valid license to use them; see reports/splash-tip-icon-findings.md),
# the script asks whether you hold a license and want them installed into the target bottle
# before deploying. --install-fonts / --no-fonts answer that non-interactively -- for scripted or
# repeated runs (e.g. a periodic check) where a prompt can't be answered by a human.
#
# Requires: dotnet SDK (checked below, prints install instructions if missing), python3 (for the
# deps.json patch step), network access (NuGet restore for Mono.Cecil, and ilspycmd if not
# already installed as a dotnet tool). Nothing else -- everything patch-related is built fresh
# each run into a temp directory that's cleaned up on success.

set -euo pipefail

RELEASE_VERSION="10.4.5674"
EXPECTED_FILE_VERSION="10.4.5674.0"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
IL_PATCHES_DIR="$REPO_ROOT/il-patches"
BACKUPS_DIR="$SCRIPT_DIR/backups"

log()  { echo "[deploy] $*"; }
warn() { echo "[deploy] WARNING: $*" >&2; }
err()  { echo "[deploy] ERROR: $*" >&2; }
die()  { err "$*"; exit 1; }

# ---------------------------------------------------------------------------
# Args
# ---------------------------------------------------------------------------

BOTTLE_OVERRIDE=""
ASSUME_YES=0
LIST_ONLY=0
FORCE=0
INSTALL_FONTS=0
NO_FONTS=0

print_help() {
    sed -n '2,39p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --bottle) BOTTLE_OVERRIDE="${2:-}"; shift 2 ;;
        -y|--yes) ASSUME_YES=1; shift ;;
        --list) LIST_ONLY=1; shift ;;
        --force) FORCE=1; shift ;;
        --install-fonts) INSTALL_FONTS=1; shift ;;
        --no-fonts) NO_FONTS=1; shift ;;
        -h|--help) print_help; exit 0 ;;
        *) die "unknown argument: $1 (see --help)" ;;
    esac
done

# ---------------------------------------------------------------------------
# dotnet / python3 checks
# ---------------------------------------------------------------------------

check_dotnet() {
    command -v dotnet >/dev/null 2>&1 && return 0
    err "dotnet SDK not found -- required to build the patch tools and read assembly versions."
    if [[ -f /etc/os-release ]]; then
        # shellcheck disable=SC1091
        . /etc/os-release
        case "${ID:-}" in
            debian|ubuntu|linuxmint|pop)
                echo "  Install with:  sudo apt update && sudo apt install -y dotnet-sdk-8.0" ;;
            fedora)
                echo "  Install with:  sudo dnf install -y dotnet-sdk-8.0" ;;
            rhel|centos|rocky|almalinux)
                echo "  Install with:  sudo dnf install -y dotnet-sdk-8.0"
                echo "  (may need Microsoft's package repo enabled first: https://learn.microsoft.com/dotnet/core/install/linux-rhel)" ;;
            opensuse*|sles)
                echo "  Install with:  sudo zypper install -y dotnet-sdk-8.0" ;;
            arch|manjaro)
                echo "  Install with:  sudo pacman -S dotnet-sdk" ;;
            *)
                echo "  See: https://learn.microsoft.com/dotnet/core/install/linux" ;;
        esac
    else
        echo "  See: https://learn.microsoft.com/dotnet/core/install/linux"
    fi
    die "install dotnet SDK and re-run this script."
}

check_python3() {
    command -v python3 >/dev/null 2>&1 && return 0
    err "python3 not found -- needed to patch MailClient.deps.json."
    echo "  Debian/Ubuntu:  sudo apt install -y python3"
    echo "  Fedora/RHEL:    sudo dnf install -y python3"
    die "install python3 and re-run this script."
}

check_dotnet
check_python3

# ---------------------------------------------------------------------------
# Workdir (self-cleaning: removed on success, left in place with its path
# printed if anything fails or the script is interrupted, so it can be
# inspected)
# ---------------------------------------------------------------------------

WORKDIR="$(mktemp -d /tmp/emclient-deploy-${RELEASE_VERSION}-XXXXXX)"
CLEANUP_ON_EXIT=1

cleanup() {
    local status=$?
    if [[ $status -ne 0 || $CLEANUP_ON_EXIT -eq 0 ]]; then
        warn "leaving workdir for inspection: $WORKDIR"
    else
        rm -rf "$WORKDIR"
    fi
}
trap cleanup EXIT

# ---------------------------------------------------------------------------
# Build il-patcher (needed for --version reads below, and for every stage
# except Stage 5)
# ---------------------------------------------------------------------------

log "building il-patcher..."
mkdir -p "$WORKDIR/tools/il-patcher"
cp "$IL_PATCHES_DIR/il-patcher-Program.cs" "$WORKDIR/tools/il-patcher/Program.cs"
cat > "$WORKDIR/tools/il-patcher/il-patcher.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>il_patcher</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Mono.Cecil" Version="0.11.6" />
  </ItemGroup>
</Project>
EOF
dotnet build -c Release "$WORKDIR/tools/il-patcher" >"$WORKDIR/build-il-patcher.log" 2>&1 \
    || { cat "$WORKDIR/build-il-patcher.log" >&2; die "failed to build il-patcher (log above)"; }
ILP="dotnet $WORKDIR/tools/il-patcher/bin/Release/net10.0/il-patcher.dll"
log "il-patcher built."

# ---------------------------------------------------------------------------
# Find candidate bottles
# ---------------------------------------------------------------------------

shopt -s nullglob
BOTTLE_DLLS=()
for d in "$HOME"/.cxoffice/*/; do
    candidate="${d}drive_c/Program Files (x86)/eM Client/MailClient.dll"
    [[ -f "$candidate" ]] && BOTTLE_DLLS+=("$candidate")
done
shopt -u nullglob

[[ ${#BOTTLE_DLLS[@]} -gt 0 ]] || die "no eM Client install found under any ~/.cxoffice/*/drive_c/Program Files (x86)/eM Client/"

log "found ${#BOTTLE_DLLS[@]} eM Client install(s):"
declare -A BOTTLE_NAME_FOR_DLL
for dll in "${BOTTLE_DLLS[@]}"; do
    # bottle name = the path component right after .cxoffice/
    rel="${dll#"$HOME"/.cxoffice/}"
    bottle_name="${rel%%/*}"
    BOTTLE_NAME_FOR_DLL["$dll"]="$bottle_name"
    ver_line=$($ILP --version "$dll" 2>/dev/null | awk -F'\t' '{print $3}')
    log "  - $bottle_name  ($ver_line)"
done

if [[ $LIST_ONLY -eq 1 ]]; then
    exit 0
fi

# ---------------------------------------------------------------------------
# Select bottle
# ---------------------------------------------------------------------------

SELECTED_DLL=""
if [[ -n "$BOTTLE_OVERRIDE" ]]; then
    for dll in "${BOTTLE_DLLS[@]}"; do
        if [[ "${BOTTLE_NAME_FOR_DLL[$dll]}" == "$BOTTLE_OVERRIDE" ]]; then
            SELECTED_DLL="$dll"
            break
        fi
    done
    [[ -n "$SELECTED_DLL" ]] || die "--bottle $BOTTLE_OVERRIDE not found among the installs listed above"
elif [[ ${#BOTTLE_DLLS[@]} -eq 1 ]]; then
    SELECTED_DLL="${BOTTLE_DLLS[0]}"
else
    echo "Multiple bottles found. Pick one:"
    select dll in "${BOTTLE_DLLS[@]}"; do
        if [[ -n "${dll:-}" ]]; then SELECTED_DLL="$dll"; break; fi
    done
fi

BOTTLE_NAME="${BOTTLE_NAME_FOR_DLL[$SELECTED_DLL]}"
BOTTLE_APP_DIR="$(dirname "$SELECTED_DLL")"
log "target bottle: $BOTTLE_NAME ($BOTTLE_APP_DIR)"

# ---------------------------------------------------------------------------
# Already-patched check -- safe to re-run. Deliberately NOT a file-size or
# byte-diff check: file size isn't a reliable "already patched" signal
# across future eM Client releases (an unrelated app change could easily
# land on the same size we produce, or a future version of these same
# patches could land on a different size, independent of whether they're
# applied). Instead checks whether MailClient.dll references the
# MailClient.Licensing.BouncyCastlePatch assembly (il-patcher --check-patched)
# -- an assembly reference that cannot exist unless this pipeline created
# it, regardless of version or size. See that mode's doc comment in
# il-patcher-Program.cs for the full reasoning, including its one known
# limitation (doesn't independently verify every earlier stage is still
# intact if someone hand-reverted just one of them).
# ---------------------------------------------------------------------------

if [[ $FORCE -eq 0 ]] && $ILP --check-patched "$SELECTED_DLL" >/dev/null 2>&1; then
    log "already patched: $SELECTED_DLL references MailClient.Licensing.BouncyCastlePatch."
    log "nothing to do. Pass --force to attempt patching anyway (will very likely fail cleanly"
    log "at Stage 1's own shape-check, since the individual stages aren't self-reverting -- to"
    log "genuinely re-patch, restore from a releases/*/backups/ backup first, then re-run)."
    exit 0
fi

# ---------------------------------------------------------------------------
# Refuse to patch into a bottle whose MailClient.exe is still running: files
# could be locked mid-write, and a running instance would keep the old code
# in memory regardless of what lands on disk until it's restarted anyway.
# Matched by bottle name appearing anywhere in the process's environment
# (CrossOver's wine wrapper sets some bottle-identifying env var per launch
# -- WINEPREFIX or its own CX_BOTTLE, depending on how it was started; a
# loose substring match across the whole environment is more robust than
# guessing the exact variable name) rather than a system-wide "any
# MailClient.exe" check, so an instance running in a DIFFERENT bottle
# doesn't block this one.
# ---------------------------------------------------------------------------

find_running_pids_for_bottle() {
    local name="$1"
    local pid cmdline
    for p in /proc/[0-9]*; do
        pid="${p#/proc/}"
        [[ -r "$p/cmdline" ]] || continue
        cmdline="$(tr '\0' ' ' < "$p/cmdline" 2>/dev/null || true)"
        [[ "$cmdline" == *"MailClient.exe"* ]] || continue
        if [[ -r "$p/environ" ]] && tr '\0' '\n' < "$p/environ" 2>/dev/null | grep -q "$name"; then
            echo "$pid"
        fi
    done
}

RUNNING_PIDS=()
while IFS= read -r pid; do
    [[ -n "$pid" ]] && RUNNING_PIDS+=("$pid")
done < <(find_running_pids_for_bottle "$BOTTLE_NAME")

if [[ ${#RUNNING_PIDS[@]} -gt 0 ]]; then
    warn "eM Client appears to be running in bottle '$BOTTLE_NAME' (PID(s): ${RUNNING_PIDS[*]})."
    warn "It needs to be closed before patching."
    read -r -p "Close it now and continue? [Y/n] " reply
    if [[ -z "$reply" || "$reply" =~ ^[Yy]$ ]]; then
        # Graceful close (wmctrl -c, a WM_DELETE_WINDOW-style request) only -- never `kill` an
        # abruptly running instance: confirmed hands-on that an abrupt process kill leaves the
        # app's local database in a state that triggers a "wasn't closed correctly, checking for
        # corrupted database" recovery dialog on the next launch. Falls back to `kill` only if
        # wmctrl isn't available or finds no matching window, with a clear warning either way.
        graceful_closed=0
        if command -v wmctrl >/dev/null 2>&1; then
            while IFS= read -r wline; do
                wid="$(awk '{print $1}' <<<"$wline")"
                wpid="$(awk '{print $3}' <<<"$wline")"
                for pid in "${RUNNING_PIDS[@]}"; do
                    if [[ "$wpid" == "$pid" ]]; then
                        log "sending graceful close (wmctrl) to window $wid (PID $pid)..."
                        wmctrl -ic "$wid" 2>/dev/null && graceful_closed=1
                    fi
                done
            done < <(wmctrl -l -p 2>/dev/null)
        fi
        if [[ $graceful_closed -eq 0 ]]; then
            warn "no window found to close gracefully via wmctrl -- falling back to kill"
            warn "(this may trigger a DB-repair check on the next launch)."
            for pid in "${RUNNING_PIDS[@]}"; do
                kill "$pid" 2>/dev/null || true
            done
        fi
        for _ in $(seq 1 10); do
            sleep 1
            RUNNING_PIDS=()
            while IFS= read -r pid; do
                [[ -n "$pid" ]] && RUNNING_PIDS+=("$pid")
            done < <(find_running_pids_for_bottle "$BOTTLE_NAME")
            [[ ${#RUNNING_PIDS[@]} -eq 0 ]] && break
        done
        [[ ${#RUNNING_PIDS[@]} -eq 0 ]] || die "eM Client did not exit within 10s (still running: ${RUNNING_PIDS[*]}) -- close it manually and re-run."
        log "eM Client closed."
    else
        die "aborted -- close eM Client in this bottle and re-run."
    fi
fi

# ---------------------------------------------------------------------------
# Version gate
# ---------------------------------------------------------------------------

FOUND_VERSION_LINE=$($ILP --version "$SELECTED_DLL")
FOUND_FILE_VERSION=$(echo "$FOUND_VERSION_LINE" | awk -F'\t' '{print $3}' | sed 's/FileVersion=//')
log "installed MailClient.dll: $FOUND_VERSION_LINE"

if [[ "$FOUND_FILE_VERSION" != "$EXPECTED_FILE_VERSION" ]]; then
    warn "this release script was built and verified against FileVersion=$EXPECTED_FILE_VERSION,"
    warn "but the selected install reports FileVersion=$FOUND_FILE_VERSION."
    warn "The patches below look for specific types/methods/strings by name, not by version --"
    warn "they may still apply cleanly, or may fail loudly (and safely -- nothing gets deployed"
    warn "until every stage AND every verification check below succeeds). But an untested version"
    warn "mismatch is still a real risk: proceed at your own judgement."
    if [[ $ASSUME_YES -eq 1 ]]; then
        log "-y/--yes given, continuing despite version mismatch."
    else
        read -r -p "Continue anyway? [y/N] " reply
        [[ "$reply" =~ ^[Yy]$ ]] || die "aborted by user (version mismatch)."
    fi
fi

# ---------------------------------------------------------------------------
# Copy the found install into the workdir as Stage 0 input -- never operate
# on the live bottle directly until the final, verified deploy step.
# ---------------------------------------------------------------------------

log "copying installed assemblies into workdir (this is the 'original/' for this run)..."
mkdir -p "$WORKDIR/original"
cp -a "$BOTTLE_APP_DIR"/. "$WORKDIR/original/"

# ---------------------------------------------------------------------------
# Build license-oaep-patcher and the BouncyCastlePatch helper assembly.
# The helper's BouncyCastle.Cryptography.dll reference is pointed at the
# JUST-COPIED, install-sourced copy -- not this repo's own original/ -- so
# the deployed helper always links against the exact BouncyCastle build the
# target install actually ships, not a snapshot from this dev checkout.
# ---------------------------------------------------------------------------

log "building MailClient.Licensing.BouncyCastlePatch helper..."
mkdir -p "$WORKDIR/tools/BouncyCastlePatch"
cp "$IL_PATCHES_DIR/MailClient.Licensing.BouncyCastlePatch/OaepPatch.cs" "$WORKDIR/tools/BouncyCastlePatch/"
BC_DLL="$WORKDIR/original/BouncyCastle.Cryptography.dll"
[[ -f "$BC_DLL" ]] || die "BouncyCastle.Cryptography.dll not found in the target install -- can't build the license-Activation fix without it."
cat > "$WORKDIR/tools/BouncyCastlePatch/BouncyCastlePatch.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>MailClient.Licensing.BouncyCastlePatch</RootNamespace>
    <AssemblyName>MailClient.Licensing.BouncyCastlePatch</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="BouncyCastle.Cryptography">
      <HintPath>$BC_DLL</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
EOF
dotnet build -c Release "$WORKDIR/tools/BouncyCastlePatch" >"$WORKDIR/build-bouncycastlepatch.log" 2>&1 \
    || { cat "$WORKDIR/build-bouncycastlepatch.log" >&2; die "failed to build BouncyCastlePatch helper (log above)"; }
BOUNCYCASTLEPATCH_DLL="$WORKDIR/tools/BouncyCastlePatch/bin/Release/net8.0/MailClient.Licensing.BouncyCastlePatch.dll"

log "building license-oaep-patcher..."
mkdir -p "$WORKDIR/tools/license-oaep-patcher"
cp "$IL_PATCHES_DIR/license-oaep-patcher-Program.cs" "$WORKDIR/tools/license-oaep-patcher/Program.cs"
cat > "$WORKDIR/tools/license-oaep-patcher/license-oaep-patcher.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>license_oaep_patcher</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Mono.Cecil" Version="0.11.6" />
  </ItemGroup>
</Project>
EOF
dotnet build -c Release "$WORKDIR/tools/license-oaep-patcher" >"$WORKDIR/build-license-oaep-patcher.log" 2>&1 \
    || { cat "$WORKDIR/build-license-oaep-patcher.log" >&2; die "failed to build license-oaep-patcher (log above)"; }
ILP2="dotnet $WORKDIR/tools/license-oaep-patcher/bin/Release/net10.0/license-oaep-patcher.dll"
log "license-oaep-patcher built."

# ---------------------------------------------------------------------------
# ilspycmd, for the post-patch decompile sanity checks. Reuse a global
# install if present; otherwise install into the workdir so it's cleaned up
# with everything else.
# ---------------------------------------------------------------------------

if command -v ilspycmd >/dev/null 2>&1; then
    ILSPY="ilspycmd"
elif [[ -x "$HOME/.dotnet/tools/ilspycmd" ]]; then
    ILSPY="$HOME/.dotnet/tools/ilspycmd"
else
    log "ilspycmd not found -- installing a local copy into the workdir..."
    mkdir -p "$WORKDIR/ilspy-tools"
    dotnet tool install --tool-path "$WORKDIR/ilspy-tools" ilspycmd >"$WORKDIR/install-ilspy.log" 2>&1 \
        || { cat "$WORKDIR/install-ilspy.log" >&2; die "failed to install ilspycmd (log above)"; }
    ILSPY="$WORKDIR/ilspy-tools/ilspycmd"
fi
export DOTNET_ROOT="${DOTNET_ROOT:-$HOME/.dotnet}"
log "ilspycmd ready: $ILSPY"

# ---------------------------------------------------------------------------
# Run the patch pipeline (mirrors CLAUDE.md's "Patch pipeline" section --
# keep these two in sync if either changes)
# ---------------------------------------------------------------------------

log "Stage 1: interpolation mode..."
$ILP --patch "$IL_PATCHES_DIR/stage1-interpolation-mode.json" "$WORKDIR/original" "$WORKDIR/output"

log "Stage 2: AllPaintingInWmPaint..."
$ILP --patch-allpaintinginwmpaint "$WORKDIR/output" "$WORKDIR/output-stage2"

log "Stage 3: Settings panel Load-event fix..."
$ILP --patch-settings-refresh "$WORKDIR/output-stage2" "$WORKDIR/output-stage3-final"

log "Stage 4: category-click crash fix..."
$ILP --patch-default-client-notimpl "$WORKDIR/output-stage3-final" "$WORKDIR/output-stage4"

log "Stage 5: License Activate RSA-OAEP decrypt fix..."
mkdir -p "$WORKDIR/output-stage5"
$ILP2 "$WORKDIR/output-stage4/MailClient.dll" "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage5/MailClient.dll"
# -n/--no-clobber: fill in the rest of the tree (everything Stage 5 doesn't touch) without
# overwriting the MailClient.dll just patched above.
cp -an "$WORKDIR/output-stage4"/. "$WORKDIR/output-stage5/"
cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage5/"

log "Stage 6: license icon fix..."
$ILP --patch-license-icon "$WORKDIR/output-stage5" "$WORKDIR/output-stage6"
cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage6/"

log "Stage 7: splash-screen tip icon fix..."
$ILP --patch-splash-tip-icon "$WORKDIR/output-stage6" "$WORKDIR/output-final"
cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-final/"

FINAL_DIR="$WORKDIR/output-final"

# ---------------------------------------------------------------------------
# Verify -- every check CLAUDE.md documents, run automatically. Any failure
# aborts here, before the live bottle has been touched at all.
# ---------------------------------------------------------------------------

log "verifying..."

interp_line=$($ILP "$FINAL_DIR" | tail -1)
echo "$interp_line" | grep -q "Total InterpolationMode set-sites found: 13" \
    || die "verification failed: expected 13 total InterpolationMode set-sites, got: $interp_line"

$ILSPY -m "M:MailClient.UI.Forms.formSettings.formSettings_Load(System.Object,System.EventArgs)" "$FINAL_DIR/MailClient.dll" >/dev/null \
    || die "verification failed: formSettings_Load doc-id lookup failed"

$ILSPY -t "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid" "$FINAL_DIR/MailClient.Common.UI.dll" | grep -q AllPaintingInWmPaint \
    || die "verification failed: AllPaintingInWmPaint not found in ControlDataGrid"

oaep_hits=$($ILSPY -t "MailClient.Licensing.DecryptAndVerify" "$FINAL_DIR/MailClient.dll" | grep -c OaepPatch || true)
[[ "$oaep_hits" -ge 2 ]] || die "verification failed: expected >=2 OaepPatch call sites in DecryptAndVerify, found $oaep_hits"

$ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.Utils.Integration IsDefaultClientVista | tail -1 | grep -q "^OK:" \
    || die "verification failed: --dump-handlers reported a handler-ordering violation"

ORIG_SIZE=$(stat -c%s "$WORKDIR/output-stage5/MailClient.dll")
FINAL_SIZE=$(stat -c%s "$FINAL_DIR/MailClient.dll")
[[ "$ORIG_SIZE" -eq "$FINAL_SIZE" ]] \
    || die "verification failed: Stages 6-7 should not change MailClient.dll's byte size (was $ORIG_SIZE, now $FINAL_SIZE) -- .resources offset table may be corrupted"

log "all verification checks passed."

# ---------------------------------------------------------------------------
# Backup, then deploy -- with rollback if anything goes wrong partway
# through the copy.
# ---------------------------------------------------------------------------

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP_DIR="$BACKUPS_DIR/${BOTTLE_NAME}-${TIMESTAMP}"
mkdir -p "$BACKUP_DIR"

DEPLOY_FILES=(MailClient.dll MailClient.Common.UI.dll MailClient.Licensing.BouncyCastlePatch.dll)

log "backing up current files to $BACKUP_DIR ..."
for f in "${DEPLOY_FILES[@]}" MailClient.deps.json; do
    if [[ -f "$BOTTLE_APP_DIR/$f" ]]; then
        cp -a "$BOTTLE_APP_DIR/$f" "$BACKUP_DIR/$f"
    fi
done

WRITTEN_FILES=()
rollback() {
    err "deploy failed -- rolling back."
    for f in "${WRITTEN_FILES[@]}"; do
        if [[ -f "$BOTTLE_APP_DIR/$f" ]]; then
            mv "$BOTTLE_APP_DIR/$f" "$BOTTLE_APP_DIR/$f.new"
            warn "  moved partially-deployed $f aside as $f.new"
        fi
        if [[ -f "$BACKUP_DIR/$f" ]]; then
            cp -a "$BACKUP_DIR/$f" "$BOTTLE_APP_DIR/$f"
            warn "  restored $f from backup"
        fi
    done
    die "rollback complete -- the app should still launch on its previous files. Failed attempt's files (if any) are alongside as *.new for inspection."
}

log "deploying to $BOTTLE_APP_DIR ..."
for f in "${DEPLOY_FILES[@]}"; do
    if ! cp -a "$FINAL_DIR/$f" "$BOTTLE_APP_DIR/$f"; then
        WRITTEN_FILES+=("$f")
        rollback
    fi
    WRITTEN_FILES+=("$f")
done

WRITTEN_FILES+=("MailClient.deps.json")
if ! python3 "$IL_PATCHES_DIR/license-oaep-patcher-patch-deps-json.py" "$BOTTLE_APP_DIR/MailClient.deps.json"; then
    rollback
fi

log "deploy complete."
log "  bottle:  $BOTTLE_NAME"
log "  version: $FOUND_FILE_VERSION"
log "  backup:  $BACKUP_DIR"

# ---------------------------------------------------------------------------
# Optional: install vendored Windows fonts (Segoe UI, Tahoma, Calibri, etc.,
# under fonts/) into the bottle for better general font fidelity under Wine.
# Deliberately separate from the assembly patch pipeline above -- independent
# of it, and skipped entirely if fonts/ has no .ttf files. These are genuine
# Microsoft font files, not freely redistributable, so installing them
# requires confirming a valid license (unless --install-fonts/--no-fonts
# settles it non-interactively -- see header comment).
#
# NOTE: known to make genuine Segoe UI/Tahoma available for font resolution
# and may improve general text rendering fidelity, but confirmed (see
# reports/splash-tip-icon-findings.md) NOT to fix Wine's emoji-glyph
# rendering gap by itself -- that's a deeper issue in Wine's glyph-shaping
# code, not something registry configuration alone resolves. The splash-tip
# fix (Stage 7 above, already applied) does not depend on this running.
# ---------------------------------------------------------------------------

FONTS_DIR="$REPO_ROOT/fonts"
if [[ -d "$FONTS_DIR" ]] && compgen -G "$FONTS_DIR"/*.ttf >/dev/null; then
    font_count=$(ls "$FONTS_DIR"/*.ttf | wc -l)
    do_install_fonts=0
    if [[ $INSTALL_FONTS -eq 1 ]]; then
        do_install_fonts=1
    elif [[ $NO_FONTS -eq 1 ]]; then
        do_install_fonts=0
    else
        echo ""
        echo "This repo has $font_count Windows font files vendored under fonts/ (Segoe UI,"
        echo "Segoe UI Emoji, Tahoma, Calibri, etc.) for better font fidelity under Wine -- this"
        echo "bottle currently has none of them installed. These are genuine Microsoft fonts, not"
        echo "freely redistributable -- installing them requires a valid Windows font license."
        read -r -p "Do you hold a valid license for these fonts, and want them installed into this bottle? [y/N] " freply
        [[ "$freply" =~ ^[Yy]$ ]] && do_install_fonts=1
    fi

    if [[ $do_install_fonts -eq 1 ]]; then
        log "installing $font_count font(s) into the bottle..."
        BOTTLE_ROOT="$(cd "$BOTTLE_APP_DIR/../.." && pwd)"
        BOTTLE_FONTS_DIR="$BOTTLE_ROOT/windows/Fonts"
        cp "$FONTS_DIR"/*.ttf "$BOTTLE_FONTS_DIR/"

        mkdir -p "$WORKDIR/tools/font-systemlink-writer"
        cp "$IL_PATCHES_DIR/font-systemlink-writer/Program.cs" "$WORKDIR/tools/font-systemlink-writer/"
        cp "$IL_PATCHES_DIR/font-systemlink-writer/font-systemlink-writer.csproj" "$WORKDIR/tools/font-systemlink-writer/"
        if dotnet publish -c Release "$WORKDIR/tools/font-systemlink-writer" >"$WORKDIR/build-fontwriter.log" 2>&1; then
            FONTWRITER_EXE="$WORKDIR/tools/font-systemlink-writer/bin/Release/net8.0/win-x86/publish/font-systemlink-writer.exe"
            WIN_FONTWRITER_PATH="Z:$(echo "$FONTWRITER_EXE" | sed 's/\//\\/g')"
            # -u DOTNET_ROOT: a host-side DOTNET_ROOT (set by other tooling, e.g. for ilspycmd)
            # leaks through into the wine child process and gets reinterpreted via CrossOver's
            # own Y: drive mapping, pointing the bottle's own apphost at the HOST's dotnet
            # install instead of the bottle's -- confirmed hands-on ("hostfxr.dll could not be
            # found in [Y:\.dotnet\...]", Y: being CrossOver's mapping for $HOME). Must be
            # unset, not just left unexported, for hostfxr's resolution to use the bottle's own
            # C:\Program Files\dotnet installation as intended.
            if CX_BOTTLE="$BOTTLE_NAME" env -u DOTNET_ROOT /opt/cxoffice/bin/wine "$WIN_FONTWRITER_PATH"; then
                log "fonts registered and SystemLink fallback entries set."
            else
                warn "font-systemlink-writer failed -- font files were copied but not registered in the registry."
            fi
        else
            cat "$WORKDIR/build-fontwriter.log" >&2
            warn "failed to build font-systemlink-writer -- font files were copied but not registered in the registry."
        fi
    else
        log "skipping font install (no license confirmation given). Use --install-fonts to skip this prompt."
    fi
fi

log "Restart eM Client in this bottle to pick up the change."
