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
# layout releases/11.0.196-beta/deploy.sh's patch pipeline already expects), installs a set of
# ICU DLLs Wine doesn't provide but the app's spell-checker needs (see the ICU section below),
# and drops in a Start Menu shortcut.
#
# This does NOT apply any of this repo's Wine-compatibility IL patches -- those are a separate,
# optional step (releases/11.0.196-beta/deploy.sh), which this script offers to run for you at
# the end. It works the same way against a bottle installed by this script as it does against one
# installed any other way, since it always regenerates its patches fresh from whatever's actually
# installed.
#
# Usage:
#   ./install-msix.sh                interactive: offers to create a fresh bottle (see below),
#                                    then lists existing bottles found and prompts for choice
#   ./install-msix.sh --bottle NAME  skip bottle selection (bottle dir name under ~/.cxoffice/)
#   ./install-msix.sh --new-bottle NAME  skip the create/reuse prompt: always create a new
#                                    win11_64 bottle named NAME (must not already exist) and
#                                    install the Core Fonts prerequisite into it before proceeding
#   ./install-msix.sh -y|--yes       don't prompt on the Windows-11-template check or on whether
#                                    to run deploy.sh afterward (answers yes to both)
#   ./install-msix.sh --help         full usage
#
# Interactively (no --bottle/--new-bottle given), this first asks whether to create a brand new
# bottle. Answering yes creates a fresh win11_64 bottle (prompting for its name -- no default is
# offered, since a name is required and guessing one risks colliding with something the user
# already has) and launches CrossOver's own installer, pre-filled to install its "Core Fonts"
# prerequisite package (com.codeweavers.c4.6959 -- Arial, Times New Roman, Courier New, Verdana,
# Comic Sans MS, Impact, Georgia, Trebuchet MS, Andale Mono, Webdings) into it, via
# `cxinstaller --profileid`, before eM Client itself is ever installed into it.
#
# IMPORTANT: `cxinstaller --profileid` does NOT install anything by itself -- confirmed by
# reading CrossOver's own Python source (crossoverui.py, packageview.py): it's a GTK
# Gtk.Application whose command-line handling just pre-selects the profile and opens CrossOver's
# normal install wizard window (with a green "Install" button) -- it does not click that button
# for you, and neither it nor the individual cached font installer .exes under
# ~/.cxoffice/installers/ accept any silent/unattended flag (/Q, /S, /VERYSILENT were all tried
# and all still pop their own GUI dialog). This is a genuine, unavoidable manual step -- no GUI
# automation tool exists in this environment to click it programmatically (see CLAUDE.md's "Test
# loop" note). So this script launches the installer, prints an explicit instruction to click
# Install, and polls the new bottle's Fonts folder for up to
# CORE_FONTS_WAIT_TIMEOUT_SECS (10 minutes by default) before giving up and continuing anyway
# (with a warning) -- it does not block forever.
#
# Answering no to the "create a new bottle?" prompt falls through to the existing bottle
# discovery/selection prompt, unchanged, and does NOT touch Core Fonts at all -- that only
# happens as part of creating a brand new bottle.
#
# All downloads and extraction happen in a fresh temp directory under /tmp, cleaned up on
# success; left in place (path printed) for inspection if anything fails. The msixbundle download
# is large (800MB+) -- expect this to take a while on a slow connection.
#
# Requires: curl, python3 (to parse the .appinstaller XML manifest, and to generate the "sans"
# fallback font below), unzip, tar, dotnet SDK (needed here too, unlike previously -- for
# font-systemlink-writer, the same vendored-font installer tool releases/11.0.196-beta/deploy.sh
# uses). No IL patching happens in this script.
#
# ./install-msix.sh --install-fonts   install this repo's vendored fonts/*.ttf without the
#                                      license-consent prompt (same as deploy.sh's own flag)
# ./install-msix.sh --no-fonts        skip vendored font installation without the prompt

set -euo pipefail

APPINSTALLER_URL="https://licensing.emclient.com/api/update/emclient.appinstaller?beta=true"
DOTNET_X64_URL="https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.11/windowsdesktop-runtime-10.0.11-win-x64.exe"
DOTNET_X86_URL="https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.11/windowsdesktop-runtime-10.0.11-win-x86.exe"
# A fork of the official unicode-org/icu, built with unversioned (Microsoft-compatible) symbol
# names -- see the ICU installation section below for why this is needed and how it was verified.
ICU_URL="https://github.com/FaithLife-Community/icu/releases/download/72.1-custom%2B4/icu-win.tar.gz"
# Microsoft's own official download of the Aptos font family (Office's default font since 2023,
# replacing Calibri) -- see the Aptos installation section below for why this is needed.
APTOS_FONTS_URL="https://download.microsoft.com/download/8/6/0/860a94fa-7feb-44ef-ac79-c072d9113d69/Microsoft%20Aptos%20Fonts.zip"
# Google's own CDN, serving the widely-used `typeface-roboto` npm package's WOFF files (jsdelivr
# mirror) -- see the Roboto installation section below and woff-to-ttf.py's own doc comment for
# why WOFF (converted locally) rather than a direct .ttf link.
ROBOTO_CDN_BASE="https://cdn.jsdelivr.net/npm/typeface-roboto@1.1.13/files"
CXSTART_WINE="/opt/cxoffice/bin/wine"
CXMENU="/opt/cxoffice/bin/cxmenu"
CXBOTTLE="/opt/cxoffice/bin/cxbottle"
CXINSTALLER="/opt/cxoffice/bin/cxinstaller"
CORE_FONTS_PROFILEID="com.codeweavers.c4.6959"
# A file that only exists once Core Fonts has actually been installed (Arial.TTF isn't part of
# any bottle template, and eM Client itself hasn't been installed yet at this point) -- used to
# poll for completion of the manual "click Install" step, see create_bottle_with_core_fonts below.
CORE_FONTS_MARKER_FILENAME="Arial.TTF"
CORE_FONTS_WAIT_TIMEOUT_SECS=600
CORE_FONTS_POLL_INTERVAL_SECS=5

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
IL_PATCHES_DIR="$REPO_ROOT/il-patches"
FONTS_DIR="$REPO_ROOT/fonts"
LNK_FILE="$SCRIPT_DIR/eM Client.lnk"

log()  { echo "[install-msix] $*"; }
warn() { echo "[install-msix] WARNING: $*" >&2; }
err()  { echo "[install-msix] ERROR: $*" >&2; }
die()  { err "$*"; exit 1; }

# ---------------------------------------------------------------------------
# Args
# ---------------------------------------------------------------------------

BOTTLE_OVERRIDE=""
NEW_BOTTLE_NAME=""
ASSUME_YES=0
LIST_ONLY=0
INSTALL_FONTS=0
NO_FONTS=0

print_help() {
    sed -n '2,69p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'
}

while [[ $# -gt 0 ]]; do
    case "$1" in
        --bottle) BOTTLE_OVERRIDE="${2:-}"; shift 2 ;;
        --new-bottle) NEW_BOTTLE_NAME="${2:-}"; shift 2 ;;
        -y|--yes) ASSUME_YES=1; shift ;;
        --list) LIST_ONLY=1; shift ;;
        --install-fonts) INSTALL_FONTS=1; shift ;;
        --no-fonts) NO_FONTS=1; shift ;;
        -h|--help) print_help; exit 0 ;;
        *) die "unknown argument: $1 (see --help)" ;;
    esac
done

[[ -n "$BOTTLE_OVERRIDE" && -n "$NEW_BOTTLE_NAME" ]] && die "--bottle and --new-bottle are mutually exclusive"

# ---------------------------------------------------------------------------
# Tool checks
# ---------------------------------------------------------------------------

for tool in curl python3 unzip tar; do
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
#
# Before falling into that discovery/selection flow, interactively offer to create a brand new
# bottle instead -- the easiest way to get a genuinely clean install with no leftover state to
# confound testing. Creating one also installs CrossOver's own "Core Fonts" prerequisite package
# (com.codeweavers.c4.6959) into it via `cxinstaller --profileid`, the same mechanism CrossOver's
# own GUI "Install Windows Software" flow uses to resolve a profile's <predependency> chain --
# confirmed working: it resolves Core Fonts' own predependencies (the individual
# Arial/Times/Courier/Verdana/Comic Sans/Impact/Georgia/Trebuchet/Andale Mono/Webdings sub-
# packages) using installers cached under ~/.cxoffice/installers/ if present, or downloads them
# otherwise, and records the install in the bottle's own cxbottle.conf the same way any other
# CrossOver-managed prerequisite would be -- not just files dropped in by hand.
# ---------------------------------------------------------------------------

bottle_template() {
    grep -oP '^"Template"\s*=\s*"\K[^"]+' "$HOME/.cxoffice/$1/cxbottle.conf" 2>/dev/null || echo "?"
}

create_bottle_with_core_fonts() {
    local name="$1"
    local bottle_dir="$HOME/.cxoffice/$name"
    [[ -d "$bottle_dir" ]] && die "bottle '$name' already exists under ~/.cxoffice/ -- pick a different name"
    [[ -x "$CXBOTTLE" ]] || die "cxbottle not found at $CXBOTTLE"
    [[ -x "$CXINSTALLER" ]] || die "cxinstaller not found at $CXINSTALLER"

    log "creating new bottle '$name' (template: win11_64)..."
    "$CXBOTTLE" --bottle "$name" --create --template win11_64 \
        || die "cxbottle --create failed for '$name'"

    # See the header comment's IMPORTANT note: this opens a real window and pre-fills it, but
    # does NOT click Install for you -- that part is a genuine manual step, no way around it in
    # this environment.
    log "launching CrossOver's installer, pre-filled with the Core Fonts package..."
    "$CXINSTALLER" --bottle "$name" --profileid "$CORE_FONTS_PROFILEID" &
    disown

    echo ""
    echo ">>> A CrossOver window just opened, pre-filled to install 'Core Fonts' into '$name'."
    echo ">>> Click the green Install button there to continue."
    echo ">>> Waiting up to $((CORE_FONTS_WAIT_TIMEOUT_SECS / 60)) minutes for it to finish..."
    echo ""

    local marker="$bottle_dir/drive_c/windows/Fonts/$CORE_FONTS_MARKER_FILENAME"
    local waited=0
    while [[ ! -f "$marker" && $waited -lt $CORE_FONTS_WAIT_TIMEOUT_SECS ]]; do
        sleep "$CORE_FONTS_POLL_INTERVAL_SECS"
        waited=$((waited + CORE_FONTS_POLL_INTERVAL_SECS))
    done

    if [[ -f "$marker" ]]; then
        log "Core Fonts installed into '$name' (detected after ${waited}s)."
    else
        warn "timed out after ${CORE_FONTS_WAIT_TIMEOUT_SECS}s waiting for Core Fonts to finish installing into '$name'."
        warn "continuing without them -- install later by hand if needed:"
        warn "  $CXINSTALLER --bottle $name --profileid $CORE_FONTS_PROFILEID"
    fi
}

if [[ -n "$NEW_BOTTLE_NAME" ]]; then
    create_bottle_with_core_fonts "$NEW_BOTTLE_NAME"
    BOTTLE_NAME="$NEW_BOTTLE_NAME"
elif [[ -z "$BOTTLE_OVERRIDE" && $LIST_ONLY -eq 0 && $ASSUME_YES -eq 0 ]]; then
    echo ""
    read -r -p "Create a brand new bottle for this install (recommended for a clean test)? [y/N] " reply
    if [[ "$reply" =~ ^[Yy]$ ]]; then
        name=""
        while [[ -z "$name" ]]; do
            read -r -p "Name for the new bottle (required -- no default suggested): " name
            [[ -z "$name" ]] && warn "a bottle name is required -- try again."
        done
        create_bottle_with_core_fonts "$name"
        BOTTLE_NAME="$name"
    fi
fi

if [[ -z "${BOTTLE_NAME:-}" ]]; then
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
fi

BOTTLE_DIR="$HOME/.cxoffice/$BOTTLE_NAME"
BOTTLE_FONTS_DIR="$BOTTLE_DIR/drive_c/windows/Fonts"
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
# ICU DLLs. eM Client 11's spell-checker (MailClient.Utils.Text.WordBreak) calls ICU's
# BreakIterator C API directly via [DllImport("icuuc.dll")] -- an unversioned symbol name, the
# classic ICU4C convention. Real Windows 10 1703+ ships this itself (a built-in icu.dll plus
# compatible icuuc.dll/icuin.dll shims); Wine does not implement or ship ANY of these (confirmed:
# absent from Wine's own DLL directory and from a clean bottle's system32), so spell-check
# crashes with DllNotFoundException on essentially every keystroke in a compose window with
# spell-check enabled -- see reports/emclient11-icu-spellcheck-crash-findings.md for the full
# investigation, including why a random "icuuc.dll" found online (a thin forwarder to Windows'
# own built-in icu.dll, confirmed via objdump -- every export was a Forwarder RVA) does NOT fix
# this: it just moves the missing-dependency problem to a different, equally-absent DLL.
#
# This is a missing OS-level dependency, not a bug in eM Client's own binaries -- not something
# releases/11.0.196-beta/deploy.sh's IL patching can fix, so it's provisioned here at install
# time instead, the same way the .NET runtimes above are.
#
# Source: a fork of the official unicode-org/icu (https://github.com/FaithLife-Community/icu),
# built with unversioned (Microsoft-compatible) symbol names so icuuc.dll/icuin.dll export plain
# names like ubrk_open instead of ICU's default versioned form (e.g. ubrk_open_75) -- exactly
# what eM Client's hardcoded DllImport expects. Referenced from Wine's own bug tracker
# (https://bugs.winehq.org/show_bug.cgi?id=53354, "Wine should provide icu.dll") as the
# known-working fix for this exact gap. Verified before use: it's a direct GitHub fork of the
# real unicode-org/icu project (not an unrelated binary), and icuuc.dll/icuin.dll in this build
# are themselves thin forwarders to icu.dll (confirmed via objdump) -- but unlike the broken one
# found online, THIS package ships its own real icu.dll alongside them (~4-5MB, 2108 genuine
# exports including a non-forwarded ubrk_open), so the forwarding chain actually resolves. All
# three files must be installed together.
# ---------------------------------------------------------------------------

log "downloading ICU DLLs (Wine doesn't provide these; eM Client's spell-checker needs them)..."
curl -fL --progress-bar -o "$TMPDIR/icu-win.tar.gz" "$ICU_URL"

log "extracting and installing ICU DLLs..."
mkdir -p "$TMPDIR/icu-extract"
tar xzf "$TMPDIR/icu-win.tar.gz" -C "$TMPDIR/icu-extract"
for d in system32 syswow64; do
    [[ -d "$TMPDIR/icu-extract/windows/$d" ]] || die "expected windows/$d/ not found in the downloaded ICU archive -- its layout may have changed"
done

mkdir -p "$BOTTLE_DIR/drive_c/windows/system32" "$BOTTLE_DIR/drive_c/windows/syswow64" "$BOTTLE_DIR/drive_c/windows/globalization/ICU"
cp "$TMPDIR/icu-extract/windows/system32/"*.dll "$BOTTLE_DIR/drive_c/windows/system32/"
cp "$TMPDIR/icu-extract/windows/syswow64/"*.dll "$BOTTLE_DIR/drive_c/windows/syswow64/"
# This is Windows' own system-wide globalization data (icudtl.dat + timezone/locale .res files),
# installed to drive_c/windows/globalization/ICU/ -- a completely separate file from eM Client's
# OWN icudtl.dat that the MSIX extraction step above already placed directly in the app's install
# directory (that one is CEF/Chromium's own bundled copy, for the browser engine's internal use
# only). Different consumer, different location -- don't confuse the two.
cp "$TMPDIR/icu-extract/windows/globalization/ICU/"* "$BOTTLE_DIR/drive_c/windows/globalization/ICU/"

# Wine may have its own (stub, or simply absent) idea of these DLLs -- force it to use the real,
# just-installed files instead of anything it would otherwise try to resolve internally.
log "setting DLL overrides to native for icu/icuin/icuuc..."
for dll in icu icuin icuuc; do
    CX_BOTTLE="$BOTTLE_NAME" "$CXSTART_WINE" reg add "HKCU\\Software\\Wine\\DllOverrides" /v "$dll" /t REG_SZ /d native /f >/dev/null \
        || die "failed to set the Wine DLL override for $dll"
done
log "ICU DLLs installed and DLL overrides set."

# ---------------------------------------------------------------------------
# Aptos fonts -- Microsoft's default Office/Outlook font since 2023 (replacing Calibri). A fresh
# Wine bottle has no idea it exists, and it's genuinely common: any HTML email actually composed
# in a current Outlook/Word without an explicit font override specifies "Aptos" by name. When
# it's missing, DirectWrite correctly reports "not found" and Chromium correctly falls back to
# Arial -- not a bug, just a missing font, but a very visible one (confirmed live: message body
# text rendered in the wrong font family until this was installed -- see
# reports/emclient11-startup-stack-overflow-findings.md for the trace-based confirmation, both
# before this fix, where every "Aptos" lookup fell straight through to trying "Arial" next, and
# after, where it doesn't).
#
# Downloaded fresh from Microsoft's own official URL every run, the same "don't vendor a
# redistributable-questionable binary, fetch it fresh from the real source instead" pattern as
# the ICU DLLs and .NET runtimes above -- not part of this repo's own fonts/ (which is for files
# someone here holds a license to redistribute; this is a direct-from-Microsoft download that
# never touches this repo at all).
#
# Registers each of the 28 files in the family (regular through Black weights, Display/Mono/
# Narrow/Serif variants, each with Bold/Italic/BoldItalic) under its own real "Full Name" (name
# table nameID 4, Windows platform) rather than guessing filename-to-family-name mappings by
# hand -- the same technique releases/11.0.196-beta/make-sans-fallback-font.py already uses to
# read a font's real name table, just reading instead of patching here.
# ---------------------------------------------------------------------------

log "downloading Aptos fonts (Office's default font since 2023; Wine doesn't ship it, and a lot"
log "of real-world HTML email specifies it by name)..."
curl -fL --progress-bar -o "$TMPDIR/aptos-fonts.zip" "$APTOS_FONTS_URL"

log "extracting and installing Aptos fonts..."
mkdir -p "$TMPDIR/aptos-extract"
unzip -q -o "$TMPDIR/aptos-fonts.zip" -d "$TMPDIR/aptos-extract" -x "*.rtf"
aptos_count=$(ls "$TMPDIR/aptos-extract"/*.ttf 2>/dev/null | wc -l)
[[ "$aptos_count" -gt 0 ]] || die "no .ttf files found after extracting the Aptos fonts zip -- its layout may have changed"
cp "$TMPDIR/aptos-extract"/*.ttf "$BOTTLE_FONTS_DIR/"

python3 - "$BOTTLE_FONTS_DIR" "$BOTTLE_NAME" <<'PYEOF' > "$TMPDIR/aptos-reg-commands.sh"
import struct, glob, os, sys, shlex

def get_full_name(path):
    data = open(path, "rb").read()
    num_tables = struct.unpack_from(">H", data, 4)[0]
    name_off = None
    for i in range(num_tables):
        rec_off = 12 + i * 16
        if data[rec_off:rec_off + 4] == b"name":
            name_off = struct.unpack_from(">I", data, rec_off + 8)[0]
            break
    if name_off is None:
        return None
    _fmt, count, string_off = struct.unpack_from(">HHH", data, name_off)
    string_off += name_off
    best = None
    for i in range(count):
        rec_off = name_off + 6 + i * 12
        platform_id, _enc_id, _lang_id, name_id, length, offset = struct.unpack_from(">HHHHHH", data, rec_off)
        if name_id == 4 and platform_id == 3:
            best = data[string_off + offset:string_off + offset + length].decode("utf-16-be", errors="replace")
    return best

bottle_fonts, bottle_name = sys.argv[1], sys.argv[2]
for f in sorted(glob.glob(os.path.join(bottle_fonts, "Aptos*.ttf"))):
    full_name = get_full_name(f)
    if not full_name:
        continue
    base = os.path.basename(f)
    value_name = f"{full_name} (TrueType)"
    print(
        'CX_BOTTLE=' + shlex.quote(bottle_name) +
        ' "$CXSTART_WINE" reg add "HKLM\\\\Software\\\\Microsoft\\\\Windows NT\\\\CurrentVersion\\\\Fonts"'
        ' /v ' + shlex.quote(value_name) + ' /t REG_SZ /d ' + shlex.quote(base) + ' /f >/dev/null'
        ' || die ' + shlex.quote(f"failed to register Aptos font: {full_name}")
    )
PYEOF
source "$TMPDIR/aptos-reg-commands.sh"
log "$aptos_count Aptos font file(s) installed and registered."

# ---------------------------------------------------------------------------
# Roboto -- Google's own default Material Design / Android font, Apache-2.0 licensed (genuinely
# freely redistributable, unlike the Microsoft fonts above). Also confirmed missing from a fresh
# bottle, and also confirmed via trace to appear in real-world HTML email font-family stacks
# (commonly as part of a "system font stack" like `-apple-system, BlinkMacSystemFont, "Segoe UI",
# Roboto, Helvetica, Arial, sans-serif`) -- when missing, Chromium correctly falls through to
# whatever's next in the stack (usually Arial), same "not a bug, just a missing font" shape as
# Aptos. Installing both isn't required for eM Client to start or to look "correct" in the one
# specific case that was actually reported and fixed in this investigation (see the WindowMetrics
# MessageFont fix further below, which was the actual fix for that) -- but both are real, common
# fonts real-world email keeps asking for, so installing them means Chromium's font-fallback
# machinery does less repeated failed-lookup work, and anything that specifically wants Aptos or
# Roboto renders correctly instead of substituting.
#
# See woff-to-ttf.py's own doc comment for why this converts from WOFF rather than fetching a
# .ttf directly -- no reliable direct-.ttf source was found for Roboto specifically.
# ---------------------------------------------------------------------------

log "downloading and installing Roboto fonts (a common web/email font Wine doesn't ship)..."
mkdir -p "$TMPDIR/roboto"
for variant in 400:Regular 700:Bold 400italic:Italic 700italic:BoldItalic; do
    weight="${variant%%:*}"
    suffix="${variant##*:}"
    curl -fsSL -o "$TMPDIR/roboto/roboto-latin-$weight.woff" "$ROBOTO_CDN_BASE/roboto-latin-$weight.woff" \
        || die "failed to download Roboto ($weight) from $ROBOTO_CDN_BASE"
    python3 "$SCRIPT_DIR/woff-to-ttf.py" "$TMPDIR/roboto/roboto-latin-$weight.woff" "$BOTTLE_FONTS_DIR/Roboto-$suffix.ttf" \
        || die "woff-to-ttf.py failed converting Roboto ($weight)"
done
for suffix_name in "Regular:Roboto" "Bold:Roboto Bold" "Italic:Roboto Italic" "BoldItalic:Roboto Bold Italic"; do
    suffix="${suffix_name%%:*}"
    fullname="${suffix_name##*:}"
    CX_BOTTLE="$BOTTLE_NAME" "$CXSTART_WINE" reg add "HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts" /v "$fullname (TrueType)" /t REG_SZ /d "Roboto-$suffix.ttf" /f >/dev/null \
        || die "failed to register Roboto font: $fullname"
done
log "Roboto fonts installed and registered."

# ---------------------------------------------------------------------------
# The actual fix for the "message preview header uses the wrong font" bug -- see
# reports/emclient11-startup-stack-overflow-findings.md's own "Bug C" writeup for the full,
# occasionally-embarrassing investigation (multiple wrong turns -- Aptos and Roboto above are
# both real fixes for real, separate gaps, but neither was ever the cause of the specific header
# font issue this section fixes).
#
# eM Client's own `MailClient.Common.UI.FontManager.UIFont` (confirmed via decompile) defaults to
# `System.Drawing.SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont` -- and this bottle's
# `MessageBoxFont` (confirmed via a standalone .NET test program calling it directly) returned
# "Tahoma", not "Segoe UI" as real modern Windows (Vista+) does. `SystemFonts.MessageBoxFont`
# reads the Win32 `SystemParametersInfo(SPI_GETNONCLIENTMETRICS)` API's `lfMessageFont` field,
# which Wine sources from `HKCU\Control Panel\Desktop\WindowMetrics\MessageFont` -- a *binary*
# `LOGFONTW` structure (92 bytes: 5 `LONG` fields, 8 `BYTE` fields, then a 64-byte/32-WCHAR
# `lfFaceName`), not a plain string -- confirmed this key was entirely absent in a fresh bottle
# (Wine falls back to an internal Tahoma default when it's missing, rather than reporting
# whatever the OS's own real default would be).
#
# NOT the same mechanism as `HKLM\...\CurrentVersion\FontSubstitutes` (tried and confirmed
# ineffective for this specific bug earlier in the same investigation) -- that only affects GDI
# `CreateFont()` calls by literal name; `SystemFonts.MessageBoxFont` never calls `CreateFont("MS
# Shell Dlg")` at all, it goes through `SystemParametersInfo` directly.
# ---------------------------------------------------------------------------

log "setting the default Windows message-box font (HKCU WindowMetrics\\MessageFont) to Segoe UI --"
log "fixes eM Client's own UI font defaulting to Tahoma instead (see"
log "reports/emclient11-startup-stack-overflow-findings.md, \"Bug C\")..."
MESSAGEFONT_LOGFONT_HEX=$(python3 -c "
import struct
face = 'Segoe UI'.encode('utf-16-le')
face = face[:62].ljust(64, b'\x00')
height = -round(9.0 * 96 / 72)
blob = struct.pack('<iiiii8B', height, 0, 0, 0, 400, 0, 0, 0, 1, 0, 0, 0, 0) + face
print(blob.hex())
")
CX_BOTTLE="$BOTTLE_NAME" "$CXSTART_WINE" reg add "HKCU\\Control Panel\\Desktop\\WindowMetrics" /v "MessageFont" /t REG_BINARY /d "$MESSAGEFONT_LOGFONT_HEX" /f >/dev/null \
    || die "failed to set the WindowMetrics MessageFont registry value"
log "default message-box font set to Segoe UI."

# ---------------------------------------------------------------------------
# Optional: install vendored Windows fonts (Segoe UI, Tahoma, Calibri, etc., under fonts/) for
# better general font fidelity under Wine -- same tool, same fonts/, and the same
# --install-fonts/--no-fonts/license-consent-prompt behavior as
# releases/11.0.196-beta/deploy.sh's own font step (see that script's header comment and
# il-patches/font-systemlink-writer/'s own doc comment for the full rationale -- this is a
# straight port of that same logic to run here too, so a freshly installed bottle gets these
# fonts without a separate deploy.sh --install-fonts pass). These are genuine Microsoft font
# files, not freely redistributable, hence the license-consent prompt.
# ---------------------------------------------------------------------------

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
        if ! command -v dotnet >/dev/null 2>&1; then
            warn "dotnet SDK not found -- skipping vendored font installation (font-systemlink-writer"
            warn "needs it to build). Font files would still need registering by hand, or install"
            warn "dotnet and re-run with --install-fonts."
        else
            log "installing $font_count font(s) into the bottle..."
            cp "$FONTS_DIR"/*.ttf "$BOTTLE_FONTS_DIR/"

            mkdir -p "$TMPDIR/tools/font-systemlink-writer"
            cp "$IL_PATCHES_DIR/font-systemlink-writer/Program.cs" "$TMPDIR/tools/font-systemlink-writer/"
            cat > "$TMPDIR/tools/font-systemlink-writer/font-systemlink-writer.csproj" <<'EOF'
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
            if dotnet publish -c Release "$TMPDIR/tools/font-systemlink-writer" >"$TMPDIR/build-fontwriter.log" 2>&1; then
                FONTWRITER_EXE="$TMPDIR/tools/font-systemlink-writer/bin/Release/net10.0/win-x86/publish/font-systemlink-writer.exe"
                WIN_FONTWRITER_PATH="Z:$(echo "$FONTWRITER_EXE" | sed 's/\//\\/g')"
                # -u DOTNET_ROOT: see the .NET runtime installer step above -- same leak-through
                # issue, same fix.
                if CX_BOTTLE="$BOTTLE_NAME" env -u DOTNET_ROOT "$CXSTART_WINE" "$WIN_FONTWRITER_PATH"; then
                    log "fonts registered and SystemLink fallback entries set."
                else
                    warn "font-systemlink-writer failed -- font files were copied but not registered in the registry."
                fi
            else
                cat "$TMPDIR/build-fontwriter.log" >&2
                warn "failed to build font-systemlink-writer -- font files were copied but not registered in the registry."
            fi
        fi
    else
        log "skipping vendored font install (no license confirmation given). Use --install-fonts to skip this prompt."
    fi
fi

# ---------------------------------------------------------------------------
# Startup stack-overflow fixes (Bugs A and B, both required just to get the app to launch at
# all without crashing before it ever paints its main window -- see
# reports/emclient11-startup-stack-overflow-findings.md for the full investigation behind both).
# Like the ICU DLLs above, these are Wine/OS-level gaps, not eM Client bugs, so they're
# provisioned here at install time rather than as deploy.sh IL-patch stages.
#
# Bug A: Chromium's own WinRT accessibility text-scale-factor query
# (ui/display/win/uwp_text_scale_factor.cc, IUISettings2::get_TextScaleFactor) recurses
# unboundedly through Wine's windows.ui.dll WinRT stub. Disabling that module entirely makes the
# WinRT activation fail immediately and cleanly instead -- a failure Chromium already handles
# without looping.
# ---------------------------------------------------------------------------

log "disabling Wine's windows.ui.dll (Chromium's WinRT text-scale-factor query recurses"
log "unboundedly through it under Wine -- see reports/emclient11-startup-stack-overflow-findings.md)..."
CX_BOTTLE="$BOTTLE_NAME" "$CXSTART_WINE" reg add "HKCU\\Software\\Wine\\DllOverrides" /v "windows.ui" /t REG_SZ /d "" /f >/dev/null \
    || die "failed to set the windows.ui DLL override"

# ---------------------------------------------------------------------------
# Bug B: Chromium's font code (ui/gfx/platform_font_skia.cc) falls back to a hardcoded
# last-resort font family name -- literally "sans" -- when a requested font can't be resolved.
# Real Windows always has an OS-level generic-family resolution step that ensures something
# answers to that name; a fresh Wine bottle has nothing registered under the literal name "sans",
# so this last-resort fallback itself fails, and Chromium's own guard against retrying
# indefinitely doesn't hold under Wine -- producing an unbounded recursion that stack-overflows
# the main UI thread during startup, every time, deterministically.
#
# Fix: generate a font whose Windows-platform name-table family/full-name records are the
# literal string "sans" (make-sans-fallback-font.py, alongside this script -- see its own doc
# comment for why this is a generator rather than a vendored binary font file), based on
# whichever real font is already installed in this bottle. Tried in preference order since
# whether Core Fonts got installed as part of this exact run (vs. an already-set-up bottle this
# script was pointed at via --bottle) can vary.
# ---------------------------------------------------------------------------

log "generating a \"sans\" fallback font (fixes a Chromium/Wine startup stack overflow -- see"
log "reports/emclient11-startup-stack-overflow-findings.md)..."
# Segoe UI preferred: confirmed live that Chromium's own default-font initialization actually
# uses whatever font answers to this literal "sans" lookup for at least some rendered content
# (not just the true last-resort case the name suggests) -- basing it on Arial made message
# body/UI text visibly render in the wrong font (Arial instead of eM Client's real intended
# default, Segoe UI) once the startup crash itself was fixed. Only falls back to Arial/Tahoma/
# Verdana if Segoe UI wasn't installed (e.g. the vendored-fonts license prompt above was
# declined) -- those still fix the crash, just without matching the intended look as closely.
SANS_SOURCE=""
for candidate in segoeui.ttf Tahoma.ttf tahoma.ttf Arial.TTF arial.ttf Verdana.ttf verdana.ttf; do
    if [[ -f "$BOTTLE_FONTS_DIR/$candidate" ]]; then
        SANS_SOURCE="$BOTTLE_FONTS_DIR/$candidate"
        break
    fi
done
if [[ -z "$SANS_SOURCE" ]]; then
    warn "no known source font (Segoe UI/Tahoma/Arial/Verdana) found in $BOTTLE_FONTS_DIR --"
    warn "skipping the \"sans\" fallback font fix. The app may still stack-overflow on startup;"
    warn "see reports/emclient11-startup-stack-overflow-findings.md for how to fix this by hand."
else
    python3 "$SCRIPT_DIR/make-sans-fallback-font.py" "$SANS_SOURCE" "$BOTTLE_FONTS_DIR/sans.ttf" \
        || die "make-sans-fallback-font.py failed against $SANS_SOURCE"
    CX_BOTTLE="$BOTTLE_NAME" "$CXSTART_WINE" reg add "HKLM\\Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts" /v "sans (TrueType)" /t REG_SZ /d "sans.ttf" /f >/dev/null \
        || die "failed to register the \"sans\" fallback font"
    log "\"sans\" fallback font generated from $(basename "$SANS_SOURCE") and registered."
fi

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
log "This installed a clean, UNPATCHED copy (though the ICU DLLs above are already needed just"
log "to keep the app from crashing, independent of the optional Wine-compatibility patches below)."

# ---------------------------------------------------------------------------
# Offer to run the optional IL-patch pipeline right away, rather than just printing the command
# and leaving it as a separate manual step -- deploy.sh lives right alongside this script and
# already knows how to target the same bottle non-interactively.
# ---------------------------------------------------------------------------

DEPLOY_SH="$SCRIPT_DIR/deploy.sh"
if [[ -x "$DEPLOY_SH" ]]; then
    run_deploy=0
    if [[ $ASSUME_YES -eq 1 ]]; then
        run_deploy=1
    else
        echo ""
        read -r -p "Run deploy.sh now to apply this repo's Wine-compatibility patches to this bottle? [Y/n] " reply
        [[ -z "$reply" || "$reply" =~ ^[Yy]$ ]] && run_deploy=1
    fi

    if [[ $run_deploy -eq 1 ]]; then
        log "running deploy.sh --bottle $BOTTLE_NAME ..."
        "$DEPLOY_SH" --bottle "$BOTTLE_NAME"
    else
        log "skipped. Run it later with:"
        log "  $DEPLOY_SH --bottle $BOTTLE_NAME"
    fi
else
    warn "deploy.sh not found at $DEPLOY_SH -- skipping the offer to run it. Find it and run:"
    warn "  <that path>/deploy.sh --bottle $BOTTLE_NAME"
fi
