#!/usr/bin/env bash
# Deploys the CrossOver stabilization patches for eM Client 11 (beta) to a live CrossOver
# bottle. See ../../CLAUDE.md for the project overview and ../10.4.5674/deploy.sh for the more
# mature sibling pipeline this one deliberately does NOT share code with -- eM Client 11 is a
# genuinely different, still-beta product build (different assembly set, different bugs), and
# this script's own version gate (see SUPPORTED_EMCLIENT_VERSIONS below) keeps the two pipelines
# from ever being run against the wrong bottle by accident.
#
# Always regenerates the patch fresh from whatever assemblies are actually installed --
# deliberately never copies pre-built DLLs out of this repo (see the sibling script's own header
# comment for why: an update-prone app makes a stale pre-built copy actively dangerous). Nothing
# is deployed until every stage AND every verification check below succeeds.
#
# This script is versioned per eM Client release LINE (not per exact build): it lives at
# releases/<version>/deploy.sh and is built and tested against the eM Client build(s) listed in
# SUPPORTED_EMCLIENT_VERSIONS below. Unlike the general "copy the whole releases/<version>/
# folder to a new one" convention (still the right move whenever a new build's patches need real
# adaptation), 11.0.282 was added to this SAME script instead: a full decompile-diff plus a dry
# run of every stage against a fresh 11.0.282 install confirmed the patch content is completely
# unaffected (every touched type/method decompiles byte-for-byte identical to 11.0.196) -- see
# reports/emclient11-startup-stack-overflow-findings.md's own version-compat note and CLAUDE.md's
# eM Client 11 section. Add a new eM Client build to SUPPORTED_EMCLIENT_VERSIONS the same way
# only after doing that same verification; if a future build actually needs different patch
# logic, that's when a real fork into a new releases/<version>/ becomes the right move again.
#
# Usage:
#   ./deploy.sh                     interactive: lists bottles found, prompts for choice + confirms
#   ./deploy.sh --bottle NAME       skip bottle selection (bottle name, however the wine manager
#                                   in use names it -- CrossOver's ~/.cxoffice/ dir name, or
#                                   Bottles' own bottle name)
#   ./deploy.sh --list              list found bottles and their installed versions, then exit
#   ./deploy.sh -y|--yes            don't prompt on a version mismatch, continue automatically;
#                                   also answers every optional-stage prompt (Stage 6, Stage 7)
#                                   with its default of NOT included -- use --patches (below) to
#                                   include one non-interactively instead.
#   ./deploy.sh --force             skip the "already patched, nothing to do" short-circuit
#   ./deploy.sh --max-revision N    cap the MANDATORY stage chain at stage N (0..4) instead of
#                                   this script's own latest mandatory stage (PIPELINE_LATEST_STAGE).
#                                   Purely about the mandatory 1-4 chain -- to include or exclude an
#                                   optional stage (6, 7), use --patches instead; the two flags are
#                                   mutually exclusive. Useful for reverting a bottle to an earlier,
#                                   known-good mandatory stage (restore the bottle's files from
#                                   original/em-<version>/ first, THEN run with --max-revision --
#                                   this flag alone does not undo anything already installed) or
#                                   for bisecting which stage introduced a regression.
#   ./deploy.sh --patches 1,3,4     apply EXACTLY these stage numbers (mandatory or optional, any
#                                   combination), bypassing the normal "stages 1-4 are mandatory"
#                                   rule entirely -- for targeted patching. Only ever ADDS stages on
#                                   top of whatever the bottle already has (never removes an
#                                   already-applied one; that's what restoring from a backup or
#                                   original/em-<version>/ is for). This is also the only
#                                   non-interactive way to include an optional stage (6, 7) --
#                                   e.g. --patches 6 includes the Exchange sync-freeze fix without
#                                   answering its prompt. Mutually exclusive with --max-revision.
#   ./deploy.sh --install-fonts     install the vendored fonts/ without the license-consent prompt
#   ./deploy.sh --no-fonts          skip font installation without the license-consent prompt
#   ./deploy.sh --wine-manager crossover|bottles   which Wine-prefix manager the target bottle
#                                   lives under. Auto-detected when only one is installed;
#                                   prompted for ("Are you using CrossOver or Bottles?") when both
#                                   or neither are found and this flag isn't given (under -y with
#                                   neither/both found, this flag is required).
#
# Safe to re-run: reads a MailClient.Wine.dll version marker (same mechanism as the 10.4.5674
# pipeline -- see il-patches/MailClient.Wine/VersionMarker.cs) to tell which stages this bottle
# already has (a bitmask, one bit per stage number -- see STAGE_BIT below) and skips any stage
# whose bit is already set. Fonts (below) are independent of the DLL patch pipeline and always
# still checked, same as the sibling script.
#
# NOT backward compatible with a bottle patched by an older (pre-bitmask) version of this script
# -- that older marker encoded a linear stage COUNT, which this script now reads directly as a
# raw bitmask instead (deliberately no migration/bridging logic -- see CLAUDE.md). Reset such a
# bottle to pristine (fresh copy from original/em-<version>/) before running this version against
# it.
#
# Fonts: if fonts/*.ttf exist in this repo (genuine Microsoft fonts -- Segoe UI, Tahoma, Calibri
# -- vendored by whoever holds a valid license to use them; see reports/splash-tip-icon-findings.md),
# the script asks whether you hold a license and want them installed into the target bottle
# before deploying. --install-fonts / --no-fonts answer that non-interactively -- for scripted or
# repeated runs (e.g. a periodic check) where a prompt can't be answered by a human.
#
# Requires: dotnet SDK (checked below, prints install instructions if missing). python3 is only
# needed if Stage 7 (the optional Microsoft 365 / Graph API fix) is included -- that's the first
# stage in this release line to add a new sibling assembly MailClient.Wine.dll is genuinely
# LOADED as (same MailClient.deps.json edit the 10.4.5674 pipeline's own Stage 5 needs); every
# mandatory stage plus Stage 6 need neither. No network access needed beyond ilspycmd/NuGet
# restore (same as the sibling script).

set -euo pipefail

# eM Client FileVersion prefixes (major.minor.build, i.e. FileVersion minus its trailing
# revision component) this script is confirmed to work against, each mapped to THIS deploy
# line's own per-version release number (see il-patches/MailClient.Wine/VersionMarker.cs) --
# used only for the release/<version>-<N> git tag and log/header messages. This is a SEPARATE
# axis from PIPELINE_LATEST_STAGE below (which controls how many patch stages actually get
# built and is shared across every version here, since the patch content itself doesn't vary by
# version -- see the header comment's note on how 11.0.282 was verified): 11.0.282 starts its
# own release-number count at 1 even though the underlying pipeline is already at stage 4,
# because it's the first tagged release for that specific eM Client build, not because fewer
# stages apply to it. Add a new entry the first time a new eM Client build is confirmed working
# (after doing that same verification -- don't just add a version number here on faith); bump an
# existing entry's number when you tag a new release against a version that was already
# supported.
declare -A OUR_RELEASE_NUMBER_FOR_VERSION=( ["11.0.196"]=5 ["11.0.282"]=4 )

# Highest MANDATORY pipeline stage number this script builds by default -- shared across every
# supported eM Client version above. This (not OUR_RELEASE_NUMBER_FOR_VERSION) is what controls
# how far a fresh deploy goes absent any opt-in, unrelated to which eM Client build it's on.
#
# Stage 4 is the preview-pane periodic-repaint fix (mandatory) -- see its own header comment
# further down and reports/emclient11-preview-pane-blank-findings.md. It took the slot that used
# to be deliberately vacant (reserved for exactly this: "whatever the next real mandatory patch
# turns out to be"), leaving a fresh vacant Stage 5 behind for the next one. Optional stages (6,
# 7) always sit at the highest number(s), past every mandatory one, so a future mandatory patch
# takes the vacant slot and shifts every still-optional stage up by one again, same shuffle as
# last time.
PIPELINE_LATEST_STAGE=4

# Each stage tracks as its OWN bit in a single mask, stored directly as the on-bottle
# MailClient.Wine.dll marker's FileVersion trailing component -- e.g. a bottle with Stages
# 1,2,3,4 applied reads back as mask 15 (0b1111); one with 1-4 plus Stage 7 (but not 6) reads
# back as 79 (0b1001111). This is what lets Stage 6 and Stage 7 -- two independent optional
# fixes a bottle can carry in ANY combination -- be represented natively, with no per-stage
# special-casing needed (a plain linear "revision N means stages 1..N are done" count has no way
# to express "has 7 but not 6"). No entry for Stage 5 -- it's a vacant slot, nothing to track.
#
# Deliberately no migration from the older, pre-bitmask linear-count scheme this same marker
# field used to hold -- see the header comment's own note. A bottle patched by an older script
# needs a pristine reset before this version touches it.
declare -A STAGE_BIT=( [1]=1 [2]=2 [3]=4 [4]=8 [6]=32 [7]=64 )
MANDATORY_MASK=0
for _n in 1 2 3 4; do MANDATORY_MASK=$(( MANDATORY_MASK | STAGE_BIT[$_n] )); done
ALL_KNOWN_MASK=0
for _n in "${!STAGE_BIT[@]}"; do ALL_KNOWN_MASK=$(( ALL_KNOWN_MASK | STAGE_BIT[$_n] )); done
unset _n

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
MAX_REVISION=""
PATCHES_ARG=""
WINE_MANAGER_ARG=""

print_help() {
    sed -n '2,65p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --bottle) BOTTLE_OVERRIDE="${2:-}"; shift 2 ;;
        -y|--yes) ASSUME_YES=1; shift ;;
        --list) LIST_ONLY=1; shift ;;
        --force) FORCE=1; shift ;;
        --max-revision) MAX_REVISION="${2:-}"; shift 2 ;;
        --patches) PATCHES_ARG="${2:-}"; shift 2 ;;
        --install-fonts) INSTALL_FONTS=1; shift ;;
        --no-fonts) NO_FONTS=1; shift ;;
        --wine-manager) WINE_MANAGER_ARG="${2:-}"; shift 2 ;;
        -h|--help) print_help; exit 0 ;;
        *) die "unknown argument: $1 (see --help)" ;;
    esac
done

[[ -n "$MAX_REVISION" && -n "$PATCHES_ARG" ]] && die "--max-revision and --patches are mutually exclusive"

# MANDATORY_TARGET_MASK: how far up the mandatory 1-4 chain this run should reach -- either this
# script's own latest (PIPELINE_LATEST_STAGE) or, if --max-revision was given, that lower cap.
# Computed here (right after arg parsing, before the installed eM Client version is even known --
# deliberately: this is about pipeline stages, not about which version's release-number is being
# tagged) rather than inline at each use site, since it needs validating exactly once.
if [[ -n "$MAX_REVISION" ]]; then
    [[ "$MAX_REVISION" =~ ^[0-9]+$ ]] || die "--max-revision must be a non-negative integer, got: $MAX_REVISION"
    [[ "$MAX_REVISION" -ge 0 && "$MAX_REVISION" -le "$PIPELINE_LATEST_STAGE" ]] || die "--max-revision must be between 0 and $PIPELINE_LATEST_STAGE (the mandatory chain only -- use --patches to include an optional stage), got: $MAX_REVISION"
    MANDATORY_TARGET_MASK=0
    for _n in $(seq 1 "$MAX_REVISION"); do MANDATORY_TARGET_MASK=$(( MANDATORY_TARGET_MASK | STAGE_BIT[$_n] )); done
    unset _n
else
    MANDATORY_TARGET_MASK=$MANDATORY_MASK
fi

# PATCHES_MASK: the exact set of stages --patches named, validated against STAGE_BIT's known
# domain. Empty (0) if --patches wasn't given.
PATCHES_MASK=0
if [[ -n "$PATCHES_ARG" ]]; then
    IFS=',' read -ra _patch_list <<< "$PATCHES_ARG"
    for _n in "${_patch_list[@]}"; do
        [[ "$_n" =~ ^[0-9]+$ ]] || die "--patches: '$_n' is not a stage number"
        [[ -n "${STAGE_BIT[$_n]+x}" ]] || die "--patches: stage $_n doesn't exist (known stages: ${!STAGE_BIT[*]})"
        PATCHES_MASK=$(( PATCHES_MASK | STAGE_BIT[$_n] ))
    done
    unset _n _patch_list
fi

# ---------------------------------------------------------------------------
# Wine-prefix manager: CrossOver or Bottles (usebottles.com, the FOSS/Flatpak Wine-prefix
# manager). Auto-detected when only one is present; --wine-manager or an interactive prompt
# settles it when both or neither are found. Every bottle-touching operation below (discovery,
# running-instance detection, "run something inside the bottle", registry edits) dispatches on
# $WINE_MANAGER rather than hardcoding CrossOver -- see wine_run()/wine_reg_add() further down.
#
# Detection verified hands-on against a real Bottles install (Flatpak, com.usebottles.bottles)
# this same session -- several real quirks confirmed and worked around, not assumed from docs:
#   - `-j`/`--json` is a GLOBAL flag and must precede the subcommand (`bottles-cli -j list
#     bottles`, not `bottles-cli list bottles -j`) -- the latter errors as an unrecognized arg.
#   - On a genuinely fresh Bottles data directory, the FIRST-EVER bottles-cli invocation that
#     calls `list bottles` can crash with an uncaught FileNotFoundError (it lists Paths.bottles
#     before creating it) -- any other subcommand run first (this script always runs
#     `info bottles-path` before `list bottles`, needed anyway to resolve non-relocated bottle
#     paths) completes the same directory setup without crashing, so this is a non-issue as long
#     as that ordering is kept.
# ---------------------------------------------------------------------------

HAVE_CROSSOVER=0
[[ -x /opt/cxoffice/bin/wine ]] && HAVE_CROSSOVER=1

HAVE_BOTTLES=0
BOTTLES_CLI_CMD=()
if command -v bottles-cli >/dev/null 2>&1; then
    HAVE_BOTTLES=1
    BOTTLES_CLI_CMD=(bottles-cli)
elif command -v flatpak >/dev/null 2>&1 && flatpak list --app --columns=application 2>/dev/null | grep -qx "com.usebottles.bottles"; then
    HAVE_BOTTLES=1
    BOTTLES_CLI_CMD=(flatpak run --command=bottles-cli com.usebottles.bottles)
fi

print_bottles_install_instructions() {
    echo "  Flatpak (recommended, works on any distro):"
    echo "    flatpak install flathub com.usebottles.bottles"
    if [[ -f /etc/os-release ]]; then
        # shellcheck disable=SC1091
        . /etc/os-release
        case "${ID:-}" in
            arch|manjaro) echo "  Or from the AUR:  yay -S bottles" ;;
        esac
    fi
}

if [[ -n "$WINE_MANAGER_ARG" ]]; then
    WINE_MANAGER="$WINE_MANAGER_ARG"
    [[ "$WINE_MANAGER" == "crossover" || "$WINE_MANAGER" == "bottles" ]] \
        || die "--wine-manager must be 'crossover' or 'bottles', got: $WINE_MANAGER"
    [[ "$WINE_MANAGER" == "crossover" && $HAVE_CROSSOVER -eq 0 ]] && die "--wine-manager crossover given but CrossOver's wine binary wasn't found at /opt/cxoffice/bin/wine"
    [[ "$WINE_MANAGER" == "bottles" && $HAVE_BOTTLES -eq 0 ]] && die "--wine-manager bottles given but Bottles wasn't found (neither a native bottles-cli nor the com.usebottles.bottles Flatpak)"
elif [[ $HAVE_CROSSOVER -eq 1 && $HAVE_BOTTLES -eq 0 ]]; then
    WINE_MANAGER=crossover
elif [[ $HAVE_CROSSOVER -eq 0 && $HAVE_BOTTLES -eq 1 ]]; then
    WINE_MANAGER=bottles
elif [[ $HAVE_CROSSOVER -eq 0 && $HAVE_BOTTLES -eq 0 ]]; then
    err "neither CrossOver nor Bottles was found on this system."
    err "Install CrossOver (https://www.codeweavers.com/crossover) or Bottles:"
    print_bottles_install_instructions
    die "install one of the two and re-run this script."
else
    if [[ $ASSUME_YES -eq 1 ]]; then
        die "both CrossOver and Bottles were found -- pass --wine-manager crossover|bottles to pick one (required under -y)."
    fi
    echo ""
    echo "Both CrossOver and Bottles were found on this system."
    read -r -p "Are you using CrossOver or Bottles for this bottle? [c/b] " wmreply
    case "$wmreply" in
        [Cc]*) WINE_MANAGER=crossover ;;
        [Bb]*) WINE_MANAGER=bottles ;;
        *) die "please answer 'c' (CrossOver) or 'b' (Bottles), or pass --wine-manager." ;;
    esac
fi
log "wine manager: $WINE_MANAGER"

bottles_cli() { "${BOTTLES_CLI_CMD[@]}" "$@"; }

# wine_run <bottle-name> <windows-path-to-exe> [args...] -- launches an exe inside the bottle.
# -u DOTNET_ROOT: a host-side DOTNET_ROOT leaks into the wine child process and gets
# reinterpreted via the manager's own host-root drive mapping, pointing a .NET app's own apphost
# at the HOST's dotnet install instead of the bottle's -- confirmed for CrossOver (see the font
# install section below); stripped defensively here for Bottles too even though its own drive
# mapping wasn't specifically confirmed to have the same issue, since unsetting it is free
# insurance either way (harmless for non-.NET executables).
#
# NOTE (Bottles only, confirmed hands-on): `bottles-cli run`'s own stdout is NOT reliably
# forwarded back to the caller -- fine for every use here (installers/writers/scripts checked by
# exit code and on-disk side effects only, never by reading their console output), but do not
# extend this to anything that needs to read a launched program's output.
wine_run() {
    local name="$1"; shift
    local exe="$1"; shift
    if [[ "$WINE_MANAGER" == "crossover" ]]; then
        CX_BOTTLE="$name" env -u DOTNET_ROOT /opt/cxoffice/bin/wine "$exe" "$@"
    else
        env -u DOTNET_ROOT "${BOTTLES_CLI_CMD[@]}" run -b "$name" -e "$exe" "$@"
    fi
}

# wine_reg_add <bottle-name> <key> <value-name> <type> <data> -- REG_SZ/REG_BINARY/REG_DWORD all
# spelled identically by both `wine reg add` and `bottles-cli reg add`, so $type passes straight
# through unchanged either way. Confirmed hands-on: bottles-cli reg add accepts both HKCU-style
# short hive names and full HKEY_CURRENT_USER-style ones, same as wine's own reg.exe -- no key
# rewriting needed between the two branches. (Confirmed NOT reliable for Bottles: `reg.exe import`
# via wine_run -- a merged-.reg-file import silently failed to persist in testing. Every registry
# write in this project's scripts goes through single key/value calls like this one anyway, so
# that gap doesn't affect anything here.)
wine_reg_add() {
    local name="$1" key="$2" valname="$3" type="$4" data="$5"
    if [[ "$WINE_MANAGER" == "crossover" ]]; then
        CX_BOTTLE="$name" /opt/cxoffice/bin/wine reg add "$key" /v "$valname" /t "$type" /d "$data" /f >/dev/null
    else
        bottles_cli reg add -b "$name" -k "$key" -v "$valname" -t "$type" -d "$data" >/dev/null
    fi
}

# ---------------------------------------------------------------------------
# dotnet check
# ---------------------------------------------------------------------------

print_dotnet_install_instructions() {
    if [[ -f /etc/os-release ]]; then
        # shellcheck disable=SC1091
        . /etc/os-release
        case "${ID:-}" in
            debian|ubuntu|linuxmint|pop)
                echo "  Install with:  sudo apt update && sudo apt install -y dotnet-sdk-10.0" ;;
            fedora)
                echo "  Install with:  sudo dnf install -y dotnet-sdk-10.0" ;;
            rhel|centos|rocky|almalinux)
                echo "  Install with:  sudo dnf install -y dotnet-sdk-10.0"
                echo "  (may need Microsoft's package repo enabled first: https://learn.microsoft.com/dotnet/core/install/linux-rhel)" ;;
            opensuse*|sles)
                echo "  Install with:  sudo zypper install -y dotnet-sdk-10.0" ;;
            arch|manjaro)
                echo "  Install with:  sudo pacman -S dotnet-sdk" ;;
            *)
                echo "  See: https://learn.microsoft.com/dotnet/core/install/linux" ;;
        esac
    else
        echo "  See: https://learn.microsoft.com/dotnet/core/install/linux"
    fi
}

# Unlike the 10.4.5674 sibling script (net8.0), il-patcher here targets net10.0 (matching eM
# Client 11's own target -- see the csproj written further down) -- a .NET 8-only SDK install
# still satisfies `command -v dotnet` below, so that alone is not sufficient: it needs an actual
# net10.0-capable SDK present, or the first real build attempt fails deep into the pipeline with
# NETSDK1045 instead of a clear message here (hit for real -- see git history for the report).
check_dotnet() {
    if ! command -v dotnet >/dev/null 2>&1; then
        err "dotnet SDK not found -- required to build the patch tool and read assembly versions."
        print_dotnet_install_instructions
        die "install a .NET 10 SDK and re-run this script."
    fi
    if ! dotnet --list-sdks 2>/dev/null | awk '{print $1}' | cut -d. -f1 | grep -qE '^[1-9][0-9]$'; then
        err "no .NET 10+ SDK found -- il-patcher targets net10.0 (matching eM Client 11 itself)."
        err "installed SDK(s):"
        dotnet --list-sdks 2>/dev/null | sed 's/^/  /' >&2
        print_dotnet_install_instructions
        die "install a .NET 10 SDK (in addition to any older one already installed) and re-run this script."
    fi
}

check_dotnet

# ---------------------------------------------------------------------------
# Workdir (self-cleaning: removed on success, left in place with its path
# printed if anything fails or the script is interrupted, so it can be
# inspected)
# ---------------------------------------------------------------------------

WORKDIR="$(mktemp -d /tmp/emclient11-deploy-XXXXXX)"
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
# Find candidate bottles -- same classic install path shape as eM Client 10.x, checking BOTH
# architectures' conventional install locations (confirmed against the beta build: the x86
# package lands in "Program Files (x86)\eM Client", the x64 one in plain "Program Files\eM
# Client" -- see install-msix.sh's own --arch flag). Everything downstream derives its paths
# from whichever candidate is actually selected (BOTTLE_APP_DIR = dirname of the chosen
# MailClient.dll), so no other part of this script needs to know or care which architecture a
# given bottle is running -- the patch content itself is identical either way (MailClient
# .Common.UI.dll and MailClient.Accounts.dll are genuinely AnyCPU, byte-for-byte identical
# between the two packages; MailClient.dll IS architecture-specific at the PE level, but
# decompiles byte-for-byte identical either way -- verified via a full decompile-diff and dry
# run of every stage against a real x64 install).
# ---------------------------------------------------------------------------

shopt -s nullglob
BOTTLE_DLLS=()
declare -A BOTTLE_NAME_FOR_DLL

if [[ "$WINE_MANAGER" == "crossover" ]]; then
    for d in "$HOME"/.cxoffice/*/; do
        for candidate in "${d}drive_c/Program Files (x86)/eM Client/MailClient.dll" "${d}drive_c/Program Files/eM Client/MailClient.dll"; do
            if [[ -f "$candidate" ]]; then
                BOTTLE_DLLS+=("$candidate")
                rel="${candidate#"$HOME"/.cxoffice/}"
                BOTTLE_NAME_FOR_DLL["$candidate"]="${rel%%/*}"
            fi
        done
    done
    [[ ${#BOTTLE_DLLS[@]} -gt 0 ]] || die "no eM Client install found under any ~/.cxoffice/*/drive_c/Program Files (x86)/eM Client/ or .../Program Files/eM Client/"
else
    # Bottles: `info bottles-path` is run first (it's needed anyway to resolve non-relocated
    # bottle paths below) -- also serves as the fresh-data-dir warm-up call `list bottles` itself
    # needs (see the WINE_MANAGER detection block's own comment above).
    BOTTLES_BASE_PATH="$(bottles_cli info bottles-path 2>/dev/null)"
    [[ -n "$BOTTLES_BASE_PATH" ]] || die "'bottles-cli info bottles-path' returned nothing -- is Bottles set up correctly?"
    BOTTLES_JSON="$(bottles_cli -j list bottles 2>/dev/null | tail -1)"
    while IFS=$'\t' read -r bottle_name bottle_path; do
        [[ -n "$bottle_name" ]] || continue
        for candidate in "$bottle_path/drive_c/Program Files (x86)/eM Client/MailClient.dll" "$bottle_path/drive_c/Program Files/eM Client/MailClient.dll"; do
            if [[ -f "$candidate" ]]; then
                BOTTLE_DLLS+=("$candidate")
                BOTTLE_NAME_FOR_DLL["$candidate"]="$bottle_name"
            fi
        done
    done < <(python3 -c "
import json, sys
data = json.loads(sys.argv[1])
base = sys.argv[2]
for name, info in data.items():
    path = info.get('Path', name)
    real = path if info.get('Custom_Path') else base + '/' + path
    print(name + '\t' + real)
" "$BOTTLES_JSON" "$BOTTLES_BASE_PATH")
    [[ ${#BOTTLE_DLLS[@]} -gt 0 ]] || die "no eM Client install found in any Bottles bottle (checked 'Program Files (x86)\\eM Client' and 'Program Files\\eM Client' under each bottle's drive_c)"
fi
shopt -u nullglob

log "found ${#BOTTLE_DLLS[@]} eM Client install(s):"
for dll in "${BOTTLE_DLLS[@]}"; do
    bottle_name="${BOTTLE_NAME_FOR_DLL[$dll]}"
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

# RELEASE_VERSION is the found FileVersion's major.minor.build prefix (e.g. "11.0.196.0" ->
# "11.0.196") -- used from here on for every "which eM Client build is this" comparison and
# message (the on-bottle marker's own version check, the MailClient.Wine.dll AssemblyVersion
# built further down, etc). OUR_RELEASE_NUMBER is looked up from that prefix, purely for the
# release/<version>-<N> style messages -- see OUR_RELEASE_NUMBER_FOR_VERSION's own doc comment.
RELEASE_VERSION="${FOUND_FILE_VERSION%.*}"
if [[ -n "${OUR_RELEASE_NUMBER_FOR_VERSION[$RELEASE_VERSION]+x}" ]]; then
    OUR_RELEASE_NUMBER="${OUR_RELEASE_NUMBER_FOR_VERSION[$RELEASE_VERSION]}"
    log "eM Client $RELEASE_VERSION is a supported build (this deploy line's release $RELEASE_VERSION-$OUR_RELEASE_NUMBER)."
else
    OUR_RELEASE_NUMBER="?"
    warn "this deploy script is verified against: ${!OUR_RELEASE_NUMBER_FOR_VERSION[*]}"
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
# Read the MailClient.Wine.dll marker -- same mechanism (and same tracked source,
# il-patches/MailClient.Wine/VersionMarker.cs) as the 10.4.5674 pipeline, just interpreted as a
# bitmask (see STAGE_BIT above) rather than a linear stage count. A bottle with no marker, or a
# marker for a different eM Client version (e.g. it was last patched on 11.0.196 and has since
# been upgraded to 11.0.282 in place), is simply mask 0: the assemblies are different binaries
# even where the patch content happens to be identical, so a full fresh-patch pass is the safe
# default rather than assuming any stage's completion carries over. No legacy-scheme handling --
# see the header comment's own note on why a bottle patched by an older, pre-bitmask version of
# this script needs a pristine reset first rather than being auto-migrated.
# ---------------------------------------------------------------------------

INSTALLED_MASK=0
MARKER_DLL="$BOTTLE_APP_DIR/MailClient.Wine.dll"
if [[ -f "$MARKER_DLL" ]]; then
    marker_line=$($ILP --version "$MARKER_DLL" 2>/dev/null || true)
    marker_file_version=$(echo "$marker_line" | awk -F'\t' '{print $3}' | sed 's/FileVersion=//')
    marker_emclient_version="${marker_file_version%.*}"
    marker_mask="${marker_file_version##*.}"
    if [[ "$marker_emclient_version" == "$RELEASE_VERSION" && "$marker_mask" =~ ^[0-9]+$ ]]; then
        INSTALLED_MASK="$marker_mask"
        log "found MailClient.Wine.dll: this install has stage mask $INSTALLED_MASK ($RELEASE_VERSION)."
    else
        warn "MailClient.Wine.dll present but for a different eM Client version"
        warn "($marker_file_version, expected $RELEASE_VERSION.N) -- ignoring it, treating as unpatched."
    fi
fi

# ---------------------------------------------------------------------------
# Optional stages: Stage 6 (Exchange sync-freeze fix) and Stage 7 (Microsoft 365 / Graph API
# DNS+socket sync-freeze fix) -- see each one's own header comment further down for what they do
# and why they're optional. Both decided here, identically: already applied -> carry forward with
# no prompt; named explicitly via --patches -> included with no prompt; otherwise, with -y and no
# --patches, default to NOT included; otherwise ask interactively.
# ---------------------------------------------------------------------------

OPTIONAL_TARGET_MASK=0

if (( INSTALLED_MASK & STAGE_BIT[6] )); then
    OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[6] ))
elif (( PATCHES_MASK & STAGE_BIT[6] )); then
    OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[6] ))
    log "including the optional Exchange sync-freeze fix (Stage 6, named via --patches)."
elif [[ $ASSUME_YES -eq 0 ]]; then
    echo ""
    echo "Stage 6 (optional): a fix for severe, recurring multi-minute freezes during Exchange"
    echo "sync. Confirmed working on some machines, but not yet confirmed to behave the same"
    echo "way on every machine -- kept optional until that's better understood. Skipping this"
    echo "keeps the bottle on the mandatory pipeline only; you can opt in later with a plain"
    echo "re-run (or --patches 6) once you're ready to try it."
    read -r -p "Include the Exchange sync-freeze fix in this deploy? [y/N] " sfreply
    if [[ "$sfreply" =~ ^[Yy]$ ]]; then
        OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[6] ))
    fi
fi

if (( INSTALLED_MASK & STAGE_BIT[7] )); then
    OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[7] ))
elif (( PATCHES_MASK & STAGE_BIT[7] )); then
    OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[7] ))
    log "including the optional Microsoft 365 / Graph API sync-freeze fix (Stage 7, named via --patches)."
elif [[ $ASSUME_YES -eq 0 ]]; then
    echo ""
    echo "Stage 7 (optional): a fix for Microsoft 365 / Graph API account sync freezes -- a"
    echo "DIFFERENT bug from Stage 6's classic IMAP/EWS Exchange sync freeze. Root cause: the"
    echo "same broken Wine async DNS resolution, this time hit through HttpClient's own"
    echo "connection path, plus a second hang in socket I/O found after fixing the first."
    echo "Confirmed fixing a real, reproduced freeze live; not yet soaked broadly, so kept"
    echo "optional. Skipping this leaves Microsoft 365 / Graph accounts exposed to the Wine"
    echo "defect Stage 6 doesn't touch; you can opt in later with a plain re-run (or --patches 7)."
    read -r -p "Include the Microsoft 365 / Graph API sync-freeze fix in this deploy? [y/N] " gfreply
    if [[ "$gfreply" =~ ^[Yy]$ ]]; then
        OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[7] ))
    fi
fi

# ---------------------------------------------------------------------------
# TARGET_MASK: every stage this run should end up with -- the mandatory chain up to
# MANDATORY_TARGET_MASK, whichever optional stages were just decided above, and anything named
# directly via --patches (covers naming a MANDATORY stage number too, e.g. --patches 3 alone,
# which bypasses the normal "1-4 are all mandatory together" rule entirely -- targeted patching,
# not a full mandatory run). NEEDS_WORK_MASK is what's actually missing right now; masks only
# ever grow (a stage already applied is never re-applied, never removed by a later run).
# ---------------------------------------------------------------------------

TARGET_MASK=$(( MANDATORY_TARGET_MASK | OPTIONAL_TARGET_MASK | PATCHES_MASK ))
NEEDS_WORK_MASK=$(( TARGET_MASK & ~INSTALLED_MASK & ALL_KNOWN_MASK ))
# What the marker should read AFTER this run -- masks only ever grow, so this is always at least
# INSTALLED_MASK even in the (already-handled-above) case where nothing new gets applied.
NEW_MASK=$(( INSTALLED_MASK | TARGET_MASK ))

DLL_ALREADY_PATCHED=0
if [[ $FORCE -eq 0 && $NEEDS_WORK_MASK -eq 0 ]]; then
    DLL_ALREADY_PATCHED=1
    log "already has every requested stage (mask $INSTALLED_MASK) -- skipping the DLL patch"
    log "pipeline (pass --force to attempt patching anyway)."
elif [[ $FORCE -eq 0 ]]; then
    log "install has stage mask $INSTALLED_MASK -- applying what's missing (mask $NEEDS_WORK_MASK)."
else
    NEEDS_WORK_MASK=$(( TARGET_MASK & ALL_KNOWN_MASK ))
    log "--force given -- re-applying every targeted stage (mask $NEEDS_WORK_MASK) regardless of"
    log "what's already installed."
fi

if [[ $DLL_ALREADY_PATCHED -eq 0 ]]; then

# ---------------------------------------------------------------------------
# Refuse to patch into a bottle whose MailClient.exe is still running (same reasoning and same
# graceful-close-first logic as the 10.4.5674 pipeline -- see its own header comment for why an
# abrupt kill is avoided). Manager-agnostic by construction, no dispatch needed: it matches on
# the bottle NAME appearing anywhere in the process's environment, and both CrossOver
# (CX_BOTTLE/WINEPREFIX) and Bottles (WINEPREFIX pointed at .../bottles/<name>/) put the bottle's
# name into a launched wine process's environment somewhere either way.
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
# with the ACTUALLY DETECTED eM Client build's own version numbers (RELEASE_VERSION, resolved
# above from the real installed FileVersion, not a hardcoded constant -- this is what lets the
# same script support multiple eM Client builds): AssemblyVersion is "$RELEASE_VERSION.0",
# FileVersion's leading three components match that with the 4th being NEW_MASK -- the stage
# bitmask this run leaves the bottle at (see STAGE_BIT above), not just this script's own latest.
# ---------------------------------------------------------------------------

log "building MailClient.Wine version marker ($RELEASE_VERSION mask $NEW_MASK)..."
mkdir -p "$WORKDIR/tools/MailClient.Wine"
cp "$IL_PATCHES_DIR/MailClient.Wine/MailClient.Wine.csproj" "$IL_PATCHES_DIR/MailClient.Wine/VersionMarker.cs" "$WORKDIR/tools/MailClient.Wine/"
dotnet build -c Release \
    -p:AssemblyVersion="$RELEASE_VERSION.0" \
    -p:FileVersion="$RELEASE_VERSION.$NEW_MASK" \
    "$WORKDIR/tools/MailClient.Wine" >"$WORKDIR/build-mailclient-wine.log" 2>&1 \
    || { cat "$WORKDIR/build-mailclient-wine.log" >&2; die "failed to build MailClient.Wine marker (log above)"; }
MAILCLIENT_WINE_DLL="$WORKDIR/tools/MailClient.Wine/bin/Release/net8.0/MailClient.Wine.dll"
log "MailClient.Wine marker built ($RELEASE_VERSION.$NEW_MASK)."

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

if (( NEEDS_WORK_MASK & STAGE_BIT[1] )); then
    log "Stage 1: PBKDF2 startup crash fix (bcrypt-backed static Pbkdf2 -> working instance API)..."
    $ILP --patch-pbkdf2-instance-api "$WORKDIR/original" "$WORKDIR/output-stage1"
    STAGE1_DIR="$WORKDIR/output-stage1"
elif (( INSTALLED_MASK & STAGE_BIT[1] )); then
    # Already baked into what's actually installed ($WORKDIR/original is already a full,
    # complete copy of it) -- re-running would be redundant (and, unlike Stage 1 of the
    # 10.4.5674 pipeline, this patch has no "expected pristine, found already-patched" guard of
    # its own, so a re-run wouldn't even fail loudly -- skipping outright is the safe choice).
    log "Stage 1 already applied -- using the installed files as-is."
    STAGE1_DIR="$WORKDIR/original"
else
    # Not wanted this run at all (e.g. --max-revision capped below Stage 1). Nothing to carry
    # forward from (Stage 1 is the first), so this is the one case where "not wanted" and "not
    # yet started" coincide with plain original.
    log "Stage 1 skipped -- not targeted this run."
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

if (( NEEDS_WORK_MASK & STAGE_BIT[2] )); then
    log "Stage 2: splash-screen tip label icon fix (same tofu-box bug/fix as the 10.4.5674 pipeline)..."
    $ILP --patch-splash-tip-icon "$STAGE1_DIR" "$WORKDIR/output-stage2"
    STAGE2_DIR="$WORKDIR/output-stage2"
elif (( INSTALLED_MASK & STAGE_BIT[2] )); then
    log "Stage 2 already applied -- using the installed files as-is."
    STAGE2_DIR="$WORKDIR/original"
else
    log "Stage 2 skipped -- not targeted this run."
    STAGE2_DIR="$STAGE1_DIR"
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

if (( NEEDS_WORK_MASK & STAGE_BIT[3] )); then
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
elif (( INSTALLED_MASK & STAGE_BIT[3] )); then
    log "Stage 3 already applied -- using the installed files as-is."
    STAGE3_DIR="$WORKDIR/original"
else
    log "Stage 3 skipped -- not targeted this run."
    STAGE3_DIR="$STAGE2_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 4: message preview pane periodic-repaint fix (see
# reports/emclient11-preview-pane-blank-findings.md). Intermittently, the CEF-hosted preview pane
# goes completely blank -- sometimes just the header, sometimes a white square, sometimes the
# whole email -- for an arbitrary length of time, recovering instantly the moment the mouse moves
# back into the window. Root-caused as far as: a CX_DEBUGMSG="+message" trace showed the CEF
# child window's own hwnd going 19+ seconds with zero WM_PAINT dispatches, exactly overlapping a
# period where Wine delivers no mouse input to the process at all -- but the actual Wine/Chromium
# mechanism that stops (and later resumes) WM_PAINT was never pinned down; a leading theory
# (Chromium's own native-window-occlusion tracking) was built, deployed, and empirically ruled
# out live (see --patch-disable-native-win-occlusion's own doc comment, still in this tool,
# unused, as a documented dead end).
#
# Rather than keep chasing the exact mechanism, this is the same pragmatic mitigation already
# proven for an analogous Wine paint-staleness bug in this codebase
# (--patch-notification-periodic-reblit): periodically force a real repaint of the
# already-correctly-rendered content, independent of whatever normally would (or wouldn't)
# trigger one. --patch-preview-pane-periodic-repaint adds a 400ms Timer to
# ControlMessageDetail's constructor that calls a new private user32.dll RedrawWindow P/Invoke
# (RDW_INVALIDATE|RDW_ERASE|RDW_ALLCHILDREN|RDW_UPDATENOW) against
# webBrowser.NativeBrowserWindowHandle -- the real Chrome_WidgetWin_* native child hwnd CEF owns
# directly, exposed as its own public property -- whenever webBrowser is non-null, Visible, and
# has a real handle. Self-contained entirely within MailClient.dll (a private P/Invoke declared
# directly on ControlMessageDetail, not the shared WinApi.dll Win32 class), so no cross-module
# ImportReference complication and no shared-type blast radius. Confirmed via decompile +
# --dump-handlers (no exception handlers in the constructor, nothing branches to its final `ret`
# -- a safe insertion point with no retargeting needed) and live end-to-end: deployed to
# emClient_11_beta_win_11, user reproduced the freeze and confirmed the pane now recovers within
# about a second on its own, without needing a mouse move -- "This looks like the fix for this
# issue."
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[4] )); then
    log "Stage 4: message preview pane periodic-repaint fix..."
    $ILP --patch-preview-pane-periodic-repaint "$STAGE3_DIR" "$WORKDIR/output-stage4"
    STAGE4_DIR="$WORKDIR/output-stage4"
elif (( INSTALLED_MASK & STAGE_BIT[4] )); then
    log "Stage 4 already applied -- using the installed files as-is."
    STAGE4_DIR="$WORKDIR/original"
else
    log "Stage 4 skipped -- not targeted this run."
    STAGE4_DIR="$STAGE3_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 6 (OPTIONAL -- see the prompt/flags above): Exchange sync-freeze fix -- the exact same
# bug and same fix as the 10.4.5674 pipeline's Stage 15 (see
# reports/exchange-sync-freeze-findings.md there). Confirmed identical root cause here, not just
# a similar-looking symptom, before porting anything: decompile-diffed both target methods
# against the 10.4.5674 build and found them byte-for-byte structurally identical
# (MailClient.Accounts.AccountManager.SendAndReceiveAll and
# MailClient.Storage.Application.Folder.Synchronize(bool,bool) -- both still fully synchronous,
# both still reachable from the UI thread, no dedicated-thread dispatch of their own). Both
# `--patch-account-manager-sync-async` and `--patch-folder-sync-async` applied cleanly to this
# build's MailClient.Accounts.dll completely UNMODIFIED -- no adaptation needed at all (unlike
# Stage 3's two adapted sub-flags), verified via decompile (both the new dispatch wrappers and
# the moved-body `__Run*Core`/`__folderSyncTaskEntry` methods read correctly) and
# --dump-handlers (both add a `try`/`catch(Exception)`, both report correct nesting). Order
# between the two flags doesn't matter (different types, no shared state), same as the sibling
# script's own Stage 15.
#
# Numbered 6 (not 5) and OPTIONAL, unlike every mandatory stage before it: this fix has been
# observed to behave differently across machines, so it's no longer applied unconditionally --
# see the prompt/flags right after the bottle list above, and PIPELINE_LATEST_STAGE's own comment
# near the top of this script for the full renumbering rationale (Stage 5 is now the deliberately
# vacant slot, reserved for the next patch that IS safe to apply unconditionally -- Stage 4 took
# the previous vacant slot; see its own header comment above).
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[6] )); then
    log "Stage 6: Exchange sync-freeze fix (same bug/fix as the 10.4.5674 pipeline's Stage 15)..."
    $ILP --patch-account-manager-sync-async "$STAGE4_DIR" "$WORKDIR/output-stage6a"
    $ILP --patch-folder-sync-async "$WORKDIR/output-stage6a" "$WORKDIR/output-final"
    FINAL_DIR="$WORKDIR/output-final"
elif (( INSTALLED_MASK & STAGE_BIT[6] )); then
    log "Stage 6 already applied -- using the installed files as-is."
    FINAL_DIR="$WORKDIR/original"
else
    log "Stage 6 not included in this deploy."
    FINAL_DIR="$STAGE4_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 7 (OPTIONAL, independent axis -- see the detection/prompt block above): Microsoft 365 /
# Graph API DNS+socket sync-freeze fix -- a genuinely separate bug from Stage 6's classic
# IMAP/EWS Exchange sync freeze (see reports/emclient11-msgraph-sync-freeze-findings.md for the
# full investigation, including two mitigations tried and found insufficient before this, and a
# THIRD, different, CPU-bound freeze mechanism found and NOT fixed by this or anything else so
# far -- this stage does not claim to fix every Microsoft 365 freeze, only the DNS/socket one).
#
# Root cause: the same Wine GetAddrInfoExW defect already behind Stage 6's Exchange-sync bug,
# but reached through SocketsHttpHandler's own async DNS resolution when the Microsoft Graph SDK
# makes its HTTP calls -- confirmed via a bracketed CX_DEBUGMSG=+winsock trace and measured live
# as a single Graph HTTP call taking ~46 seconds with zero retries. A second hang, found live
# after fixing DNS alone, moved to TLS handshake/request/response -- the same broken-async-I/O
# defect family, one layer deeper. Fixed by MailClient.Wine.DnsConnectHelper/
# TimeoutBoundedNetworkStream (il-patches/MailClient.Wine/DnsConnectHelper.cs): resolve DNS via
# the older synchronous Dns.GetHostAddresses and connect directly by IP (sidestepping the broken
# async DNS path entirely), then bound every subsequent Read/Write to a genuinely synchronous,
# SO_RCVTIMEO/SO_SNDTIMEO-bounded socket call instead of .NET's broken async socket path.
# MailClient.Wine.TokenRefreshRetryHelper (same directory) additionally adds a narrow 3-attempt
# retry for OAuth2 token refresh (MailClient.Accounts.Credentials.GetAccessTokenRefreshResponse),
# confirmed via decompile to have had zero retry of its own for exactly the network failures the
# two fixes above can now surface as a clean timeout instead of an indefinite hang.
#
# Unlike every stage before it, MailClient.Wine.dll here is a REAL loaded dependency (both
# --patch-http-dns-connect-callback and --patch-token-refresh-retry call into it directly), not
# an inert marker file the app never actually loads -- so it must be built targeting net10.0
# (matching this app's own target framework; the plain version-marker build above stays net8.0,
# unaffected, since it's still never loaded by anything and the 10.4.5674 pipeline shares that
# same csproj for its own net8.0 bottles), and MailClient.deps.json needs a real entry for it
# (same mechanism as the 10.4.5674 pipeline's own BouncyCastle helper, Stage 5 there).
# ---------------------------------------------------------------------------

if (( TARGET_MASK & STAGE_BIT[7] )); then
    if (( INSTALLED_MASK & STAGE_BIT[7] )); then
        log "Stage 7 (MS Graph DNS/socket fix) already applied -- carrying the existing functional"
        log "MailClient.Wine.dll forward as-is (MailClient.dll/MailClient.Accounts.dll already"
        log "have this baked in via the pass-through logic above)."
        MAILCLIENT_WINE_DLL_OVERRIDE="$WORKDIR/original/MailClient.Wine.dll"
    else
        log "Stage 7: Microsoft 365 / Graph API DNS+socket sync-freeze fix..."
        mkdir -p "$WORKDIR/tools/MailClient.Wine.Full"
        cp "$IL_PATCHES_DIR/MailClient.Wine/VersionMarker.cs" \
           "$IL_PATCHES_DIR/MailClient.Wine/DnsConnectHelper.cs" \
           "$IL_PATCHES_DIR/MailClient.Wine/TokenRefreshRetryHelper.cs" \
           "$WORKDIR/tools/MailClient.Wine.Full/"
        # A fresh csproj, NOT a copy of the shared net8.0 one -- same reasoning and same pattern
        # as the font-systemlink-writer step below builds its own net10.0 csproj rather than
        # editing the shared net8.0 source file the 10.4.5674 pipeline still needs unchanged.
        cat > "$WORKDIR/tools/MailClient.Wine.Full/MailClient.Wine.csproj" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MailClient.Wine</RootNamespace>
    <AssemblyName>MailClient.Wine</AssemblyName>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="MailClient.Accounts">
      <HintPath>$BOTTLE_APP_DIR/MailClient.Accounts.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
EOF
        dotnet build -c Release \
            -p:AssemblyVersion="$RELEASE_VERSION.0" \
            -p:FileVersion="$RELEASE_VERSION.$NEW_MASK" \
            "$WORKDIR/tools/MailClient.Wine.Full" >"$WORKDIR/build-mailclient-wine-full.log" 2>&1 \
            || { cat "$WORKDIR/build-mailclient-wine-full.log" >&2; die "failed to build the full MailClient.Wine assembly for Stage 7 (log above)"; }
        WINE_FULL_DLL="$WORKDIR/tools/MailClient.Wine.Full/bin/Release/net10.0/MailClient.Wine.dll"

        $ILP --patch-http-dns-connect-callback "$FINAL_DIR" "$WINE_FULL_DLL" "$WORKDIR/output-stage7a"
        $ILP --patch-token-refresh-retry "$WORKDIR/output-stage7a" "$WINE_FULL_DLL" "$WORKDIR/output-stage7-final"
        FINAL_DIR="$WORKDIR/output-stage7-final"
        MAILCLIENT_WINE_DLL_OVERRIDE="$WINE_FULL_DLL"
    fi
else
    MAILCLIENT_WINE_DLL_OVERRIDE=""
fi

# Both Cecil patch invocations (any stage, not just Stage 7) only ever copy *.dll files forward --
# non-dll files like MailClient.deps.json never make it into a stage's own output directory.
# Backfill everything ELSE from $WORKDIR/original (the one directory guaranteed to hold a
# complete, untouched copy of everything the live bottle had) unconditionally, regardless of
# which stages ran this time -- --ignore-existing so no .dll actually patched this run is ever
# clobbered back to its pre-patch bytes. Only ever actually changes anything when Stage 7 ran
# (every other stage's own file set already has no non-dll gaps to fill), but doing it
# unconditionally, every run, is simpler and cheaper than conditioning it on which stage(s) fired.
rsync -a --ignore-existing "$WORKDIR/original"/ "$FINAL_DIR"/

if (( NEEDS_WORK_MASK & STAGE_BIT[7] )); then
    # MailClient.deps.json: MailClient.Wine is now a REAL loaded dependency (not just an inert
    # marker file sitting alongside the app) -- needs an explicit entry, same mechanism as the
    # 10.4.5674 pipeline's own BouncyCastle helper (Stage 5 there). VERSION here must match the
    # AssemblyVersion actually baked into MailClient.Wine.dll ($RELEASE_VERSION.0, set via the
    # -p:AssemblyVersion build property above) -- confirmed the hard way live: passing the
    # BouncyCastle helper's own default "1.0.0" here (its AssemblyVersion happens to also default
    # to 1.0.0.0 unless overridden, which is why that value worked fine there) built and deployed
    # without any error, but crashed on first launch with
    # System.IO.FileNotFoundException: Could not load ... 'MailClient.Wine, Version=11.0.196.0'
    # -- the deps.json entry existed, but under the wrong version key, so runtime resolution for
    # the version MailClient.dll's own AssemblyReference actually asks for still failed.
    command -v python3 >/dev/null 2>&1 || die "python3 is required for Stage 7 (patching MailClient.deps.json) but wasn't found"
    python3 "$IL_PATCHES_DIR/license-oaep-patcher-patch-deps-json.py" \
        "$FINAL_DIR/MailClient.deps.json" "MailClient.Wine" "$RELEASE_VERSION.0" ".NETCoreApp,Version=v10.0/win-x86" \
        || die "failed to patch MailClient.deps.json for MailClient.Wine (Stage 7)"
fi

# The version marker (or Stage 7's real functional build of the same filename, when included) is
# copied in here, unconditionally, regardless of which branch above produced FINAL_DIR -- it must
# always reflect NEW_MASK (every stage this run leaves the bottle with), not just whichever stage
# happened to run last. Safe to always overwrite: NEW_MASK is always >= INSTALLED_MASK (masks
# only ever grow), so this is never a regression even in the "nothing new to apply" case (which
# the DLL_ALREADY_PATCHED short-circuit above already handles by skipping this whole block).
if [[ -n "$MAILCLIENT_WINE_DLL_OVERRIDE" ]]; then
    cp "$MAILCLIENT_WINE_DLL_OVERRIDE" "$FINAL_DIR/MailClient.Wine.dll"
else
    cp "$MAILCLIENT_WINE_DLL" "$FINAL_DIR/"
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
if (( NEEDS_WORK_MASK & STAGE_BIT[2] )); then
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
if (( NEEDS_WORK_MASK & STAGE_BIT[3] )); then
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

# Stage 4 verification -- only meaningful (and only ran) when Stage 4 actually ran this time.
if (( NEEDS_WORK_MASK & STAGE_BIT[4] )); then
    $ILSPY -t "MailClient.UI.Controls.ControlMessageDetail.ControlMessageDetail" "$FINAL_DIR/MailClient.dll" | grep -q "__previewPaneRepaintTick" \
        || die "verification failed: ControlMessageDetail.__previewPaneRepaintTick not found -- preview pane periodic-repaint fix missing"

    $ILSPY -t "MailClient.UI.Controls.ControlMessageDetail.ControlMessageDetail" "$FINAL_DIR/MailClient.dll" | grep -q "__RedrawWindow" \
        || die "verification failed: ControlMessageDetail.__RedrawWindow P/Invoke not found -- preview pane periodic-repaint fix missing"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.UI.Controls.ControlMessageDetail.ControlMessageDetail .ctor 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in ControlMessageDetail's constructor"
fi

# Stage 6 verification -- mirrors the 10.4.5674 pipeline's own equivalent checks (see its Stage
# 15 verification block), only meaningful (and only ran) when Stage 6 actually ran this time.
if (( NEEDS_WORK_MASK & STAGE_BIT[6] )); then
    $ILSPY -t "MailClient.Accounts.AccountManager" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__RunSendAndReceiveAllCore" \
        || die "verification failed: AccountManager.__RunSendAndReceiveAllCore not found -- sync freeze fix missing"

    $ILSPY -t "MailClient.Storage.Application.Folder" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__folderSyncTaskEntry" \
        || die "verification failed: Folder.__folderSyncTaskEntry not found -- sync freeze fix missing"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Accounts.AccountManager __syncTaskEntry 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in AccountManager.__syncTaskEntry"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Storage.Application.Folder __folderSyncTaskEntry 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in Folder.__folderSyncTaskEntry"
fi

# Stage 7 verification -- only meaningful (and only ran) when Stage 7 was newly applied this run
# (a carry-forward of an already-applied Stage 7 has nothing new to verify -- MailClient.dll/
# MailClient.Accounts.dll came from the untouched $WORKDIR/original pass-through, already proven
# correct by whichever earlier run originally applied it).
if (( NEEDS_WORK_MASK & STAGE_BIT[7] )); then
    $ILSPY -t "MailClient.Protocols.InteractionController" "$FINAL_DIR/MailClient.dll" | grep -q "InstallConnectCallback" \
        || die "verification failed: InteractionController.CreateHttpClient doesn't reference InstallConnectCallback -- MS Graph DNS fix missing"

    $ILSPY -t "MailClient.Accounts.Credentials" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__GetAccessTokenRefreshResponseCore" \
        || die "verification failed: Credentials.__GetAccessTokenRefreshResponseCore not found -- token-refresh retry fix missing"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.Protocols.InteractionController CreateHttpClient 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in InteractionController.CreateHttpClient"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Accounts.Credentials GetAccessTokenRefreshResponse 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in Credentials.GetAccessTokenRefreshResponse"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Accounts.Credentials __GetAccessTokenRefreshResponseCore 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in Credentials.__GetAccessTokenRefreshResponseCore"

    grep -q "\"MailClient.Wine/$RELEASE_VERSION.0\"" "$FINAL_DIR/MailClient.deps.json" \
        || die "verification failed: MailClient.deps.json has no MailClient.Wine/$RELEASE_VERSION.0 entry -- Stage 7's helper assembly won't load at runtime"
fi

log "all verification checks passed."

# ---------------------------------------------------------------------------
# Backup, then deploy -- with rollback if anything goes wrong partway through the copy.
# ---------------------------------------------------------------------------

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP_DIR="$BACKUPS_DIR/${BOTTLE_NAME}-${TIMESTAMP}"
mkdir -p "$BACKUP_DIR"

DEPLOY_FILES=(MailClient.dll MailClient.Abstractions.dll MailClient.Common.UI.dll MailClient.Accounts.dll MailClient.Wine.dll MailClient.deps.json)

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
log "  bottle:      $BOTTLE_NAME"
log "  version:     $FOUND_FILE_VERSION (release $RELEASE_VERSION-$OUR_RELEASE_NUMBER)"
log "  stage mask:  $NEW_MASK"
log "  Exchange sync-freeze fix (Stage 6):  $([[ $(( NEW_MASK & STAGE_BIT[6] )) -ne 0 ]] && echo included || echo not included)"
log "  MS Graph DNS/socket fix (Stage 7):   $([[ $(( NEW_MASK & STAGE_BIT[7] )) -ne 0 ]] && echo included || echo not included)"
log "  backup:      $BACKUP_DIR"

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
            # A host-side DOTNET_ROOT (set by other tooling, e.g. for ilspycmd) leaks through into
            # the wine child process and gets reinterpreted via the manager's own host-root drive
            # mapping, pointing the bottle's own apphost at the HOST's dotnet install instead of
            # the bottle's -- confirmed hands-on under CrossOver ("hostfxr.dll could not be found
            # in [Y:\.dotnet\...]", Y: being CrossOver's mapping for $HOME) -- must be unset for
            # hostfxr's resolution to use the bottle's own C:\Program Files\dotnet installation as
            # intended. wine_run() (see the WINE_MANAGER detection block above) strips it for both
            # managers.
            if wine_run "$BOTTLE_NAME" "$WIN_FONTWRITER_PATH"; then
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
