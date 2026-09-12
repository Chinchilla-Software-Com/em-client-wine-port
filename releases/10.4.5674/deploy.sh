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
#   ./deploy.sh --bottle NAME       skip bottle selection (bottle name, however the wine manager
#                                   in use names it -- CrossOver's ~/.cxoffice/ dir name, or
#                                   Bottles' own bottle name)
#   ./deploy.sh --list              list found bottles and their installed versions, then exit
#   ./deploy.sh -y|--yes            don't prompt on a version mismatch, continue automatically.
#                                   The optional-patch prompt (patch 5) is only ever shown on a
#                                   fully plain invocation (no -y, no --max-revision, no
#                                   --patches) -- -y, --max-revision, or --patches all skip it and
#                                   default to NOT included; use --patches (below) to include it
#                                   non-interactively instead.
#   ./deploy.sh --force             skip the "already patched, nothing to do" short-circuit
#   ./deploy.sh --max-revision N    cap the MANDATORY patches at patch N (0..4) instead of this
#                                   script's own latest mandatory patch (PIPELINE_LATEST_PATCH).
#                                   Purely about the mandatory patches -- to include or exclude the
#                                   optional patch 5, use --patches instead; the two flags are
#                                   mutually exclusive. Useful for reverting a bottle to an
#                                   earlier, known-good mandatory patch (restore the bottle's
#                                   files from original/em-<version>/ first, THEN run with
#                                   --max-revision -- this flag alone does not undo anything
#                                   already installed) or for bisecting which patch introduced a
#                                   regression.
#   ./deploy.sh --patches 1,3,4     apply EXACTLY these PATCH numbers (mandatory or optional, any
#                                   combination), bypassing the normal "these patches are mandatory
#                                   together" rule entirely -- for targeted patching. Only ever
#                                   ADDS patches on top of whatever the bottle already has (never
#                                   removes an already-applied one; that's what restoring from a
#                                   backup or original/em-<version>/ is for). This is also the
#                                   only non-interactive way to include the optional patch 5 --
#                                   e.g. --patches 5 includes the sync-freeze fix without
#                                   answering its prompt. A "patch" is a curated, thematic group of
#                                   one or more internal stages (see PATCH_STAGES below) -- NOT the
#                                   same numbering as the internal stage numbers this script's own
#                                   progress log lines use ("Stage N: ..."); see PATCH_STAGES'
#                                   own comment, or README.md's patch table, for exactly which
#                                   stages each patch number covers. Mutually exclusive with
#                                   --max-revision.
#   ./deploy.sh --install-fonts     install the vendored fonts/ without the license-consent prompt
#   ./deploy.sh --no-fonts          skip font installation without the license-consent prompt
#   ./deploy.sh --install-associations   add file-type associations without the prompt
#   ./deploy.sh --no-associations        skip file-type associations without the prompt
#   ./deploy.sh --force-associations     also overwrite extensions that already have an association
#   ./deploy.sh --wine-manager crossover|bottles   which Wine-prefix manager the target bottle
#                                   lives under. Auto-detected when only one is installed;
#                                   prompted for ("Are you using CrossOver or Bottles?") when both
#                                   or neither are found and this flag isn't given (under -y with
#                                   neither/both found, this flag is required).
#
# Safe to re-run: reads a MailClient.Wine.dll version marker (il-patches/MailClient.Wine/
# VersionMarker.cs) whose FileVersion trailing component is a bitmask -- one bit per stage
# number (see STAGE_BIT below) -- telling this script exactly which stages this bottle already
# has, and skips any stage whose bit is already set.
#
# NOT backward compatible with a bottle patched by an older (pre-bitmask) version of this script,
# OR with the even older marker-less "BouncyCastlePatch assembly-reference present" detection
# this script used before MailClient.Wine.dll existed at all -- both get silently reinterpreted
# incorrectly under the new scheme (deliberately no migration/bridging logic -- see CLAUDE.md).
# Reset such a bottle to pristine (fresh copy from original/em-<version>/) before running this
# version against it.
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
# Requires: dotnet SDK -- **.NET 10+** specifically (checked below), matching the 11.0.196-beta
# sibling pipeline's own requirement, even though every project THIS script builds still targets
# net8.0 (a net10 SDK builds net8.0-targeted projects fine; this is purely about which SDK needs
# to be INSTALLED, not what any project here TARGETS -- none of that changed). python3 (for the
# deps.json patch step), network access (NuGet restore for Mono.Cecil, and ilspycmd if not
# already installed as a dotnet tool). Nothing else -- everything patch-related is built fresh
# each run into a temp directory that's cleaned up on success.

set -euo pipefail

RELEASE_VERSION="10.4.5674"
EXPECTED_FILE_VERSION="10.4.5674.0"

# This project's own release number against RELEASE_VERSION (see
# il-patches/MailClient.Wine/VersionMarker.cs) -- bump this every time a new release ships
# against the same eM Client version. Tag as release/<RELEASE_VERSION>-<OUR_RELEASE_NUMBER>.
OUR_RELEASE_NUMBER=4

# Each internal STAGE tracks as its OWN bit in a single mask, stored directly as the on-bottle
# MailClient.Wine.dll marker's FileVersion trailing component -- same scheme as the
# 11.0.196-beta sibling script (see its own STAGE_BIT comment for the full rationale: this is
# what lets an optional stage be represented and detected natively, no per-stage special-casing
# needed). This is a purely INTERNAL/technical numbering -- see PATCH_STAGES below for the
# externally-facing "patch" numbering --patches/--max-revision actually take.
#
# Stages 8-14 (the notification-toast fix, historically documented and log-messaged as seven
# separate numbered steps -- see each Stage's own header comment further down) share ONE bit:
# the tool itself enforces a strict internal ordering between them (fails loudly rather than
# silently misapplying if violated -- see CLAUDE.md's IL-patching lessons), so they only ever
# move together as a single atomic unit, never independently.
#
# Deliberately no migration from either older marker format this same field used to hold (the
# linear-count MailClient.Wine.dll scheme, or the even older marker-less BouncyCastlePatch-
# assembly-reference check from before that) -- see the header comment's own note. A bottle
# patched by an older script needs a pristine reset before this version touches it.
declare -A STAGE_BIT=( [1]=1 [2]=2 [3]=4 [4]=8 [5]=16 [6]=32 [7]=64 \
    [8]=128 [9]=128 [10]=128 [11]=128 [12]=128 [13]=128 [14]=128 \
    [15]=256 )

# PATCH is the externally-facing unit -- what --patches/--max-revision take, what README.md's
# patch tables document, what a user thinks of as "patch N". It's a curated, THEMATIC grouping of
# the internal stages above, chosen for what makes sense to a user picking which fixes to apply --
# deliberately NOT the same numbering as STAGE_BIT's own keys, and not required to stay in sync
# with it. Update this map (and README.md's patch tables) if the grouping ever changes; never
# assume a patch number equals a stage number anywhere in this script.
#   1 = stages 1, 6, 7   (icon/font "tofu box" fixes: splash banner blur, license icon, splash tip)
#   2 = stages 2, 3, 4   (Settings dialog fixes: clip-region paint, Load event, category-click crash)
#   3 = stage  5         (License Activation, encryption)
#   4 = stages 8-14      (notification toast -- all one bit already, see STAGE_BIT above)
#   5 = stage  15        (Exchange sync-freeze fix -- OPTIONAL, see its own section further down)
declare -A PATCH_STAGES=(
    [1]="1 6 7"
    [2]="2 3 4"
    [3]="5"
    [4]="8"
    [5]="15"
)
# Highest MANDATORY patch number this script builds by default. Patch 5 (sync-freeze fix) is
# OPTIONAL, past this -- see its own header comment further down for why (the same underlying
# fix in the 11.0.196-beta pipeline turned out to behave differently across machines).
PIPELINE_LATEST_PATCH=4
MANDATORY_PATCHES=(1 2 3 4)

MANDATORY_MASK=0
for _p in "${MANDATORY_PATCHES[@]}"; do
    for _s in ${PATCH_STAGES[$_p]}; do MANDATORY_MASK=$(( MANDATORY_MASK | STAGE_BIT[$_s] )); done
done
ALL_KNOWN_MASK=0
for _n in "${!STAGE_BIT[@]}"; do ALL_KNOWN_MASK=$(( ALL_KNOWN_MASK | STAGE_BIT[$_n] )); done
unset _n _p _s

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
MAX_REVISION=""
PATCHES_ARG=""
WINE_MANAGER_ARG=""

print_help() {
    sed -n '2,71p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
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
        --install-associations) INSTALL_ASSOCIATIONS=1; shift ;;
        --no-associations) NO_ASSOCIATIONS=1; shift ;;
        --force-associations) FORCE_ASSOCIATIONS=1; shift ;;
        --wine-manager) WINE_MANAGER_ARG="${2:-}"; shift 2 ;;
        -h|--help) print_help; exit 0 ;;
        *) die "unknown argument: $1 (see --help)" ;;
    esac
done

[[ -n "$MAX_REVISION" && -n "$PATCHES_ARG" ]] && die "--max-revision and --patches are mutually exclusive"

# MANDATORY_TARGET_MASK: how far up the mandatory patches (1 through 4) this run should reach.
# Three cases: --max-revision given -> that lower cap; --patches given (no --max-revision) ->
# ZERO -- --patches means targeted patching, bypassing the "mandatory patches apply together"
# default entirely, so nothing mandatory is implied just because it wasn't named (the only
# patches this run targets are whatever's in PATCHES_MASK below); neither flag given -> this
# script's own latest (PIPELINE_LATEST_PATCH), the plain "just run it" default.
if [[ -n "$MAX_REVISION" ]]; then
    [[ "$MAX_REVISION" =~ ^[0-9]+$ ]] || die "--max-revision must be a non-negative integer, got: $MAX_REVISION"
    [[ "$MAX_REVISION" -ge 0 && "$MAX_REVISION" -le "$PIPELINE_LATEST_PATCH" ]] || die "--max-revision must be between 0 and $PIPELINE_LATEST_PATCH (the mandatory patches only -- use --patches to include the optional patch 5), got: $MAX_REVISION"
    MANDATORY_TARGET_MASK=0
    for _p in $(seq 1 "$MAX_REVISION"); do
        for _s in ${PATCH_STAGES[$_p]}; do MANDATORY_TARGET_MASK=$(( MANDATORY_TARGET_MASK | STAGE_BIT[$_s] )); done
    done
    unset _p _s
elif [[ -n "$PATCHES_ARG" ]]; then
    MANDATORY_TARGET_MASK=0
else
    MANDATORY_TARGET_MASK=$MANDATORY_MASK
fi

# PATCHES_MASK: the exact set of PATCHES --patches named, each expanded to its underlying
# stage(s) via PATCH_STAGES and validated against that map's known domain. Empty (0) if
# --patches wasn't given.
PATCHES_MASK=0
if [[ -n "$PATCHES_ARG" ]]; then
    IFS=',' read -ra _patch_list <<< "$PATCHES_ARG"
    for _p in "${_patch_list[@]}"; do
        [[ "$_p" =~ ^[0-9]+$ ]] || die "--patches: '$_p' is not a patch number"
        [[ -n "${PATCH_STAGES[$_p]+x}" ]] || die "--patches: patch $_p doesn't exist (known patches: ${!PATCH_STAGES[*]})"
        for _s in ${PATCH_STAGES[$_p]}; do PATCHES_MASK=$(( PATCHES_MASK | STAGE_BIT[$_s] )); done
    done
    unset _p _s _patch_list
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
# forwarded back to the caller -- fine for every use here (installers/writers checked by exit
# code and on-disk side effects only, never by reading their console output), but do not extend
# this to anything that needs to read a launched program's output (see the file-associations
# section's own existing-value check further down for the case that DOES need this, and why it
# reads the bottle's own .reg files directly instead, for Bottles).
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
# short hive names and full HKEY_CURRENT_USER-style ones (and HKEY_CLASSES_ROOT, used by the
# file-associations section below), same as wine's own reg.exe -- no key rewriting needed between
# the two branches; an empty value-name ("") correctly targets a key's default (@=) value on both.
# (Confirmed NOT reliable for Bottles: `reg.exe import` via wine_run -- a merged-.reg-file import
# silently failed to persist in testing. Every registry write in this project's scripts goes
# through single key/value calls like this one anyway, so that gap doesn't affect anything here.)
wine_reg_add() {
    local name="$1" key="$2" valname="$3" type="$4" data="$5"
    if [[ "$WINE_MANAGER" == "crossover" ]]; then
        CX_BOTTLE="$name" /opt/cxoffice/bin/wine reg add "$key" /v "$valname" /t "$type" /d "$data" /f >/dev/null
    else
        bottles_cli reg add -b "$name" -k "$key" -v "$valname" -t "$type" -d "$data" >/dev/null
    fi
}

# ---------------------------------------------------------------------------
# dotnet / python3 checks
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

# .NET 10+ specifically, matching the 11.0.196-beta sibling script -- see this file's own header
# comment for why (SDK requirement only, no project here changes its own TargetFramework).
check_dotnet() {
    if ! command -v dotnet >/dev/null 2>&1; then
        err "dotnet SDK not found -- required to build the patch tools and read assembly versions."
        print_dotnet_install_instructions
        die "install a .NET 10 SDK and re-run this script."
    fi
    if ! dotnet --list-sdks 2>/dev/null | awk '{print $1}' | cut -d. -f1 | grep -qE '^[1-9][0-9]$'; then
        err "no .NET 10+ SDK found -- il-patcher (and this script's own tooling) targets net10.0."
        err "installed SDK(s):"
        dotnet --list-sdks 2>/dev/null | sed 's/^/  /' >&2
        print_dotnet_install_instructions
        die "install a .NET 10 SDK (in addition to any older one already installed) and re-run this script."
    fi
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
declare -A BOTTLE_NAME_FOR_DLL

if [[ "$WINE_MANAGER" == "crossover" ]]; then
    for d in "$HOME"/.cxoffice/*/; do
        candidate="${d}drive_c/Program Files (x86)/eM Client/MailClient.dll"
        if [[ -f "$candidate" ]]; then
            BOTTLE_DLLS+=("$candidate")
            # bottle name = the path component right after .cxoffice/
            rel="${candidate#"$HOME"/.cxoffice/}"
            BOTTLE_NAME_FOR_DLL["$candidate"]="${rel%%/*}"
        fi
    done
    [[ ${#BOTTLE_DLLS[@]} -gt 0 ]] || die "no eM Client install found under any ~/.cxoffice/*/drive_c/Program Files (x86)/eM Client/"
else
    # Bottles: `info bottles-path` is run first (it's needed anyway to resolve non-relocated
    # bottle paths below) -- also serves as the fresh-data-dir warm-up call `list bottles` itself
    # needs (see the WINE_MANAGER detection block's own comment above). Checks both
    # "Program Files (x86)" and plain "Program Files" under each bottle's drive_c, even though
    # this pipeline is 32-bit-only -- a Bottles bottle's own WoW64 layout was found, hands-on this
    # session, to vary by --arch/runner in ways not fully pinned down, so don't assume either path.
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
# is ever closed. Originally lived after the running-instance check -- moved here after a live
# review pointed out that a mismatch warning is useless once the disruptive part (closing the
# app) has already happened.
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
# Read the MailClient.Wine.dll marker -- interpreted as a bitmask (see STAGE_BIT above). A
# bottle with no marker, or a marker for a different eM Client version, is simply mask 0: the
# assemblies are different binaries even where the patch content happens to be identical, so a
# full fresh-patch pass is the safe default rather than assuming any stage's completion carries
# over. No legacy-scheme handling of any kind (neither the older linear-count marker format, nor
# the even older marker-less BouncyCastlePatch-assembly-reference check) -- see the header
# comment's own note on why a bottle patched by an older version of this script needs a pristine
# reset first rather than being auto-migrated.
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
        warn "MailClient.Wine.dll present but unreadable or for a different eM Client version"
        warn "($marker_file_version, expected $RELEASE_VERSION.N) -- ignoring it, treating as unpatched."
    fi
fi

# ---------------------------------------------------------------------------
# Optional patch: patch 5 (stage 15, sync-freeze fix) -- see its own header comment further down
# for what it does and why it's optional. Decided here: already applied -> carry forward with no
# prompt; named explicitly via --patches -> included with no prompt; otherwise, with -y and no
# --patches, default to NOT included; otherwise ask interactively. Same mechanism as the
# 11.0.196-beta sibling script's own optional patch.
# ---------------------------------------------------------------------------

OPTIONAL_TARGET_MASK=0

if (( INSTALLED_MASK & STAGE_BIT[15] )); then
    OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[15] ))
elif (( PATCHES_MASK & STAGE_BIT[15] )); then
    OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[15] ))
    log "including the optional sync-freeze fix (patch 5, named via --patches)."
elif [[ $ASSUME_YES -eq 0 && -z "$MAX_REVISION" && -z "$PATCHES_ARG" ]]; then
    # Only prompt in the fully-default invocation (no --max-revision, no --patches) -- if the
    # user gave either flag, they've already told this script exactly what they want; prompting
    # about something they didn't ask for would ignore that. Falls through to "not included"
    # (same as the -y default) when either flag was given without naming patch 5 explicitly.
    echo ""
    echo "Patch 5 (optional): a fix for severe, recurring multi-minute freezes during account/"
    echo "folder sync (see reports/exchange-sync-freeze-findings.md). The 11.0.196-beta sibling"
    echo "pipeline found this exact same fix behaves differently across machines once shipped as"
    echo "mandatory there, so it's kept optional here too until that's better understood."
    echo "Skipping this keeps the bottle on the mandatory patches only; you can opt in later"
    echo "with a plain re-run (or --patches 5) once you're ready to try it."
    read -r -p "Include the sync-freeze fix in this deploy? [y/N] " sfreply
    if [[ "$sfreply" =~ ^[Yy]$ ]]; then
        OPTIONAL_TARGET_MASK=$(( OPTIONAL_TARGET_MASK | STAGE_BIT[15] ))
    fi
fi

# ---------------------------------------------------------------------------
# TARGET_MASK: every stage this run should end up with -- the mandatory chain up to
# MANDATORY_TARGET_MASK, Stage 15 if just decided above, and anything named directly via
# --patches (covers naming a MANDATORY stage number too, e.g. --patches 3 alone, which bypasses
# the normal "the mandatory chain moves together" rule entirely -- targeted patching, not a full
# mandatory run). NEEDS_WORK_MASK is what's actually missing right now; masks only ever grow (a
# stage already applied is never re-applied, never removed by a later run).
# ---------------------------------------------------------------------------

TARGET_MASK=$(( MANDATORY_TARGET_MASK | OPTIONAL_TARGET_MASK | PATCHES_MASK ))
NEEDS_WORK_MASK=$(( TARGET_MASK & ~INSTALLED_MASK & ALL_KNOWN_MASK ))
# What the marker should read AFTER this run -- masks only ever grow, so this is always at least
# INSTALLED_MASK even in the (already-handled-below) case where nothing new gets applied.
NEW_MASK=$(( INSTALLED_MASK | TARGET_MASK ))

DLL_ALREADY_PATCHED=0
if [[ $FORCE -eq 0 && $NEEDS_WORK_MASK -eq 0 ]]; then
    DLL_ALREADY_PATCHED=1
    log "already has every requested stage (mask $INSTALLED_MASK) -- skipping the DLL patch"
    log "pipeline (pass --force to attempt patching anyway). Still checking fonts/file-"
    log "associations below, since those are independent of whether the DLL patches are applied."
elif [[ $FORCE -eq 0 ]]; then
    log "install has stage mask $INSTALLED_MASK -- applying what's missing (mask $NEEDS_WORK_MASK)."
else
    NEEDS_WORK_MASK=$(( TARGET_MASK & ALL_KNOWN_MASK ))
    log "--force given -- re-applying every targeted stage (mask $NEEDS_WORK_MASK) regardless of"
    log "what's already installed."
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
# doesn't block this one. Manager-agnostic by construction, no dispatch
# needed: both CrossOver (CX_BOTTLE/WINEPREFIX) and Bottles (WINEPREFIX
# pointed at .../bottles/<name>/) put the bottle's name into a launched wine
# process's environment somewhere either way.
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
# Copy the found install into the workdir as Stage 0 input -- never operate
# on the live bottle directly until the final, verified deploy step.
# ---------------------------------------------------------------------------

log "copying installed assemblies into workdir (this is the 'original/' for this run)..."
mkdir -p "$WORKDIR/original"
cp -a "$BOTTLE_APP_DIR"/. "$WORKDIR/original/"

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
# Build MailClient.Wine.dll -- the version marker. FileVersion's trailing component is NEW_MASK
# (the stage bitmask this run leaves the bottle at), not just this script's own latest. Always
# built and deployed regardless of which stages actually run, including on an already-fully-
# patched bottle reached only to add one more targeted stage, so every install this script ever
# touches ends up with an accurate marker for next time.
# ---------------------------------------------------------------------------

log "building MailClient.Wine version marker (release $RELEASE_VERSION-$OUR_RELEASE_NUMBER, mask $NEW_MASK)..."
mkdir -p "$WORKDIR/tools/MailClient.Wine"
cp "$IL_PATCHES_DIR/MailClient.Wine/MailClient.Wine.csproj" "$IL_PATCHES_DIR/MailClient.Wine/VersionMarker.cs" "$WORKDIR/tools/MailClient.Wine/"
dotnet build -c Release \
    -p:AssemblyVersion="$EXPECTED_FILE_VERSION" \
    -p:FileVersion="$RELEASE_VERSION.$NEW_MASK" \
    "$WORKDIR/tools/MailClient.Wine" >"$WORKDIR/build-mailclient-wine.log" 2>&1 \
    || { cat "$WORKDIR/build-mailclient-wine.log" >&2; die "failed to build MailClient.Wine marker (log above)"; }
MAILCLIENT_WINE_DLL="$WORKDIR/tools/MailClient.Wine/bin/Release/net8.0/MailClient.Wine.dll"
log "MailClient.Wine marker built."

# ---------------------------------------------------------------------------
# Stage 1: interpolation mode (splash banner + Settings icon resize blurriness).
# Touches MailClient.dll and MailClient.Common.UI.dll. See reports/gdiplus-interpolation-findings.md.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[1] )); then
    log "Stage 1: interpolation mode..."
    $ILP --patch "$IL_PATCHES_DIR/stage1-interpolation-mode.json" "$WORKDIR/original" "$WORKDIR/output-stage1"
    STAGE1_DIR="$WORKDIR/output-stage1"
elif (( INSTALLED_MASK & STAGE_BIT[1] )); then
    log "Stage 1 already applied -- using the installed files as-is."
    STAGE1_DIR="$WORKDIR/original"
else
    log "Stage 1 skipped -- not targeted this run."
    STAGE1_DIR="$WORKDIR/original"
fi

# ---------------------------------------------------------------------------
# Stage 2: AllPaintingInWmPaint (Wine clip-region paint bug in the Settings grid).
# Touches MailClient.Common.UI.dll only. See reports/settings-panel-clip-region-findings.md.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[2] )); then
    log "Stage 2: AllPaintingInWmPaint..."
    $ILP --patch-allpaintinginwmpaint "$STAGE1_DIR" "$WORKDIR/output-stage2"
    STAGE2_DIR="$WORKDIR/output-stage2"
elif (( INSTALLED_MASK & STAGE_BIT[2] )); then
    log "Stage 2 already applied -- using the installed files as-is."
    STAGE2_DIR="$STAGE1_DIR"
else
    log "Stage 2 skipped -- not targeted this run."
    STAGE2_DIR="$STAGE1_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 3: Settings panel Load-event fix (category panel blank under Wine).
# Touches MailClient.dll only.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[3] )); then
    log "Stage 3: Settings panel Load-event fix..."
    $ILP --patch-settings-refresh "$STAGE2_DIR" "$WORKDIR/output-stage3"
    STAGE3_DIR="$WORKDIR/output-stage3"
elif (( INSTALLED_MASK & STAGE_BIT[3] )); then
    log "Stage 3 already applied -- using the installed files as-is."
    STAGE3_DIR="$STAGE2_DIR"
else
    log "Stage 3 skipped -- not targeted this run."
    STAGE3_DIR="$STAGE2_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 4: category-click crash fix (Integration.IsDefaultClientVista).
# Touches MailClient.dll only.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[4] )); then
    log "Stage 4: category-click crash fix..."
    $ILP --patch-default-client-notimpl "$STAGE3_DIR" "$WORKDIR/output-stage4"
    STAGE4_DIR="$WORKDIR/output-stage4"
elif (( INSTALLED_MASK & STAGE_BIT[4] )); then
    log "Stage 4 already applied -- using the installed files as-is."
    STAGE4_DIR="$STAGE3_DIR"
else
    log "Stage 4 skipped -- not targeted this run."
    STAGE4_DIR="$STAGE3_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 5: License Activate RSA-OAEP decrypt fix. Touches MailClient.dll only, and adds a new
# sibling assembly (MailClient.Licensing.BouncyCastlePatch.dll) -- built here only when Stage 5
# is newly needed this run; carried forward from $WORKDIR/original (already contains it) when
# already applied, same "conditional extra-assembly build" pattern as the 11.0.196-beta sibling
# script's own Stage 7. BOUNCYCASTLEPATCH_DLL is set to the effective path to use for every
# subsequent stage's own explicit copy-forward -- empty if Stage 5 isn't part of TARGET_MASK at
# all (a bottle that genuinely never gets this fix has no such file to carry).
# ---------------------------------------------------------------------------

BOUNCYCASTLEPATCH_DLL=""
if (( TARGET_MASK & STAGE_BIT[5] )); then
    if (( INSTALLED_MASK & STAGE_BIT[5] )); then
        log "Stage 5 (License OAEP fix) already applied -- carrying the existing helper assembly forward."
        BOUNCYCASTLEPATCH_DLL="$WORKDIR/original/MailClient.Licensing.BouncyCastlePatch.dll"
        STAGE5_DIR="$STAGE4_DIR"
    else
        log "Stage 5: License Activate RSA-OAEP decrypt fix..."
        # The helper's BouncyCastle.Cryptography.dll reference is pointed at the JUST-COPIED,
        # install-sourced copy -- not this repo's own original/ -- so the deployed helper always
        # links against the exact BouncyCastle build the target install actually ships.
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

        mkdir -p "$WORKDIR/output-stage5"
        $ILP2 "$STAGE4_DIR/MailClient.dll" "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage5/MailClient.dll"
        # -n/--no-clobber: fill in the rest of the tree (everything Stage 5 doesn't touch)
        # without overwriting the MailClient.dll just patched above.
        cp -an "$STAGE4_DIR"/. "$WORKDIR/output-stage5/"
        cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage5/"
        STAGE5_DIR="$WORKDIR/output-stage5"
    fi
elif (( INSTALLED_MASK & STAGE_BIT[5] )); then
    log "Stage 5 already applied -- using the installed files as-is."
    BOUNCYCASTLEPATCH_DLL="$WORKDIR/original/MailClient.Licensing.BouncyCastlePatch.dll"
    STAGE5_DIR="$STAGE4_DIR"
else
    log "Stage 5 skipped -- not targeted this run."
    STAGE5_DIR="$STAGE4_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 6: license dialog "Get a license" button icon fix. Raw byte-level resource edit (no IL) --
# verified below by confirming output size is byte-identical to the input, which proves the
# resource container's offset table wasn't disturbed. Touches MailClient.dll only. Independent
# of Stage 5's own content (different resource entirely) -- carries BOUNCYCASTLEPATCH_DLL forward
# explicitly since Stage 5's helper assembly isn't something $ILP's own copy-everything-else
# logic would otherwise know to preserve across a stage it doesn't touch at all if it wasn't
# already sitting in this stage's own input directory.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[6] )); then
    log "Stage 6: license icon fix..."
    $ILP --patch-license-icon "$STAGE5_DIR" "$WORKDIR/output-stage6"
    [[ -n "$BOUNCYCASTLEPATCH_DLL" ]] && cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage6/"
    STAGE6_DIR="$WORKDIR/output-stage6"
elif (( INSTALLED_MASK & STAGE_BIT[6] )); then
    log "Stage 6 already applied -- using the installed files as-is."
    STAGE6_DIR="$STAGE5_DIR"
else
    log "Stage 6 skipped -- not targeted this run."
    STAGE6_DIR="$STAGE5_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 7: splash-screen tip label icon fix. Same raw byte-level resource edit pattern as
# Stage 6 (genuine Wine emoji-glyph gap, not a bad-bytes-in-our-own-resource issue like Stage 6
# -- see reports/splash-tip-icon-findings.md). Touches MailClient.dll only.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[7] )); then
    log "Stage 7: splash-screen tip icon fix..."
    $ILP --patch-splash-tip-icon "$STAGE6_DIR" "$WORKDIR/output-stage7"
    [[ -n "$BOUNCYCASTLEPATCH_DLL" ]] && cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage7/"
    STAGE7_DIR="$WORKDIR/output-stage7"
elif (( INSTALLED_MASK & STAGE_BIT[7] )); then
    log "Stage 7 already applied -- using the installed files as-is."
    STAGE7_DIR="$STAGE6_DIR"
else
    log "Stage 7 skipped -- not targeted this run."
    STAGE7_DIR="$STAGE6_DIR"
fi

# ---------------------------------------------------------------------------
# Stages 8-14 (one bit, STAGE_BIT[8] == ... == STAGE_BIT[14] -- see the top-of-file comment for
# why): new-mail notification toast fixes (see CLAUDE.md's Status section, "Email notifications
# not displaying correctly until fade" -- one root problem, several visible symptoms, all fixed
# together as one chain). Order matters and is enforced by the tool itself (fails loudly on the
# wrong order rather than silently misapplying) -- this exact sequence was verified by a fresh
# rebuild from original/ straight through, decompiled-output-identical to what was live-tested
# end to end. Don't reorder without re-verifying the same way.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[8] )); then
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
    [[ -n "$BOUNCYCASTLEPATCH_DLL" ]] && cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-stage14/"

    STAGE14_DIR="$WORKDIR/output-stage14"
elif (( INSTALLED_MASK & STAGE_BIT[8] )); then
    log "Stages 8-14 already applied -- using the installed files as-is."
    STAGE14_DIR="$STAGE7_DIR"
else
    log "Stages 8-14 skipped -- not targeted this run."
    STAGE14_DIR="$STAGE7_DIR"
fi

# ---------------------------------------------------------------------------
# Stage 15 (OPTIONAL -- see the prompt/flags above): sync freeze fix (see
# reports/exchange-sync-freeze-findings.md). AccountManager.SendAndReceiveAll's own sequential
# per-account loop, and Folder.Synchronize's own recursive per-folder walk, could each block
# whatever thread calls them (very often the UI thread -- the once-a-minute auto-sync timer, the
# "check for mail on startup" option, manual refresh, and the "download for offline use"
# folder-tree walk all call one or the other directly and synchronously) for up to 20 seconds per
# account/folder on Wine's GetAddrInfoExW, which can't honor a connection-timeout cancellation.
# Both patches touch MailClient.Accounts.dll only, dispatching onto a dedicated background
# Thread instead (not Task.Run -- see either patch's own doc comment for why: Task.Run's shared
# ThreadPool reintroduced a different stutter under concurrent load).
#
# Numbered past the mandatory chain and OPTIONAL, unlike every stage before it: the identical fix
# in the 11.0.196-beta sibling pipeline was found to behave differently across machines once
# shipped as mandatory there (see that script's own Stage 6 comment) -- kept optional here too,
# from the start, rather than waiting to discover the same thing the hard way on this line.
# ---------------------------------------------------------------------------

if (( NEEDS_WORK_MASK & STAGE_BIT[15] )); then
    log "Stage 15: sync freeze fix (account-level sync + folder-tree sync both now run off the UI thread)..."
    $ILP --patch-account-manager-sync-async "$STAGE14_DIR" "$WORKDIR/output-stage15a"
    $ILP --patch-folder-sync-async "$WORKDIR/output-stage15a" "$WORKDIR/output-final"
    [[ -n "$BOUNCYCASTLEPATCH_DLL" ]] && cp "$BOUNCYCASTLEPATCH_DLL" "$WORKDIR/output-final/"
    FINAL_DIR="$WORKDIR/output-final"
elif (( INSTALLED_MASK & STAGE_BIT[15] )); then
    log "Stage 15 already applied -- using the installed files as-is."
    FINAL_DIR="$STAGE14_DIR"
else
    log "Stage 15 not included in this deploy."
    FINAL_DIR="$STAGE14_DIR"
fi

# Every Cecil patch invocation above only ever copies *.dll files forward -- non-dll files (and,
# in one specific edge case, MailClient.Licensing.BouncyCastlePatch.dll if a stage's own input
# directory didn't already have it staged in) never make it into a stage's own output directory
# on their own. Backfill everything ELSE from $WORKDIR/original (the one directory guaranteed to
# hold a complete, untouched copy of everything the live bottle had) unconditionally --
# --ignore-existing so no file actually (re)patched this run is ever clobbered back to its
# pre-patch bytes.
rsync -a --ignore-existing "$WORKDIR/original"/ "$FINAL_DIR"/

cp "$MAILCLIENT_WINE_DLL" "$FINAL_DIR/"

# ---------------------------------------------------------------------------
# Verify -- every check CLAUDE.md documents, run automatically. Any failure
# aborts here, before the live bottle has been touched at all.
# ---------------------------------------------------------------------------

log "verifying..."

if (( NEEDS_WORK_MASK & STAGE_BIT[1] )); then
    interp_line=$($ILP "$FINAL_DIR" | tail -1)
    echo "$interp_line" | grep -q "Total InterpolationMode set-sites found: 13" \
        || die "verification failed: expected 13 total InterpolationMode set-sites, got: $interp_line"
fi

if (( NEEDS_WORK_MASK & STAGE_BIT[3] )); then
    $ILSPY -m "M:MailClient.UI.Forms.formSettings.formSettings_Load(System.Object,System.EventArgs)" "$FINAL_DIR/MailClient.dll" >/dev/null \
        || die "verification failed: formSettings_Load doc-id lookup failed"
fi

if (( NEEDS_WORK_MASK & STAGE_BIT[2] )); then
    $ILSPY -t "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid" "$FINAL_DIR/MailClient.Common.UI.dll" | grep -q AllPaintingInWmPaint \
        || die "verification failed: AllPaintingInWmPaint not found in ControlDataGrid"
fi

if (( NEEDS_WORK_MASK & STAGE_BIT[5] )); then
    oaep_hits=$($ILSPY -t "MailClient.Licensing.DecryptAndVerify" "$FINAL_DIR/MailClient.dll" | grep -c OaepPatch || true)
    [[ "$oaep_hits" -ge 2 ]] || die "verification failed: expected >=2 OaepPatch call sites in DecryptAndVerify, found $oaep_hits"
fi

if (( NEEDS_WORK_MASK & STAGE_BIT[4] )); then
    $ILP --dump-handlers "$FINAL_DIR/MailClient.dll" MailClient.Utils.Integration IsDefaultClientVista | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation"
fi

# Stages 6-7 are raw resource byte edits and must not change file size (see their own header
# comments) -- checked against whatever fed INTO Stage 6 this run (STAGE5_DIR, whatever that
# resolved to) versus whatever came OUT of the last of 6/7 that actually ran, since the .resources
# offset table must be identical before and after a byte-level edit regardless of which other
# stages did or didn't run alongside it this time.
if (( NEEDS_WORK_MASK & (STAGE_BIT[6] | STAGE_BIT[7]) )); then
    # Only a valid apples-to-apples comparison when at least one of stages 1-5 ALSO ran fresh
    # this pass -- that's the only case STAGE5_DIR is guaranteed to be a real Mono.Cecil-
    # serialized output, same as whatever stage 6/7 produced. If none of 1-5 ran this pass (e.g.
    # a bottle already has them, or they were never targeted), STAGE5_DIR is instead the
    # pristine, NEVER-Cecil-touched original -- Cecil's own writer re-serializes the WHOLE PE
    # file on any patch pass (metadata table layout, debug info, etc. can shift by tens of KB),
    # producing a size delta that's an artifact of the Cecil round-trip itself, unrelated to
    # whether stage 6/7's OWN resource edit preserved length (confirmed hands-on against the
    # 11.0.196-beta sibling script's own equivalent check: a real ~107KB "mismatch" with the
    # resource bytes underneath already correct). Currently unreachable through this script's own
    # patch grouping (patch 1 always bundles stages 1, 6, 7 together, so stage 1 is always fresh
    # whenever 6/7 are), but guarded defensively rather than relying on that staying true.
    if (( NEEDS_WORK_MASK & (STAGE_BIT[1] | STAGE_BIT[2] | STAGE_BIT[3] | STAGE_BIT[4] | STAGE_BIT[5]) )); then
        PRE_BYTE_EDIT_SIZE=$(stat -c%s "$STAGE5_DIR/MailClient.dll")
        POST_BYTE_EDIT_DIR="$STAGE7_DIR"
        [[ "$POST_BYTE_EDIT_DIR" == "$WORKDIR/original" ]] && POST_BYTE_EDIT_DIR="$STAGE6_DIR"
        [[ "$POST_BYTE_EDIT_DIR" == "$WORKDIR/original" ]] && POST_BYTE_EDIT_DIR="$STAGE5_DIR"
        POST_BYTE_EDIT_SIZE=$(stat -c%s "$POST_BYTE_EDIT_DIR/MailClient.dll")
        [[ "$PRE_BYTE_EDIT_SIZE" -eq "$POST_BYTE_EDIT_SIZE" ]] \
            || die "verification failed: Stages 6-7 should not change MailClient.dll's byte size (was $PRE_BYTE_EDIT_SIZE, now $POST_BYTE_EDIT_SIZE) -- .resources offset table may be corrupted"
    fi
fi

if (( NEEDS_WORK_MASK & STAGE_BIT[8] )); then
    $ILSPY -t "MailClient.UI.Forms.NotificationForms.FormGenericNotification" "$FINAL_DIR/MailClient.dll" | grep -q "__drawNotificationTextIntoBitmap" \
        || die "verification failed: __drawNotificationTextIntoBitmap not found -- notification text-in-bitmap fix missing"

    $ILSPY -t "MailClient.UI.Forms.NotificationForms.FormMailNotification" "$FINAL_DIR/MailClient.dll" | grep -q "RaisePaint" \
        || die "verification failed: FormMailNotification doesn't reference RaisePaint -- notification toolbar-icons fix missing"

    $ILSPY -t "MailClient.Common.UI.Controls.ControlToolStrip.ControlToolStripButton" "$FINAL_DIR/MailClient.Common.UI.dll" | grep -q "public void RaisePaint" \
        || die "verification failed: ControlToolStripButton.RaisePaint not found or not public -- notification toolbar-icons fix missing"
fi

if (( NEEDS_WORK_MASK & STAGE_BIT[15] )); then
    $ILSPY -t "MailClient.Accounts.AccountManager" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__syncTaskEntry" \
        || die "verification failed: AccountManager.__syncTaskEntry not found -- sync freeze fix missing"

    $ILSPY -t "MailClient.Storage.Application.Folder" "$FINAL_DIR/MailClient.Accounts.dll" | grep -q "__folderSyncTaskEntry" \
        || die "verification failed: Folder.__folderSyncTaskEntry not found -- sync freeze fix missing"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Accounts.AccountManager __syncTaskEntry 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in AccountManager.__syncTaskEntry"

    $ILP --dump-handlers "$FINAL_DIR/MailClient.Accounts.dll" MailClient.Storage.Application.Folder __folderSyncTaskEntry 2>&1 | tail -1 | grep -q "^OK:" \
        || die "verification failed: --dump-handlers reported a handler-ordering violation in Folder.__folderSyncTaskEntry"
fi

log "all verification checks passed."

# ---------------------------------------------------------------------------
# Backup, then deploy -- with rollback if anything goes wrong partway
# through the copy.
# ---------------------------------------------------------------------------

TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
BACKUP_DIR="$BACKUPS_DIR/${BOTTLE_NAME}-${TIMESTAMP}"
mkdir -p "$BACKUP_DIR"

DEPLOY_FILES=(MailClient.dll MailClient.Common.UI.dll MailClient.Accounts.dll MailClient.Wine.dll)
[[ -f "$FINAL_DIR/MailClient.Licensing.BouncyCastlePatch.dll" ]] && DEPLOY_FILES+=(MailClient.Licensing.BouncyCastlePatch.dll)

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

PATCHES_APPLIED=""
for _p in $(printf '%s\n' "${!PATCH_STAGES[@]}" | sort -n); do
    _pm=0
    for _s in ${PATCH_STAGES[$_p]}; do _pm=$(( _pm | STAGE_BIT[$_s] )); done
    if [[ $(( NEW_MASK & _pm )) -eq $_pm ]]; then
        PATCHES_APPLIED="${PATCHES_APPLIED:+$PATCHES_APPLIED,}$_p"
    fi
done
unset _p _s _pm

log "deploy complete."
log "  bottle:      $BOTTLE_NAME"
log "  version:     $FOUND_FILE_VERSION"
log "  patches applied: ${PATCHES_APPLIED:-none} (stage mask $NEW_MASK -- see README.md's patch table for what each number covers)"
log "  sync-freeze fix (patch 5):  $([[ $(( NEW_MASK & STAGE_BIT[15] )) -ne 0 ]] && echo included || echo not included)"
log "  backup:      $BACKUP_DIR"

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

# Bottle-prefix root (holds system.reg/user.reg, three levels above BOTTLE_APP_DIR -- .../eM
# Client -> .../Program Files (x86) -> .../drive_c -> the bottle root itself) -- only actually
# used by the Bottles existing-value check below, but resolved unconditionally since it's cheap.
BOTTLE_PREFIX_ROOT="$(cd "$BOTTLE_APP_DIR/../../.." && pwd)"

# Bottles-only: is there already an `@=` (default value) under [Software\Classes\.$ext] in either
# user.reg (HKCU) or system.reg (HKLM) -- the two files a Bottles bottle's HKEY_CLASSES_ROOT merge
# actually reads from. Reads the bottle's own .reg files directly rather than shelling to `wine
# reg query` (confirmed hands-on: `bottles-cli run`'s own stdout isn't reliably forwarded back to
# the caller, so a query's result can't be read that way under Bottles -- see the WINE_MANAGER
# detection block's own note). python3 (already a hard dependency of this script) rather than a
# hand-rolled awk/grep state machine, for the same reason parse-reg-associations.py itself is
# python: multi-line section parsing is fiddly to get right in awk and easy to get right here.
bottles_hkcr_has_default() {
    local ext="$1"
    python3 -c "
import re, sys
ext_re = re.compile(r'^\[Software\\\\Classes\\\\\.' + re.escape(sys.argv[1]) + r'\]')
for path in sys.argv[2:]:
    try:
        with open(path, encoding='utf-8', errors='replace') as f:
            lines = f.read().splitlines()
    except FileNotFoundError:
        continue
    in_section = False
    for line in lines:
        if ext_re.match(line):
            in_section = True
            continue
        if in_section:
            if line.startswith('['):
                in_section = False
                continue
            if line.startswith('@='):
                sys.exit(0)
sys.exit(1)
" "$ext" "$BOTTLE_PREFIX_ROOT/user.reg" "$BOTTLE_PREFIX_ROOT/system.reg"
}

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
        # Bottles path: instead of one merged .reg file (reg.exe import was confirmed NOT to
        # reliably persist under Bottles -- see the WINE_MANAGER detection block's own note),
        # accumulate the individual key/data pairs to apply via wine_reg_add, one call per pair.
        BOTTLES_PAIRS_TO_APPLY=()

        added=0
        skipped=0
        forced=0
        while IFS=$'\t' read -r ext block_b64 pairs_json; do
            if [[ "$WINE_MANAGER" == "crossover" ]]; then
                # `wine reg query` exits non-zero for a genuinely-missing key -- expected, not an
                # error, but pipefail (set above) would otherwise propagate that through this
                # substitution and abort the whole script via set -e. || true absorbs it;
                # $existing is correctly empty in that case either way.
                existing=$( (CX_BOTTLE="$BOTTLE_NAME" /opt/cxoffice/bin/wine reg query "HKCR\\.$ext" /ve 2>/dev/null \
                    | sed -n 's/.*REG_SZ *//p' | tr -d '\r') || true)
                has_existing=0
                [[ -n "$existing" && "$existing" != "(value not set)" ]] && has_existing=1
            else
                has_existing=0
                bottles_hkcr_has_default "$ext" && has_existing=1
            fi

            if [[ $has_existing -eq 1 ]]; then
                if [[ $FORCE_ASSOCIATIONS -eq 1 ]]; then
                    echo "$block_b64" | base64 -d >> "$MERGED_REG"
                    printf '\r\n' >> "$MERGED_REG"
                    BOTTLES_PAIRS_TO_APPLY+=("$pairs_json")
                    forced=$((forced + 1))
                else
                    skipped=$((skipped + 1))
                fi
            else
                echo "$block_b64" | base64 -d >> "$MERGED_REG"
                printf '\r\n' >> "$MERGED_REG"
                BOTTLES_PAIRS_TO_APPLY+=("$pairs_json")
                added=$((added + 1))
            fi
        done < <(for f in "${ASSOC_FILES_PRESENT[@]}"; do python3 "$REPO_ROOT/file-associations/parse-reg-associations.py" "$f"; done)

        log "file associations: $added new, $forced forced-overwrite, $skipped already present (unchanged)."

        if [[ $((added + forced)) -gt 0 ]]; then
            if [[ "$WINE_MANAGER" == "crossover" ]]; then
                WIN_MERGED_REG="Z:$(echo "$MERGED_REG" | sed 's/\//\\/g')"
                if CX_BOTTLE="$BOTTLE_NAME" /opt/cxoffice/bin/wine regedit /S "$WIN_MERGED_REG"; then
                    log "file-type associations imported."
                else
                    warn "wine regedit import of file-type associations failed -- bottle's associations may be partially updated."
                fi
            else
                assoc_import_failed=0
                for pairs_json in "${BOTTLES_PAIRS_TO_APPLY[@]}"; do
                    while IFS=$'\t' read -r pair_key pair_data; do
                        [[ -n "$pair_key" ]] || continue
                        wine_reg_add "$BOTTLE_NAME" "HKEY_CLASSES_ROOT\\$pair_key" "" REG_SZ "$pair_data" \
                            || assoc_import_failed=1
                    done < <(python3 -c "
import json, sys
for key, data in json.loads(sys.argv[1]):
    print(key + '\t' + data)
" "$pairs_json")
                done
                if [[ $assoc_import_failed -eq 0 ]]; then
                    log "file-type associations imported."
                else
                    warn "one or more file-type association registry writes failed -- bottle's associations may be partially updated."
                fi
            fi
        else
            log "no file-type association changes needed."
        fi
    else
        log "skipping file-type associations. Use --install-associations to skip this prompt."
    fi
fi

log "Restart eM Client in this bottle to pick up the change."
