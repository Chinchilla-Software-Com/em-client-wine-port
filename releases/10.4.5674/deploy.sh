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
#   ./deploy.sh                     interactive: lists bottles found, prompts for choice + confirms
#   ./deploy.sh --bottle NAME       skip bottle selection (bottle dir name under ~/.cxoffice/)
#   ./deploy.sh --list              list found bottles and their installed versions, then exit
#   ./deploy.sh -y|--yes            don't prompt on a version mismatch, continue automatically
#   ./deploy.sh --force             skip the "already patched, nothing to do" short-circuit
#   ./deploy.sh --install-fonts     install the vendored fonts/ without the license-consent prompt
#   ./deploy.sh --no-fonts          skip font installation without the license-consent prompt
#   ./deploy.sh --install-associations   add file-type associations without the prompt
#   ./deploy.sh --no-associations        skip file-type associations without the prompt
#   ./deploy.sh --force-associations     also overwrite extensions that already have an association
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
# File-type associations: file-associations/*.reg fixes attachment types (office documents,
# images, archives, audio/video) that a fresh CrossOver bottle has no working association for --
# see reports/office-file-associations-findings.md. Only extensions with no existing association
# are added by default (so a bottle with a real Office/LibreOffice install isn't touched);
# --force-associations also overwrites extensions that already have something set.
# --install-associations / --no-associations answer the prompt non-interactively.
#
# Requires: dotnet SDK (checked below, prints install instructions if missing), python3 (for the
# deps.json patch step), network access (NuGet restore for Mono.Cecil, and ilspycmd if not
# already installed as a dotnet tool). Nothing else -- everything patch-related is built fresh
# each run into a temp directory that's cleaned up on success.

set -euo pipefail

RELEASE_VERSION="10.4.5674"
EXPECTED_FILE_VERSION="10.4.5674.0"

# This project's own release number against RELEASE_VERSION (see
# il-patches/MailClient.Wine/VersionMarker.cs) -- bump this, and add a row to
# REVISION_LAST_STAGE below, every time a new release ships against the same eM Client version.
# Tag as release/<RELEASE_VERSION>-<OUR_RELEASE_NUMBER> (release/10.4.5674-3 for this one).
OUR_RELEASE_NUMBER=3

# release number -> last stage number that release introduced. Drives the "resume mid-pipeline"
# logic below: a bottle already at revision N only needs stages after REVISION_LAST_STAGE[N]
# applied. 1 (release/10.4.5674, this project's first tagged release) predates
# MailClient.Wine.dll entirely -- detected via the older BouncyCastlePatch-only marker instead,
# see the revision-detection block below.
declare -A REVISION_LAST_STAGE=( [0]=0 [1]=7 [2]=14 [3]=15 )

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
INSTALL_ASSOCIATIONS=0
NO_ASSOCIATIONS=0
FORCE_ASSOCIATIONS=0

print_help() {
    sed -n '2,50p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --bottle) BOTTLE_OVERRIDE="${2:-}"; shift 2 ;;
        -y|--yes) ASSUME_YES=1; shift ;;
        --list) LIST_ONLY=1; shift ;;
        --force) FORCE=1; shift ;;
        --install-fonts) INSTALL_FONTS=1; shift ;;
        --no-fonts) NO_FONTS=1; shift ;;
        --install-associations) INSTALL_ASSOCIATIONS=1; shift ;;
        --no-associations) NO_ASSOCIATIONS=1; shift ;;
        --force-associations) FORCE_ASSOCIATIONS=1; shift ;;
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
# Revision check -- safe to re-run, and now upgrade-aware: a bottle already patched by an
# OLDER release of this same script gets only the stages it's missing applied on top of what's
# already there, not a doomed re-run of the whole pipeline from scratch (Stage 1 in particular
# would refuse -- it looks for the pristine InterpolationMode value and won't find it).
#
# Revision is read from MailClient.Wine.dll (il-patches/MailClient.Wine/VersionMarker.cs), a
# tiny marker assembly with no code, never loaded by eM Client itself -- just dropped in the
# install directory so this script can read its FileVersion back. That field's leading three
# components are the eM Client version this marker was built for (must match RELEASE_VERSION);
# the trailing component is OUR_RELEASE_NUMBER as of whichever deploy created it. Deliberately
# not a file-size or byte-diff check: size isn't a reliable "already patched" signal across
# future eM Client releases, and an assembly-reference check alone (the OLD approach, still used
# as a fallback below) can only ever answer "is Stage 5 applied", not "which release is this".
# ---------------------------------------------------------------------------

CURRENT_REVISION=0
MARKER_DLL="$BOTTLE_APP_DIR/MailClient.Wine.dll"
if [[ -f "$MARKER_DLL" ]]; then
    marker_line=$($ILP --version "$MARKER_DLL" 2>/dev/null || true)
    marker_file_version=$(echo "$marker_line" | awk -F'\t' '{print $3}' | sed 's/FileVersion=//')
    marker_emclient_version="${marker_file_version%.*}"
    marker_revision="${marker_file_version##*.}"
    if [[ "$marker_emclient_version" == "$RELEASE_VERSION" && "$marker_revision" =~ ^[0-9]+$ ]]; then
        CURRENT_REVISION="$marker_revision"
        log "found MailClient.Wine.dll: this install is at release $RELEASE_VERSION-$CURRENT_REVISION."
    else
        warn "MailClient.Wine.dll present but unreadable or for a different eM Client version"
        warn "($marker_file_version, expected $RELEASE_VERSION.N) -- ignoring it."
    fi
elif $ILP --check-patched "$SELECTED_DLL" >/dev/null 2>&1; then
    # Old marker (license fix applied) with no version-stamped MailClient.Wine.dll at all --
    # predates this mechanism, i.e. this project's first tagged release (release/10.4.5674, no
    # numeric suffix), treated as revision 1.
    CURRENT_REVISION=1
    log "found the pre-versioning license-fix marker only: this install is at release $RELEASE_VERSION-1 (legacy)."
fi

DLL_ALREADY_PATCHED=0
START_STAGE=1
if [[ $FORCE -eq 0 && "$CURRENT_REVISION" -ge "$OUR_RELEASE_NUMBER" ]]; then
    DLL_ALREADY_PATCHED=1
    log "already at release $RELEASE_VERSION-$CURRENT_REVISION (this script is release"
    log "$RELEASE_VERSION-$OUR_RELEASE_NUMBER) -- skipping the DLL patch pipeline (pass --force to"
    log "attempt patching anyway). Still checking fonts/file-associations below, since those are"
    log "independent of whether the DLL patches are applied."
elif [[ "$CURRENT_REVISION" -gt 0 && $FORCE -eq 0 ]]; then
    START_STAGE=$(( ${REVISION_LAST_STAGE[$CURRENT_REVISION]} + 1 ))
    log "install is at release $RELEASE_VERSION-$CURRENT_REVISION -- resuming from Stage $START_STAGE"
    log "(stages 1-$(( START_STAGE - 1 )) already applied, left as-is)."
fi

if [[ $DLL_ALREADY_PATCHED -eq 0 ]]; then

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
        if [[ ${#RUNNING_PIDS[@]} -gt 0 ]]; then
            # A graceful close CAN succeed (window disappears, wmctrl reports success) without
            # the process ever exiting -- confirmed hands-on: eM Client's own "Close application
            # to tray" setting (Settings > General) turns a close request into a minimize-to-tray,
            # not an exit. Already gave it a fair 10s chance above; fall back to kill now rather
            # than dying, same DB-repair caveat as the no-window case above.
            warn "eM Client is still running 10s after a graceful close request -- it may have"
            warn "minimized to tray instead of exiting (e.g. its own 'Close application to tray'"
            warn "setting). Falling back to kill (may trigger a DB-repair check on next launch)."
            for pid in "${RUNNING_PIDS[@]}"; do
                kill "$pid" 2>/dev/null || true
            done
            for _ in $(seq 1 10); do
                sleep 1
                RUNNING_PIDS=()
                while IFS= read -r pid; do
                    [[ -n "$pid" ]] && RUNNING_PIDS+=("$pid")
                done < <(find_running_pids_for_bottle "$BOTTLE_NAME")
                [[ ${#RUNNING_PIDS[@]} -eq 0 ]] && break
            done
            [[ ${#RUNNING_PIDS[@]} -eq 0 ]] || die "eM Client did not exit within 20s total (still running: ${RUNNING_PIDS[*]}) -- close it manually and re-run."
        fi
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
# ilspycmd runs as its own apphost binary (not `dotnet ilspycmd.dll`), so hostfxr needs
# DOTNET_ROOT to find the shared runtime. Don't hardcode ~/.dotnet -- that's only where
# dotnet-install.sh puts it; an apt/dnf/zypper/pacman package install lives elsewhere
# (e.g. /usr/lib/dotnet, /usr/share/dotnet) and DOTNET_ROOT is usually unset there because
# the `dotnet` command itself doesn't need it. Derive it from wherever `dotnet` (the one
# check_dotnet found on PATH) actually resolves to, symlinks and all, so this works for any
# install method. Only touch DOTNET_ROOT if the caller hasn't already set one themselves.
if [[ -z "${DOTNET_ROOT:-}" ]]; then
    DOTNET_BIN="$(readlink -f "$(command -v dotnet)")"
    export DOTNET_ROOT="$(dirname "$DOTNET_BIN")"
fi
log "ilspycmd ready: $ILSPY"

# ---------------------------------------------------------------------------
# Build MailClient.Wine.dll -- the version marker (see il-patches/MailClient.Wine/
# VersionMarker.cs and the revision-detection block above). Always built and deployed
# regardless of START_STAGE, including on a fresh install, so every install this script
# ever touches ends up with an accurate marker for next time.
# ---------------------------------------------------------------------------

log "building MailClient.Wine version marker (release $RELEASE_VERSION-$OUR_RELEASE_NUMBER)..."
mkdir -p "$WORKDIR/tools/MailClient.Wine"
cp "$IL_PATCHES_DIR/MailClient.Wine/MailClient.Wine.csproj" "$IL_PATCHES_DIR/MailClient.Wine/VersionMarker.cs" "$WORKDIR/tools/MailClient.Wine/"
dotnet build -c Release \
    -p:AssemblyVersion="$EXPECTED_FILE_VERSION" \
    -p:FileVersion="$RELEASE_VERSION.$OUR_RELEASE_NUMBER" \
    "$WORKDIR/tools/MailClient.Wine" >"$WORKDIR/build-mailclient-wine.log" 2>&1 \
    || { cat "$WORKDIR/build-mailclient-wine.log" >&2; die "failed to build MailClient.Wine marker (log above)"; }
MAILCLIENT_WINE_DLL="$WORKDIR/tools/MailClient.Wine/bin/Release/net8.0/MailClient.Wine.dll"
log "MailClient.Wine marker built."

# ---------------------------------------------------------------------------
# Run the patch pipeline (mirrors CLAUDE.md's "Patch pipeline" section --
# keep these two in sync if either changes)
# ---------------------------------------------------------------------------

if [[ $START_STAGE -le 1 ]]; then
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
    $ILP --patch-splash-tip-icon "$WORKDIR/output-stage6" "$WORKDIR/output-stage7"
    cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage7/"

    STAGE7_DIR="$WORKDIR/output-stage7"
else
    # Resuming past Stage 7: those stages are already baked into what's actually installed
    # ($WORKDIR/original is already a full, complete copy of it, made above) -- re-running them
    # would fail (Stage 1 in particular refuses when it doesn't find the pristine
    # InterpolationMode value it expects) and would be redundant even if it didn't.
    log "Stages 1-7 already applied (revision $CURRENT_REVISION) -- using the installed files as-is."
    STAGE7_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Stages 8-14: new-mail notification toast fixes (see CLAUDE.md's Status
# section, "Email notifications not displaying correctly until fade" -- one
# root problem, several visible symptoms, all fixed together as one chain).
# Order matters and is enforced by the tool itself (fails loudly on the wrong
# order rather than silently misapplying) -- this exact sequence was verified
# by a fresh rebuild from original/ straight through, decompiled-output-
# identical to what was live-tested end to end. Don't reorder without
# re-verifying the same way.
#
# Wrapped in the same START_STAGE guard as Stages 1-7, now that a revision 2
# (release/10.4.5674-2, these stages already applied) can genuinely resume
# straight to Stage 15 -- re-running an already-applied notification patch
# against its own output is exactly the "expected pristine, found already-
# patched, refuse" class of failure Stage 1 already guards against for
# InterpolationMode, and these stages don't all have that same guard built
# in, so skipping outright (not just risking a clean failure) is the safe
# choice here.
# ---------------------------------------------------------------------------

if [[ $START_STAGE -le 8 ]]; then
    log "Stage 8: notification click-dispatch fix (fired its handler twice per click)..."
    $ILP --patch-notification-click-resubscribe "$STAGE7_DIR" "$WORKDIR/output-stage8"

    log "Stage 9: notification layout fixes (content padding, avatar-title gap, title centering)..."
    $ILP --patch-notification-content-padding "$WORKDIR/output-stage8" "$WORKDIR/output-stage9a"
    $ILP --patch-notification-avatar-title-gap "$WORKDIR/output-stage9a" "$WORKDIR/output-stage9b"
    $ILP --patch-notification-title-singleline "$WORKDIR/output-stage9b" "$WORKDIR/output-stage9c"
    $ILP --patch-notification-title-vcenter-fix "$WORKDIR/output-stage9c" "$WORKDIR/output-stage9"

    log "Stage 10: notification empty-box-until-fade fix (title/content invisible until the toast started fading out)..."
    $ILP --patch-notification-text-in-bitmap "$WORKDIR/output-stage9" "$WORKDIR/output-stage10a"
    $ILP --patch-notification-refresh-on-content-change "$WORKDIR/output-stage10a" "$WORKDIR/output-stage10b"
    $ILP --patch-notification-periodic-reblit "$WORKDIR/output-stage10b" "$WORKDIR/output-stage10c"
    $ILP --patch-notification-suppress-self-text-only "$WORKDIR/output-stage10c" "$WORKDIR/output-stage10"

    log "Stage 11: notification close/settings icon visibility fix..."
    $ILP --patch-notification-icon-bitmap "$WORKDIR/output-stage10" "$WORKDIR/output-stage11a"
    $ILP --patch-notification-title-icon-clip "$WORKDIR/output-stage11a" "$WORKDIR/output-stage11"

    log "Stage 12: notification Light-theme bold-text fix..."
    $ILP --patch-notification-text-drawstring "$WORKDIR/output-stage11" "$WORKDIR/output-stage12"

    log "Stage 13: notification hover-pause/resume-fade fix..."
    $ILP --patch-notification-hover-forward "$WORKDIR/output-stage12" "$WORKDIR/output-stage13"

    log "Stage 14: notification reply/flag/delete/previous/next icon fix..."
    $ILP --patch-notification-toolbar-icons "$WORKDIR/output-stage13" "$WORKDIR/output-stage14"
    cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage14/"

    STAGE14_DIR="$WORKDIR/output-stage14"
else
    # Resuming past Stage 14: already baked into what's actually installed, same reasoning as
    # the Stage 1-7 skip above.
    log "Stages 8-14 already applied (revision $CURRENT_REVISION) -- using the installed files as-is."
    STAGE14_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Stage 15: sync freeze fix (see reports/exchange-sync-freeze-findings.md).
# AccountManager.SendAndReceiveAll's own sequential per-account loop, and
# Folder.Synchronize's own recursive per-folder walk, could each block
# whatever thread calls them (very often the UI thread -- the once-a-minute
# auto-sync timer, the "check for mail on startup" option, manual refresh,
# and the "download for offline use" folder-tree walk all call one or the
# other directly and synchronously) for up to 20 seconds per account/folder
# on Wine's GetAddrInfoExW, which can't honor a connection-timeout
# cancellation. Both patches touch MailClient.Accounts.dll only, dispatching
# onto a dedicated background Thread instead (not Task.Run -- see either
# patch's own doc comment for why: Task.Run's shared ThreadPool reintroduced
# a different stutter under concurrent load).
# ---------------------------------------------------------------------------

log "Stage 15: sync freeze fix (account-level sync + folder-tree sync both now run off the UI thread)..."
$ILP --patch-account-manager-sync-async "$STAGE14_DIR" "$WORKDIR/output-stage15a"
$ILP --patch-folder-sync-async "$WORKDIR/output-stage15a" "$WORKDIR/output-final"
cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-final/"
cp "$MAILCLIENT_WINE_DLL" "$WORKDIR/output-final/"

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

# Stages 6-7 are raw resource byte edits and must not change file size (see their own header
# comments) -- checked against output-stage7 specifically, not FINAL_DIR, since the notification
# stages after it are real IL insertions that legitimately grow the file. Only meaningful (and
# only ran) when Stages 1-7 actually ran this time -- skipped entirely when resuming past them.
if [[ $START_STAGE -le 1 ]]; then
    STAGE5_SIZE=$(stat -c%s "$WORKDIR/output-stage5/MailClient.dll")
    STAGE7_SIZE=$(stat -c%s "$WORKDIR/output-stage7/MailClient.dll")
    [[ "$STAGE5_SIZE" -eq "$STAGE7_SIZE" ]] \
        || die "verification failed: Stages 6-7 should not change MailClient.dll's byte size (was $STAGE5_SIZE, now $STAGE7_SIZE) -- .resources offset table may be corrupted"
fi

$ILSPY -t "MailClient.UI.Forms.NotificationForms.FormGenericNotification" "$FINAL_DIR/MailClient.dll" | grep -q "__drawNotificationTextIntoBitmap" \
    || die "verification failed: __drawNotificationTextIntoBitmap not found -- notification text-in-bitmap fix missing"

$ILSPY -t "MailClient.UI.Forms.NotificationForms.FormMailNotification" "$FINAL_DIR/MailClient.dll" | grep -q "RaisePaint" \
    || die "verification failed: FormMailNotification doesn't reference RaisePaint -- notification toolbar-icons fix missing"

$ILSPY -t "MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton" "$FINAL_DIR/MailClient.Common.UI.dll" | grep -q "public void RaisePaint" \
    || die "verification failed: ControlToolStripButton.RaisePaint not found or not public -- notification toolbar-icons fix missing"

$ILSPY -t "MailClient.Accounts.AccountManager" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__syncTaskEntry" \
    || die "verification failed: AccountManager.__syncTaskEntry not found -- sync freeze fix missing"

$ILSPY -t "MailClient.Storage.Application.Folder" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__folderSyncTaskEntry" \
    || die "verification failed: Folder.__folderSyncTaskEntry not found -- sync freeze fix missing"

$ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Accounts.AccountManager __syncTaskEntry 2>&1 | grep -q "^OK:" \
    || die "verification failed: --dump-handlers reported a handler-ordering violation in AccountManager.__syncTaskEntry"

$ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Storage.Application.Folder __folderSyncTaskEntry 2>&1 | grep -q "^OK:" \
    || die "verification failed: --dump-handlers reported a handler-ordering violation in Folder.__folderSyncTaskEntry"

log "all verification checks passed."

# ---------------------------------------------------------------------------
# Backup, then deploy -- with rollback if anything goes wrong partway
# through the copy.
# ---------------------------------------------------------------------------

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP_DIR="$BACKUPS_DIR/${BOTTLE_NAME}-${TIMESTAMP}"
mkdir -p "$BACKUP_DIR"

DEPLOY_FILES=(MailClient.dll MailClient.Common.UI.dll MailClient.Accounts.dll MailClient.Licensing.BouncyCastlePatch.dll MailClient.Wine.dll)

log "backing up current files to $BACKUP_DIR ..."
for f in "${DEPLOY_FILES[@]}" MailClient.deps.json; do
    if [[ -f "$BOTTLE_APP_DIR/$f" ]]; then
        cp -a "$BOTTLE_APP_DIR/$f" "$BACKUP_DIR/$f"
    fi
done

# cp -a preserves mode/ownership/timestamps -- fine for a normal same-machine bottle under
# ~/.cxoffice/, but a bottle reached over a mounted/shared filesystem (e.g. a different
# machine's bottle mounted read-write for remote debugging) can reject the timestamp-preserving
# utime() call even with full read/write permission on the file's contents ("cp: preserving
# times ... Operation not permitted"), since the mount may enforce ownership separately from the
# permission bits it otherwise honors. The file content write itself isn't the problem -- only
# the attribute-preservation step is -- so fall back to a plain content-only `cp` rather than
# failing the whole deploy over a cosmetic timestamp.
cp_deploy() {
    cp -a "$1" "$2" 2>/dev/null && return 0
    cp "$1" "$2"
}

WRITTEN_FILES=()
rollback() {
    err "deploy failed -- rolling back."
    for f in "${WRITTEN_FILES[@]}"; do
        if [[ -f "$BOTTLE_APP_DIR/$f" ]]; then
            mv "$BOTTLE_APP_DIR/$f" "$BOTTLE_APP_DIR/$f.new"
            warn "  moved partially-deployed $f aside as $f.new"
        fi
        if [[ -f "$BACKUP_DIR/$f" ]]; then
            cp_deploy "$BACKUP_DIR/$f" "$BOTTLE_APP_DIR/$f"
            warn "  restored $f from backup"
        fi
    done
    die "rollback complete -- the app should still launch on its previous files. Failed attempt's files (if any) are alongside as *.new for inspection."
}

log "deploying to $BOTTLE_APP_DIR ..."
for f in "${DEPLOY_FILES[@]}"; do
    if ! cp_deploy "$FINAL_DIR/$f" "$BOTTLE_APP_DIR/$f"; then
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

fi  # DLL_ALREADY_PATCHED

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

# ---------------------------------------------------------------------------
# Optional: file-type associations (file-associations/*.reg) for attachment
# types eM Client can't launch an external viewer for because the bottle has
# no working association for them -- see
# reports/office-file-associations-findings.md. Confirmed root cause: eM
# Client's own attachment-open logic (MailClient.UI.ShellInterop.OpenItem /
# UIUtils.OpenFileInDefaultApp) only has special internal handling for a
# handful of its own item types (.eml/.msg/.emlt/.oft/.vcf/.ics/.asc/.crt/
# .cer/.note/.pst/.emdf); everything else -- office documents, images,
# archives, audio, video -- depends entirely on the bottle's own OS-level
# file association, same as Explorer would use. A fresh CrossOver bottle
# ships a working .pdf association out of the box but not office documents
# (confirmed missing for .docx) or several common attachment types (images
# like .bmp/.tif/.webp/.heic, archives, audio, video -- confirmed empty or
# entirely absent). Each fix .reg file's own header comment has the full
# story; this section applies whichever of file-associations/*.reg exist.
#
# Deliberately NOT a blind `wine regedit /S` of the whole file: that would
# unconditionally overwrite every extension's association, including one a
# real Office/LibreOffice install already set up correctly in this bottle.
# Instead: parse each .reg file into its individual per-extension blocks
# (file-associations/parse-reg-associations.py), check each extension
# against THIS bottle's live registry via `wine reg query`, and only import
# the blocks for extensions with no existing association (a missing key, or
# a present key whose default value is "(value not set)" -- e.g. .svg,
# whose ProgID pointed nowhere; see common-attachments.reg's own comment).
# --force-associations also overwrites extensions that already have
# something set. Per the project's own stated preference: this only checks
# whether the extension already has *a* value, not what it actually points
# to or whether that target is still valid -- simple and fast, at the cost
# of not distinguishing "already correctly associated" from "associated
# with something broken" (--force-associations is the escape hatch for the
# latter).
# ---------------------------------------------------------------------------

ASSOC_FILES=("$REPO_ROOT/file-associations/office-associations.reg" "$REPO_ROOT/file-associations/common-attachments.reg")
ASSOC_FILES_PRESENT=()
for f in "${ASSOC_FILES[@]}"; do [[ -f "$f" ]] && ASSOC_FILES_PRESENT+=("$f"); done

if [[ ${#ASSOC_FILES_PRESENT[@]} -gt 0 ]]; then
    do_associations=0
    if [[ $FORCE_ASSOCIATIONS -eq 1 || $INSTALL_ASSOCIATIONS -eq 1 ]]; then
        do_associations=1
    elif [[ $NO_ASSOCIATIONS -eq 1 ]]; then
        do_associations=0
    else
        echo ""
        echo "This repo has file-type association fixes under file-associations/ for"
        echo "attachment types eM Client can't open otherwise (office documents, images,"
        echo "archives, audio/video) -- see reports/office-file-associations-findings.md."
        echo "Only extensions with no existing association in this bottle are touched."
        read -r -p "Add missing file-type associations to this bottle? [Y/n] " areply
        [[ -z "$areply" || "$areply" =~ ^[Yy]$ ]] && do_associations=1
    fi

    if [[ $do_associations -eq 1 ]]; then
        log "checking file-type associations against $BOTTLE_NAME's registry..."
        MERGED_REG="$WORKDIR/associations-merged.reg"
        printf 'Windows Registry Editor Version 5.00\r\n\r\n' > "$MERGED_REG"

        added=0
        skipped=0
        forced=0
        while IFS=$'\t' read -r ext block_b64; do
            # `wine reg query` exits non-zero for a genuinely-missing key -- expected, not an
            # error, but pipefail (set above) would otherwise propagate that through this
            # substitution and abort the whole script via set -e. || true absorbs it; $existing
            # is correctly empty in that case either way.
            existing=$( (CX_BOTTLE="$BOTTLE_NAME" /opt/cxoffice/bin/wine reg query "HKCR\\.$ext" /ve 2>/dev/null \
                | sed -n 's/.*REG_SZ *//p' | tr -d '\r') || true)
            if [[ -n "$existing" && "$existing" != "(value not set)" ]]; then
                if [[ $FORCE_ASSOCIATIONS -eq 1 ]]; then
                    echo "$block_b64" | base64 -d >> "$MERGED_REG"
                    printf '\r\n' >> "$MERGED_REG"
                    forced=$((forced + 1))
                else
                    skipped=$((skipped + 1))
                fi
            else
                echo "$block_b64" | base64 -d >> "$MERGED_REG"
                printf '\r\n' >> "$MERGED_REG"
                added=$((added + 1))
            fi
        done < <(for f in "${ASSOC_FILES_PRESENT[@]}"; do python3 "$REPO_ROOT/file-associations/parse-reg-associations.py" "$f"; done)

        log "file associations: $added new, $forced forced-overwrite, $skipped already present (unchanged)."

        if [[ $((added + forced)) -gt 0 ]]; then
            WIN_MERGED_REG="Z:$(echo "$MERGED_REG" | sed 's/\//\\/g')"
            if CX_BOTTLE="$BOTTLE_NAME" /opt/cxoffice/bin/wine regedit /S "$WIN_MERGED_REG"; then
                log "file-type associations imported."
            else
                warn "wine regedit import of file-type associations failed -- bottle's associations may be partially updated."
            fi
        else
            log "no file-type association changes needed."
        fi
    else
        log "skipping file-type associations. Use --install-associations to skip this prompt."
    fi
fi

log "Restart eM Client in this bottle to pick up the change."
