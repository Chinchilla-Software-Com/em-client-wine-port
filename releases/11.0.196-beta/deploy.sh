#!/usr/bin/env bash
# Deploys the CrossOver stabilization patches for eM Client 11 (beta) to a live CrossOver
# bottle. See ../../CLAUDE.md for the project overview and ../10.4.5674/deploy.sh for the more
# mature sibling pipeline this one deliberately does NOT share code with -- eM Client 11 is a
# genuinely different, still-beta product build (different assembly set, different bugs), and
# this script's own version gate (see EXPECTED_FILE_VERSION below) keeps the two pipelines from
# ever being run against the wrong bottle by accident.
#
# Always regenerates the patch fresh from whatever assemblies are actually installed --
# deliberately never copies pre-built DLLs out of this repo (see the sibling script's own header
# comment for why: an update-prone app makes a stale pre-built copy actively dangerous). Nothing
# is deployed until every stage AND every verification check below succeeds.
#
# This script is versioned per eM Client release: it lives at releases/<version>/deploy.sh and
# is built and tested against that exact version (see EXPECTED_FILE_VERSION below). When this
# beta updates to a new build, don't edit this file in place -- copy this whole
# releases/<version>/ folder to a new releases/<new-version>/, retest by hand (same process as
# CLAUDE.md's "Investigation method"), and adjust whatever patch logic broke.
#
# Usage:
#   ./deploy.sh                     interactive: lists bottles found, prompts for choice + confirms
#   ./deploy.sh --bottle NAME       skip bottle selection (bottle dir name under ~/.cxoffice/)
#   ./deploy.sh --list              list found bottles and their installed versions, then exit
#   ./deploy.sh -y|--yes            don't prompt on a version mismatch, continue automatically
#   ./deploy.sh --force             skip the "already at this revision" short-circuit
#   ./deploy.sh --install-fonts     install the vendored fonts/ without the license-consent prompt
#   ./deploy.sh --no-fonts          skip font installation without the license-consent prompt
#
# Safe to re-run: reads a MailClient.Wine.dll version marker (same mechanism as the 10.4.5674
# pipeline -- see il-patches/MailClient.Wine/VersionMarker.cs) to tell whether this bottle is
# already at this release, and skips the patch pipeline entirely if so. Fonts (below) are
# independent of the DLL patch pipeline and always still checked, same as the sibling script.
#
# Fonts: if fonts/*.ttf exist in this repo (genuine Microsoft fonts -- Segoe UI, Tahoma, Calibri
# -- vendored by whoever holds a valid license to use them; see reports/splash-tip-icon-findings.md),
# the script asks whether you hold a license and want them installed into the target bottle
# before deploying. --install-fonts / --no-fonts answer that non-interactively -- for scripted or
# repeated runs (e.g. a periodic check) where a prompt can't be answered by a human.
#
# Requires: dotnet SDK (checked below, prints install instructions if missing). No python3 --
# unlike the 10.4.5674 pipeline's Stage 5, this fix adds no new sibling assembly, so there's no
# MailClient.deps.json edit needed. No network access needed beyond ilspycmd/NuGet restore
# (same as the sibling script).

set -euo pipefail

RELEASE_VERSION="11.0.196"
EXPECTED_FILE_VERSION="11.0.196.0"

# This project's own release number against RELEASE_VERSION (see
# il-patches/MailClient.Wine/VersionMarker.cs) -- bump this, and add a row to
# REVISION_LAST_STAGE below, every time a new release ships against the same eM Client version.
# Tag as release/<RELEASE_VERSION>-<OUR_RELEASE_NUMBER> (release/11.0.196-4 for this one).
OUR_RELEASE_NUMBER=4

# release number -> last stage number that release introduced. Same "resume mid-pipeline" logic
# as the 10.4.5674 script.
declare -A REVISION_LAST_STAGE=( [0]=0 [1]=1 [2]=2 [3]=3 [4]=4 )

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
    sed -n '2,40p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
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
# dotnet check
# ---------------------------------------------------------------------------

check_dotnet() {
    command -v dotnet >/dev/null 2>&1 && return 0
    err "dotnet SDK not found -- required to build the patch tool and read assembly versions."
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

check_dotnet

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
# Build il-patcher
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
# Find candidate bottles -- same classic install path shape as eM Client 10.x
# (confirmed against the beta build: drive_c/Program Files (x86)/eM Client/MailClient.dll).
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
# Version gate -- deliberately BEFORE anything else touches this bottle (revision check, and
# especially the running-instance close-and-kill logic below): a pure metadata read, no side
# effects, so the user gets a chance to abort a version mismatch before their running eM Client
# is ever closed. Same fix applied to the 10.4.5674 sibling script, which had the same ordering
# issue.
# ---------------------------------------------------------------------------

FOUND_VERSION_LINE=$($ILP --version "$SELECTED_DLL")
FOUND_FILE_VERSION=$(echo "$FOUND_VERSION_LINE" | awk -F'\t' '{print $3}' | sed 's/FileVersion=//')
log "installed MailClient.dll: $FOUND_VERSION_LINE"

if [[ "$FOUND_FILE_VERSION" != "$EXPECTED_FILE_VERSION" ]]; then
    warn "this release script was built and verified against FileVersion=$EXPECTED_FILE_VERSION,"
    warn "but the selected install reports FileVersion=$FOUND_FILE_VERSION."
    warn "The patch below finds its target call sites by exact method signature, not by version --"
    warn "it may still apply cleanly, or may fail loudly (and safely -- nothing gets deployed"
    warn "until every stage AND every verification check below succeeds). But an untested version"
    warn "mismatch is still a real risk, and this is beta software that changes fast: proceed at"
    warn "your own judgement."
    if [[ $ASSUME_YES -eq 1 ]]; then
        log "-y/--yes given, continuing despite version mismatch."
    else
        read -r -p "Continue anyway? [y/N] " reply
        [[ "$reply" =~ ^[Yy]$ ]] || die "aborted by user (version mismatch)."
    fi
fi

# ---------------------------------------------------------------------------
# Revision check -- MailClient.Wine.dll marker, same mechanism (and same tracked source,
# il-patches/MailClient.Wine/VersionMarker.cs) as the 10.4.5674 pipeline, just built with this
# release line's own version numbers. No legacy-marker fallback needed here: unlike 10.4.5674,
# this is the FIRST release for this eM Client version, so there's no pre-marker history to
# account for -- a bottle with no marker, or a marker for a different eM Client version, is
# simply revision 0.
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
        warn "MailClient.Wine.dll present but for a different eM Client version"
        warn "($marker_file_version, expected $RELEASE_VERSION.N) -- ignoring it, treating as revision 0."
    fi
fi

DLL_ALREADY_PATCHED=0
START_STAGE=1
if [[ $FORCE -eq 0 && "$CURRENT_REVISION" -ge "$OUR_RELEASE_NUMBER" ]]; then
    DLL_ALREADY_PATCHED=1
    log "already at release $RELEASE_VERSION-$CURRENT_REVISION (this script is release"
    log "$RELEASE_VERSION-$OUR_RELEASE_NUMBER) -- skipping the DLL patch pipeline (pass --force to"
    log "attempt patching anyway)."
elif [[ "$CURRENT_REVISION" -gt 0 && $FORCE -eq 0 ]]; then
    START_STAGE=$(( ${REVISION_LAST_STAGE[$CURRENT_REVISION]} + 1 ))
    log "install is at release $RELEASE_VERSION-$CURRENT_REVISION -- resuming from Stage $START_STAGE"
    log "(stages 1-$(( START_STAGE - 1 )) already applied, left as-is)."
fi

if [[ $DLL_ALREADY_PATCHED -eq 0 ]]; then

# ---------------------------------------------------------------------------
# Refuse to patch into a bottle whose MailClient.exe is still running (same reasoning and same
# graceful-close-first logic as the 10.4.5674 pipeline -- see its own header comment for why an
# abrupt kill is avoided).
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
            warn "eM Client is still running 10s after a graceful close request -- it may have"
            warn "minimized to tray instead of exiting. Falling back to kill (may trigger a"
            warn "DB-repair check on next launch)."
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
# Copy the found install into the workdir -- never operate on the live bottle directly until
# the final, verified deploy step.
# ---------------------------------------------------------------------------

log "copying installed assemblies into workdir..."
mkdir -p "$WORKDIR/original"
cp -a "$BOTTLE_APP_DIR"/. "$WORKDIR/original/"

# ---------------------------------------------------------------------------
# ilspycmd, for the post-patch decompile sanity checks.
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
if [[ -z "${DOTNET_ROOT:-}" ]]; then
    DOTNET_BIN="$(readlink -f "$(command -v dotnet)")"
    export DOTNET_ROOT="$(dirname "$DOTNET_BIN")"
fi
log "ilspycmd ready: $ILSPY"

# ---------------------------------------------------------------------------
# Build MailClient.Wine.dll -- the version marker (same tracked source as the 10.4.5674
# pipeline, il-patches/MailClient.Wine/VersionMarker.cs -- see its own doc comment). Built here
# with THIS release line's own version numbers: AssemblyVersion is this eM Client build's
# FileVersion (11.0.196.0), FileVersion's leading three components match that with the 4th
# being OUR_RELEASE_NUMBER (11.0.196.1 for this release) -- the exact same scheme the 10.4.5674
# pipeline uses, just against a different base version, so the same revision-detection logic
# above (and in any future Stage 2+ this version gets) keeps working unchanged.
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
log "MailClient.Wine marker built ($RELEASE_VERSION.$OUR_RELEASE_NUMBER)."

# ---------------------------------------------------------------------------
# Stage 1: PBKDF2 startup crash fix (see reports/emclient11-pbkdf2-startup-crash-findings.md).
# .NET 10's new static Rfc2898DeriveBytes.Pbkdf2(...) always routes through native CNG on
# Windows, and Wine's bcrypt.dll throws CryptographicException("Unknown error (0xc1000008)") the
# instant it's called -- confirmed universal via a standalone repro exe, unrelated to any
# app-specific password/salt/iteration values. This blocks app startup entirely (it's on the
# InitOnBackground path, deriving the local-cache/master-password AES key). The old,
# instance-based Rfc2898DeriveBytes(...).GetBytes(n) API works fine under the same Wine build --
# same algorithm, same assembly, just a different (if "obsolete") entry point -- so the fix
# swaps one `call` for a `newobj`+`callvirt` pair, no new assembly, no deps.json edit needed.
#
# --patch-pbkdf2-instance-api scans every MailClient*.dll for this exact call shape rather than
# hardcoding one type/method, since getting past the first crash site (AESEncryptor) surfaced a
# second, independent one (FileBasedCache<T>.Initialize, in a different assembly) -- see the
# findings report for the full story. It found and fixed 3 call sites in this build (2 in
# MailClient.dll, 1 in MailClient.Abstractions.dll); a future eM Client 11 build could have more
# or fewer -- the tool fails loudly if it finds zero, but doesn't hardcode an exact count.
# ---------------------------------------------------------------------------

if [[ $START_STAGE -le 1 ]]; then
    log "Stage 1: PBKDF2 startup crash fix (bcrypt-backed static Pbkdf2 -> working instance API)..."
    $ILP --patch-pbkdf2-instance-api "$WORKDIR/original" "$WORKDIR/output-stage1"
    STAGE1_DIR="$WORKDIR/output-stage1"
else
    # Resuming past Stage 1: already baked into what's actually installed ($WORKDIR/original is
    # already a full, complete copy of it) -- re-running would be redundant (and, unlike Stage 1
    # of the 10.4.5674 pipeline, this patch has no "expected pristine, found already-patched"
    # guard of its own, so a re-run wouldn't even fail loudly -- skipping outright is the safe
    # choice, same reasoning as the sibling script's own post-hoc stage guards).
    log "Stage 1 already applied (revision $CURRENT_REVISION) -- using the installed files as-is."
    STAGE1_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Stage 2: splash screen tip label tofu-box fix. Same exact bug and same exact fix as the
# 10.4.5674 pipeline's Stage 7 (see reports/splash-tip-icon-findings.md there for the full
# investigation) -- FormSplashScreen.labelTip's baseline Text resource
# (MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text) is the same genuine emoji,
# U+1F4A1 (light bulb), Wine has no glyph for, rendering as two tofu boxes on every launch.
# Confirmed identical root cause here, not just a similar-looking symptom: extracted this
# build's own labelTip.Text resource and found the exact same 4-byte UTF-8 sequence
# (F0 9F 92 A1), nothing else, byte-for-byte the same shape the v10 fix targets. Reuses the
# EXACT SAME il-patcher flag, --patch-splash-tip-icon, unmodified -- no new patch needed, this
# assembly's FormSplashScreen.resources container has the same structure. Verified via a direct
# dry run against this build's own MailClient.dll before wiring in here: byte-size-identical
# output (confirms the .resources offset table wasn't disturbed) and the resource re-extracts as
# C2 A0 C2 A0 (two non-breaking spaces) afterward.
# ---------------------------------------------------------------------------

if [[ $START_STAGE -le 2 ]]; then
    log "Stage 2: splash-screen tip label icon fix (same tofu-box bug/fix as the 10.4.5674 pipeline)..."
    $ILP --patch-splash-tip-icon "$STAGE1_DIR" "$WORKDIR/output-stage2"
    STAGE2_DIR="$WORKDIR/output-stage2"
else
    log "Stage 2 already applied (revision $CURRENT_REVISION) -- using the installed files as-is."
    STAGE2_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Stage 3: new-mail notification toast empty-until-fade fix -- the exact same bug and same
# 7-part fix as the 10.4.5674 pipeline's Stages 8-14 (see
# reports/notification-empty-until-fade-findings.md there, and
# reports/emclient11-notification-empty-until-fade-findings.md for this version's own
# confirmation that the root cause is genuinely identical, not just similar-looking). Folded into
# ONE stage here (rather than seven, like the sibling script) since this is the first time it's
# being applied to this release line -- there's no earlier partial-application history to resume
# from mid-chain the way 10.4.5674 needed to.
#
# 5 of the 7 sub-flags apply completely unmodified. Two needed real changes, both made
# version-agnostic (auto-detecting either eM Client version's assembly shape) rather than
# hardcoded to 11.0.196-beta specifically, so the SAME il-patcher flags keep working for the
# 10.4.5674 pipeline too:
#   - --patch-notification-hover-forward: LayeredForm moved from MailClient.dll into
#     MailClient.Common.UI.dll in this version (namespace MailClient.Common.UI.Forms instead of
#     MailClient.UI.Forms) -- the flag now resolves it from the `layeredWindow` field's own
#     declared type instead of a hardcoded assembly/namespace, and writes back whichever
#     assembly actually turns out to declare it.
#   - --patch-notification-toolbar-icons: FormMailNotification's toolbar buttons were redesigned
#     from 3 fixed named buttons (button_Reply/button_Flag/button_Delete) to 5 dynamic
#     button_action0..4 fields sharing one click handler -- the flag now tries both known button
#     shapes and adapts its generated code (including a real correctness fix: the click-dispatch
#     code must pass the clicked BUTTON as `sender`, not the form, since the shared handler reads
#     sender.Tag) to whichever one it finds.
# Order matters and is enforced by the tool itself in several places (fails loudly rather than
# silently misapplying), same as the sibling script's own Stages 8-14 -- don't reorder without
# re-verifying the same way (decompile + --dump-handlers on every touched method, then a full
# fresh-from-pristine chain re-run).
# ---------------------------------------------------------------------------

if [[ $START_STAGE -le 3 ]]; then
    log "Stage 3: notification toast empty-until-fade fix (same bug/fix as the 10.4.5674 pipeline's Stages 8-14)..."
    $ILP --patch-notification-click-resubscribe "$STAGE2_DIR" "$WORKDIR/output-stage3a"
    $ILP --patch-notification-content-padding "$WORKDIR/output-stage3a" "$WORKDIR/output-stage3b"
    $ILP --patch-notification-avatar-title-gap "$WORKDIR/output-stage3b" "$WORKDIR/output-stage3c"
    $ILP --patch-notification-title-singleline "$WORKDIR/output-stage3c" "$WORKDIR/output-stage3d"
    $ILP --patch-notification-title-vcenter-fix "$WORKDIR/output-stage3d" "$WORKDIR/output-stage3e"
    $ILP --patch-notification-text-in-bitmap "$WORKDIR/output-stage3e" "$WORKDIR/output-stage3f"
    $ILP --patch-notification-refresh-on-content-change "$WORKDIR/output-stage3f" "$WORKDIR/output-stage3g"
    $ILP --patch-notification-periodic-reblit "$WORKDIR/output-stage3g" "$WORKDIR/output-stage3h"
    $ILP --patch-notification-suppress-self-text-only "$WORKDIR/output-stage3h" "$WORKDIR/output-stage3i"
    $ILP --patch-notification-icon-bitmap "$WORKDIR/output-stage3i" "$WORKDIR/output-stage3j"
    $ILP --patch-notification-title-icon-clip "$WORKDIR/output-stage3j" "$WORKDIR/output-stage3k"
    $ILP --patch-notification-text-drawstring "$WORKDIR/output-stage3k" "$WORKDIR/output-stage3l"
    $ILP --patch-notification-hover-forward "$WORKDIR/output-stage3l" "$WORKDIR/output-stage3m"
    $ILP --patch-notification-toolbar-icons "$WORKDIR/output-stage3m" "$WORKDIR/output-final"
    STAGE3_DIR="$WORKDIR/output-final"
else
    log "Stage 3 already applied (revision $CURRENT_REVISION) -- using the installed files as-is."
    STAGE3_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Stage 4: Exchange sync-freeze fix -- the exact same bug and same fix as the 10.4.5674
# pipeline's Stage 15 (see reports/exchange-sync-freeze-findings.md there). Confirmed identical
# root cause here, not just a similar-looking symptom, before porting anything: decompile-diffed
# both target methods against the 10.4.5674 build and found them byte-for-byte structurally
# identical (MailClient.Accounts.AccountManager.SendAndReceiveAll and
# MailClient.Storage.Application.Folder.Synchronize(bool,bool) -- both still fully synchronous,
# both still reachable from the UI thread, no dedicated-thread dispatch of their own). Both
# `--patch-account-manager-sync-async` and `--patch-folder-sync-async` applied cleanly to this
# build's MailClient.Accounts.dll completely UNMODIFIED -- no adaptation needed at all (unlike
# Stage 3's two adapted sub-flags), verified via decompile (both the new dispatch wrappers and
# the moved-body `__Run*Core`/`__folderSyncTaskEntry` methods read correctly) and
# --dump-handlers (both add a `try`/`catch(Exception)`, both report correct nesting). Order
# between the two flags doesn't matter (different types, no shared state), same as the sibling
# script's own Stage 15.
# ---------------------------------------------------------------------------

if [[ $START_STAGE -le 4 ]]; then
    log "Stage 4: Exchange sync-freeze fix (same bug/fix as the 10.4.5674 pipeline's Stage 15)..."
    $ILP --patch-account-manager-sync-async "$STAGE3_DIR" "$WORKDIR/output-stage4a"
    $ILP --patch-folder-sync-async "$WORKDIR/output-stage4a" "$WORKDIR/output-final"
    cp "$MAILCLIENT_WINE_DLL" "$WORKDIR/output-final/"
    FINAL_DIR="$WORKDIR/output-final"
else
    log "Stage 4 already applied (revision $CURRENT_REVISION) -- using the installed files as-is."
    FINAL_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Verify -- run automatically before anything is deployed.
# ---------------------------------------------------------------------------

log "verifying..."

aes_encryptor_out=$($ILSPY -t "MailClient.Utils.Security.Cryptography.AESEncryptor" "$FINAL_DIR/MailClient.dll")
new_rfc_count=$(echo "$aes_encryptor_out" | grep -c "new Rfc2898DeriveBytes(" || true)
[[ "$new_rfc_count" -ge 2 ]] \
    || die "verification failed: expected >=2 'new Rfc2898DeriveBytes(' call sites in AESEncryptor, found $new_rfc_count"
echo "$aes_encryptor_out" | grep -q "Rfc2898DeriveBytes.Pbkdf2(" \
    && die "verification failed: AESEncryptor still calls the broken static Rfc2898DeriveBytes.Pbkdf2(...) -- patch did not fully apply"

filebasedcache_out=$($ILSPY -t 'MailClient.UI.FileBasedCache`1' "$FINAL_DIR/MailClient.Abstractions.dll")
echo "$filebasedcache_out" | grep -q "new Rfc2898DeriveBytes(" \
    || die "verification failed: FileBasedCache\`1.Initialize doesn't use the instance Rfc2898DeriveBytes API -- fix missing"
echo "$filebasedcache_out" | grep -q "Rfc2898DeriveBytes.Pbkdf2(" \
    && die "verification failed: FileBasedCache\`1 still calls the broken static Rfc2898DeriveBytes.Pbkdf2(...) -- patch did not fully apply"

$ILP --dump-handlers "$FINAL_DIR/MailClient.Abstractions.dll" 'MailClient.UI.FileBasedCache`1' Initialize 2>&1 | tail -1 | grep -q "^OK:" \
    || die "verification failed: --dump-handlers reported a handler-ordering violation in FileBasedCache\`1.Initialize"

# Stage 2 is a raw resource byte edit and must not change file size (same rationale as the
# 10.4.5674 pipeline's own Stages 6-7 check) -- checked against STAGE2_DIR specifically, not
# FINAL_DIR, since Stage 3 (notification chain) is real IL insertion that legitimately grows the
# file. Only meaningful (and only ran) when Stage 2 actually ran this time.
if [[ $START_STAGE -le 2 ]]; then
    STAGE1_SIZE=$(stat -c%s "$STAGE1_DIR/MailClient.dll")
    STAGE2_SIZE=$(stat -c%s "$STAGE2_DIR/MailClient.dll")
    [[ "$STAGE1_SIZE" -eq "$STAGE2_SIZE" ]] \
        || die "verification failed: Stage 2 should not change MailClient.dll's byte size (was $STAGE1_SIZE, now $STAGE2_SIZE) -- .resources offset table may be corrupted"
    ilspycmd_tip_out=$($ILSPY --resource "MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text" -o "$WORKDIR" "$STAGE2_DIR/MailClient.dll" 2>&1) || die "verification failed: couldn't extract labelTip.Text resource ($ilspycmd_tip_out)"
    tip_hex=$(xxd -p "$WORKDIR/labelTip.Text" | tr -d '\n')
    [[ "$tip_hex" == "c2a0c2a0" ]] \
        || die "verification failed: labelTip.Text should read c2 a0 c2 a0 (non-breaking spaces), found $tip_hex"
fi

# Stage 3 verification -- mirrors the 10.4.5674 pipeline's own equivalent checks (see its Stages
# 8-14 verification block), only meaningful (and only ran) when Stage 3 actually ran this time.
if [[ $START_STAGE -le 3 ]]; then
    $ILSPY -t "MailClient.UI.Forms.NotificationForms.FormGenericNotification" "$FINAL_DIR/MailClient.dll" | grep -q "__drawNotificationTextIntoBitmap" \
        || die "verification failed: __drawNotificationTextIntoBitmap not found -- notification text-in-bitmap fix missing"

    $ILSPY -t "MailClient.UI.Forms.NotificationForms.FormMailNotification" "$FINAL_DIR/MailClient.dll" | grep -q "RaisePaint" \
        || die "verification failed: FormMailNotification doesn't reference RaisePaint -- notification toolbar-icons fix missing"

    $ILSPY -t "MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton" "$FINAL_DIR/MailClient.Common.UI.dll" | grep -q "public void RaisePaint" \
        || die "verification failed: ControlToolStripButton.RaisePaint not found or not public -- notification toolbar-icons fix missing"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.UI.Forms.NotificationForms.FormGenericNotification OnShown 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in FormGenericNotification.OnShown"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Common.UI.dll" MailClient.Common.UI.Forms.LayeredForm WndProc 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in LayeredForm.WndProc"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.UI.Forms.NotificationForms.FormMailNotification updateBackgroundBitmap 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in FormMailNotification.updateBackgroundBitmap"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.UI.Forms.NotificationForms.FormMailNotification performMouseClick 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in FormMailNotification.performMouseClick"
fi

# Stage 4 verification -- mirrors the 10.4.5674 pipeline's own equivalent checks (see its Stage
# 15 verification block), only meaningful (and only ran) when Stage 4 actually ran this time.
if [[ $START_STAGE -le 4 ]]; then
    $ILSPY -t "MailClient.Accounts.AccountManager" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__RunSendAndReceiveAllCore" \
        || die "verification failed: AccountManager.__RunSendAndReceiveAllCore not found -- sync freeze fix missing"

    $ILSPY -t "MailClient.Storage.Application.Folder" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__folderSyncTaskEntry" \
        || die "verification failed: Folder.__folderSyncTaskEntry not found -- sync freeze fix missing"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Accounts.AccountManager __syncTaskEntry 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in AccountManager.__syncTaskEntry"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Storage.Application.Folder __folderSyncTaskEntry 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in Folder.__folderSyncTaskEntry"
fi

log "all verification checks passed."

# ---------------------------------------------------------------------------
# Backup, then deploy -- with rollback if anything goes wrong partway through the copy.
# ---------------------------------------------------------------------------

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP_DIR="$BACKUPS_DIR/${BOTTLE_NAME}-${TIMESTAMP}"
mkdir -p "$BACKUP_DIR"

DEPLOY_FILES=(MailClient.dll MailClient.Abstractions.dll MailClient.Common.UI.dll MailClient.Accounts.dll MailClient.Wine.dll)

log "backing up current files to $BACKUP_DIR ..."
for f in "${DEPLOY_FILES[@]}"; do
    if [[ -f "$BOTTLE_APP_DIR/$f" ]]; then
        cp -a "$BOTTLE_APP_DIR/$f" "$BACKUP_DIR/$f"
    fi
done

# cp -a preserving timestamps can fail against a mounted/shared bottle filesystem even with full
# read/write on content ("preserving times ... Operation not permitted") -- fall back to a
# plain content-only copy rather than failing the whole deploy over a cosmetic timestamp (same
# fix as the 10.4.5674 pipeline's own cp_deploy).
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

log "deploy complete."
log "  bottle:  $BOTTLE_NAME"
log "  version: $FOUND_FILE_VERSION"
log "  backup:  $BACKUP_DIR"

fi  # DLL_ALREADY_PATCHED

# ---------------------------------------------------------------------------
# Optional: install vendored Windows fonts (Segoe UI, Tahoma, Calibri, etc., under fonts/) into
# the bottle for better general font fidelity under Wine. Same tool and same fonts/ this repo
# already vendors for the 10.4.5674 pipeline (il-patches/font-systemlink-writer/ -- see its own
# doc comment and reports/splash-tip-icon-findings.md for why this goes through the real Win32
# registry API rather than a `.reg` file import). Deliberately separate from the DLL patch
# pipeline above -- independent of it (runs regardless of whether Stage 1 ran this time), and
# skipped entirely if fonts/ has no .ttf files. These are genuine Microsoft font files, not
# freely redistributable, so installing them requires confirming a valid license (unless
# --install-fonts/--no-fonts settles it non-interactively -- see header comment).
#
# font-systemlink-writer is built here targeting net10.0, NOT the net8.0 the 10.4.5674 pipeline
# builds it as -- this bottle only has the .NET 10 desktop runtime installed (confirmed: no
# Microsoft.NETCore.App/8.x under either Program Files\dotnet\shared here), matching eM Client
# 11's own .NET 10 target, so a net8.0 framework-dependent apphost would fail to launch
# ("framework not found") even though the tool's own code (plain Microsoft.Win32.Registry calls)
# is identical and has no version-specific behavior. Program.cs itself is reused verbatim from
# the shared, tracked source; only the csproj's TargetFramework differs, written fresh here
# rather than editing the shared file (which the 10.4.5674 pipeline still needs at net8.0 for
# its own, .NET-8-only bottles).
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
        cat > "$WORKDIR/tools/font-systemlink-writer/font-systemlink-writer.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>win-x86</RuntimeIdentifier>
    <SelfContained>false</SelfContained>
    <UseAppHost>true</UseAppHost>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
EOF
        if dotnet publish -c Release "$WORKDIR/tools/font-systemlink-writer" >"$WORKDIR/build-fontwriter.log" 2>&1; then
            FONTWRITER_EXE="$WORKDIR/tools/font-systemlink-writer/bin/Release/net10.0/win-x86/publish/font-systemlink-writer.exe"
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
