#!/usr/bin/env bash
# Installs eM Client 11 (beta) into a CrossOver bottle by hand.
#
# eM Client 11 ships as an MSIX package (Microsoft Store-style), not a classic .exe/.msi
# installer -- there's nothing for CrossOver/Wine to run inside the bottle the way
# releases/10.4.5674/deploy.sh's target install already exists. This script gets a working,
# unpatched install into place manually: downloads the .NET 10 desktop runtimes (the app targets
# .NET 10, and a fresh bottle won't have it), installs them into the chosen bottle, resolves the
# app's own update feed to find the current MSIX bundle, downloads it, extracts the x86 package's
# contents directly into the bottle's classic "Program Files (x86)\eM Client" location (the same
# layout releases/11.0.196-beta/deploy.sh's patch pipeline already expects), and drops in a Start
# Menu shortcut.
#
# This does NOT apply any of this repo's Wine-compatibility patches -- run
# releases/11.0.196-beta/deploy.sh separately afterward for that (it works the same way against a
# bottle installed by this script as it does against one installed any other way, since it always
# regenerates its patches fresh from whatever's actually installed).
#
# Usage:
#   ./install-msix.sh                interactive: lists bottles found, prompts for choice
#   ./install-msix.sh --bottle NAME  skip bottle selection (bottle dir name under ~/.cxoffice/)
#   ./install-msix.sh -y|--yes       don't prompt if the chosen bottle isn't a Windows 11 template
#   ./install-msix.sh --help         full usage
#
# All downloads and extraction happen in a fresh temp directory under /tmp, cleaned up on
# success; left in place (path printed) for inspection if anything fails. The msixbundle download
# is large (800MB+) -- expect this to take a while on a slow connection.
#
# Requires: curl, python3 (to parse the .appinstaller XML manifest), unzip. No dotnet SDK needed
# -- unlike deploy.sh, this script does no IL patching, just file extraction and one silent
# installer run inside the bottle via wine.

set -euo pipefail

APPINSTALLER_URL="https://licensing.emclient.com/api/update/emclient.appinstaller?beta=true"
DOTNET_X64_URL="https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.11/windowsdesktop-runtime-10.0.11-win-x64.exe"
DOTNET_X86_URL="https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.11/windowsdesktop-runtime-10.0.11-win-x86.exe"
CXSTART_WINE="/opt/cxoffice/bin/wine"
CXMENU="/opt/cxoffice/bin/cxmenu"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
LNK_FILE="$REPO_ROOT/supporting/eM Client.lnk"

log()  { echo "[install-msix] $*"; }
warn() { echo "[install-msix] WARNING: $*" >&2; }
err()  { echo "[install-msix] ERROR: $*" >&2; }
die()  { err "$*"; exit 1; }

# ---------------------------------------------------------------------------
# Args
# ---------------------------------------------------------------------------

BOTTLE_OVERRIDE=""
ASSUME_YES=0
LIST_ONLY=0

print_help() {
    sed -n '2,32p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --bottle) BOTTLE_OVERRIDE="${2:-}"; shift 2 ;;
        -y|--yes) ASSUME_YES=1; shift ;;
        --list) LIST_ONLY=1; shift ;;
        -h|--help) print_help; exit 0 ;;
        *) die "unknown argument: $1 (see --help)" ;;
    esac
done

# ---------------------------------------------------------------------------
# Tool checks
# ---------------------------------------------------------------------------

for tool in curl python3 unzip; do
    command -v "$tool" >/dev/null 2>&1 || die "$tool not found -- required by this script."
done
[[ -f "$LNK_FILE" ]] || die "shortcut file not found: $LNK_FILE"
[[ -x "$CXSTART_WINE" ]] || die "CrossOver's wine binary not found at $CXSTART_WINE"

# ---------------------------------------------------------------------------
# Workdir -- everything downloaded/extracted lives here. Cleaned up on success; left in place
# (path printed) if anything fails, same convention as releases/*/deploy.sh.
# ---------------------------------------------------------------------------

TMPDIR="$(mktemp -d /tmp/emclient11-install-XXXXXX)"
CLEANUP_ON_EXIT=1
cleanup() {
    local status=$?
    if [[ $status -ne 0 || $CLEANUP_ON_EXIT -eq 0 ]]; then
        warn "leaving workdir for inspection: $TMPDIR"
    else
        rm -rf "$TMPDIR"
    fi
}
trap cleanup EXIT
log "working in $TMPDIR"

# ---------------------------------------------------------------------------
# Bottle discovery + selection. Unlike releases/*/deploy.sh, this doesn't filter for bottles that
# already have eM Client installed -- the whole point here is installing into one that doesn't
# yet. Every CrossOver bottle (anything with its own cxbottle.conf) is a candidate; the bottle's
# own "Template" setting (win11_64, win10_64, etc. -- confirmed present and reliable across every
# existing bottle checked) is used to warn if the chosen one isn't Windows 11, which is the only
# target this app has been confirmed to work on so far.
# ---------------------------------------------------------------------------

bottle_template() {
    grep -oP '^"Template"\s*=\s*"\K[^"]+' "$HOME/.cxoffice/$1/cxbottle.conf" 2>/dev/null || echo "?"
}

shopt -s nullglob
BOTTLES=()
for d in "$HOME"/.cxoffice/*/; do
    [[ -f "${d}cxbottle.conf" ]] && BOTTLES+=("$(basename "$d")")
done
shopt -u nullglob
[[ ${#BOTTLES[@]} -gt 0 ]] || die "no CrossOver bottles found under ~/.cxoffice/"

log "found ${#BOTTLES[@]} bottle(s):"
for b in "${BOTTLES[@]}"; do
    tmpl="$(bottle_template "$b")"
    note=""
    [[ "$tmpl" != win11_* ]] && note="  <- not Windows 11"
    log "  - $b  (template: $tmpl)$note"
done

if [[ $LIST_ONLY -eq 1 ]]; then
    exit 0
fi

if [[ -n "$BOTTLE_OVERRIDE" ]]; then
    BOTTLE_NAME="$BOTTLE_OVERRIDE"
    [[ -d "$HOME/.cxoffice/$BOTTLE_NAME" ]] || die "--bottle $BOTTLE_NAME not found under ~/.cxoffice/"
elif [[ ${#BOTTLES[@]} -eq 1 ]]; then
    BOTTLE_NAME="${BOTTLES[0]}"
else
    echo ""
    echo "Multiple bottles found. eM Client 11 is only confirmed working on a Windows 11 bottle"
    echo "-- pick one of those unless you specifically want to test elsewhere."
    select b in "${BOTTLES[@]}"; do
        if [[ -n "${b:-}" ]]; then BOTTLE_NAME="$b"; break; fi
    done
fi

BOTTLE_DIR="$HOME/.cxoffice/$BOTTLE_NAME"
TEMPLATE="$(bottle_template "$BOTTLE_NAME")"
if [[ "$TEMPLATE" != win11_* ]]; then
    warn "bottle '$BOTTLE_NAME' has template '$TEMPLATE', not a Windows 11 one."
    warn "eM Client 11 has only been confirmed working on a Windows 11 bottle so far."
    if [[ $ASSUME_YES -eq 1 ]]; then
        log "-y/--yes given, continuing anyway."
    else
        read -r -p "Continue anyway? [y/N] " reply
        [[ "$reply" =~ ^[Yy]$ ]] || die "aborted -- pick a Windows 11 bottle, or pass -y to override this check."
    fi
fi
log "target bottle: $BOTTLE_NAME ($BOTTLE_DIR)"

# ---------------------------------------------------------------------------
# Download the .NET 10 desktop runtime installers (both x64 and x86 -- the bottle's own Wine
# process architecture needs the x64 one, and the x86 eM Client executable itself (see below --
# the app installs as a 32-bit app even in a 64-bit bottle, matching every other eM Client build
# tested in this repo) needs the x86 one).
# ---------------------------------------------------------------------------

log "downloading .NET 10 desktop runtime (x64)..."
curl -fL --progress-bar -o "$TMPDIR/windowsdesktop-runtime-10.0.11-win-x64.exe" "$DOTNET_X64_URL"
log "downloading .NET 10 desktop runtime (x86)..."
curl -fL --progress-bar -o "$TMPDIR/windowsdesktop-runtime-10.0.11-win-x86.exe" "$DOTNET_X86_URL"

# ---------------------------------------------------------------------------
# Install both runtimes into the bottle, silently. These are Microsoft's standard Burn-based
# bootstrapper installers -- /install /quiet /norestart is the documented unattended-install
# invocation, same for both architectures.
#
# -u DOTNET_ROOT: a host-side DOTNET_ROOT (set by other tooling in this repo, e.g. for ilspycmd)
# leaks through into the wine child process and gets reinterpreted via CrossOver's own Y: drive
# mapping, pointing anything that resolves it at the HOST's dotnet install instead of the
# bottle's -- the exact bug already documented and fixed in releases/10.4.5674/deploy.sh's font
# install step. Neither of these installers is itself a .NET app, so this is precautionary rather
# than a confirmed issue here, but costs nothing to guard against.
# ---------------------------------------------------------------------------

install_runtime() {
    local exe="$1"
    local win_path="Z:$(echo "$exe" | sed 's/\//\\/g')"
    log "installing $(basename "$exe") into $BOTTLE_NAME (silent)..."
    CX_BOTTLE="$BOTTLE_NAME" env -u DOTNET_ROOT "$CXSTART_WINE" "$win_path" /install /quiet /norestart \
        || die "failed to install $(basename "$exe") -- see any Wine output above"
}
install_runtime "$TMPDIR/windowsdesktop-runtime-10.0.11-win-x64.exe"
install_runtime "$TMPDIR/windowsdesktop-runtime-10.0.11-win-x86.exe"
log ".NET 10 desktop runtimes installed."

# ---------------------------------------------------------------------------
# Resolve the current MSIX bundle URL from the app's own .appinstaller update feed (a small XML
# manifest -- see its own <MainBundle Uri="..."> attribute) rather than hardcoding a
# version-specific download URL, since that URL changes with every eM Client release.
# ---------------------------------------------------------------------------

log "fetching appinstaller manifest..."
curl -fL -o "$TMPDIR/emclient.appinstaller" "$APPINSTALLER_URL"

read -r MSIX_BUNDLE_URL MSIX_VERSION < <(python3 -c "
import sys
import xml.etree.ElementTree as ET
ns = {'a': 'http://schemas.microsoft.com/appx/appinstaller/2017'}
root = ET.parse('$TMPDIR/emclient.appinstaller').getroot()
mb = root.find('a:MainBundle', ns)
if mb is None or 'Uri' not in mb.attrib:
    sys.exit('MainBundle element or its Uri attribute not found')
print(mb.attrib['Uri'], mb.attrib.get('Version', '?'))
") || die "couldn't parse MainBundle Uri from the appinstaller manifest -- its format may have changed, check $TMPDIR/emclient.appinstaller by hand"

[[ -n "$MSIX_BUNDLE_URL" ]] || die "parsed an empty MainBundle Uri from the appinstaller manifest"
log "resolved eM Client $MSIX_VERSION: $MSIX_BUNDLE_URL"

# ---------------------------------------------------------------------------
# Download the bundle (a plain zip containing one .msix per architecture -- win-x86, win-x64,
# win-arm64, confirmed via `unzip -l` against a real bundle -- plus bundle-level Appx metadata)
# and extract just the x86 package from it. eM Client's own MSIX ships a flat file layout inside
# each architecture's .msix (also a plain zip) -- MailClient.exe and everything else sit directly
# at the archive root, no VFS/redirection subfolder -- confirmed by inspecting a real x86 .msix's
# own file listing, and matching this repo's own original/em-11.0.196/ pristine snapshot
# byte-for-byte in file count (2493 files both ways). This means the x86 .msix's entire contents
# can be extracted directly into the bottle's classic "Program Files (x86)\eM Client" location
# with no repackaging step.
# ---------------------------------------------------------------------------

log "downloading msixbundle (this is large, 800MB+, may take a while)..."
curl -fL --progress-bar -o "$TMPDIR/setup.msixbundle" "$MSIX_BUNDLE_URL"

log "extracting the x86 package from the bundle..."
mkdir -p "$TMPDIR/bundle-extract"
X86_ENTRY="$(unzip -Z1 "$TMPDIR/setup.msixbundle" | grep -i 'win-x86\.msix$' | head -1)"
[[ -n "$X86_ENTRY" ]] || die "couldn't find a *win-x86.msix entry inside the downloaded bundle -- its internal layout may have changed"
unzip -q -o "$TMPDIR/setup.msixbundle" "$X86_ENTRY" -d "$TMPDIR/bundle-extract"
X86_MSIX="$TMPDIR/bundle-extract/$X86_ENTRY"
[[ -f "$X86_MSIX" ]] || die "extraction reported success but $X86_MSIX doesn't exist"

INSTALL_DIR="$BOTTLE_DIR/drive_c/Program Files (x86)/eM Client"
log "installing eM Client into $INSTALL_DIR..."
mkdir -p "$INSTALL_DIR"
unzip -q -o "$X86_MSIX" -d "$INSTALL_DIR"
[[ -f "$INSTALL_DIR/MailClient.exe" ]] || die "extraction completed but MailClient.exe is missing from $INSTALL_DIR -- something's wrong with the package layout"
log "eM Client files installed."

# ---------------------------------------------------------------------------
# Start Menu shortcut -- a pre-built .lnk already pointing at
# C:\Program Files (x86)\eM Client\MailClient.exe (confirmed via a raw strings check against the
# tracked file), matching exactly where this script just installed to. Copied as-is, not
# regenerated.
# ---------------------------------------------------------------------------

STARTMENU_DIR="$BOTTLE_DIR/drive_c/users/crossover/AppData/Roaming/Microsoft/Windows/Start Menu/Programs"
mkdir -p "$STARTMENU_DIR"
cp "$LNK_FILE" "$STARTMENU_DIR/"
log "Start Menu shortcut installed."

# ---------------------------------------------------------------------------
# Ask CrossOver to rescan this bottle's Start Menu / Desktop and update its own native menu
# entries accordingly, so the new shortcut actually shows up without a manual "Install Application
# into Bottle" pass. Non-fatal if it fails -- the app is already fully installed and can still be
# launched directly (or the shortcut re-synced later by hand).
# ---------------------------------------------------------------------------

if [[ -x "$CXMENU" ]]; then
    log "syncing CrossOver's menu for this bottle..."
    "$CXMENU" --sync --bottle "$BOTTLE_NAME" || warn "cxmenu --sync failed -- the app is still installed, but its shortcut may need a manual menu refresh."
else
    warn "cxmenu not found at $CXMENU -- skipping menu sync. The app is still installed; refresh CrossOver's menu by hand if the shortcut doesn't appear."
fi

log "done."
log "  bottle:      $BOTTLE_NAME"
log "  eM Client:   $MSIX_VERSION"
log "  installed to: $INSTALL_DIR"
log ""
log "This installed a clean, UNPATCHED copy. Next: run"
log "  releases/11.0.196-beta/deploy.sh --bottle $BOTTLE_NAME"
log "to apply this repo's Wine-compatibility fixes."
