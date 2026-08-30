# Attachments fail to open — "There is no Windows program configured to open this type of file"

> Investigation started from a report of PDF attachments failing to open. That turned out to be
> wrong — PDFs already worked fine. The real, confirmed failure was `.docx` (and, once checked
> properly, a wide set of other office and common attachment types). Documented here as it
> actually happened, including the wrong turn, since the corrected symptom changes the whole
> investigation.

## Symptom

Right-clicking an attachment in em Client and choosing "Open" shows:

> There is no windows program configured to open this type of file.

Confirmed **not** PDF-specific: PDFs open correctly. Confirmed present for `.docx`, and — once
checked systematically (see "Investigation" below) — for a broad set of other attachment types
that have nothing in common except lacking a working OS-level file association.

## Root cause — CONFIRMED, and confirmed NOT an eM Client bug

Traced em Client's own attachment-open code path (`MailClient.dll`):

`MailClient.UI.Mail.CommonOperations.OpenAttachment` → `MailClient.UI.UIUtils.GetItemType` /
`IsSupportedItemType` → for anything **not** one of em Client's own internally-parsed item types
(`.eml`, `.msg`, `.emlt`, `.oft`, `.vcf`, `.ics`, `.asc`, `.crt`, `.cer`, `.note`, `.pst`, `.emdf`
— see `UIUtils.GetItemType`'s extension switch and `UIUtils.OpenItemStream`'s type switch, which
has no PDF/DOCX/image/etc. case at all) — falls straight through to
`UIUtils.OpenFileInDefaultApp`, which (under Wine, where `OperatingSystem.IsWindows()` is true)
does:

1. `MailClient.UI.ShellInterop.OpenItem(filename)` — a hand-rolled COM shell interop that mirrors
   exactly what Windows Explorer does internally to open a file:
   `SHParseDisplayName` → `SHBindToParent` → `IShellFolder.GetUIObjectOf(IID_IContextMenu)` →
   `QueryContextMenu` → `GetMenuDefaultItem` → `IContextMenu.InvokeCommand`.
2. If that throws, falls back to `Process.Start(new ProcessStartInfo(filename){
   UseShellExecute = true })`, forcing `Verb = "openas"` if `ProcessStartInfo.Verbs` comes back
   empty.

Both paths ultimately depend on the file's extension having a real association under
`HKEY_CLASSES_ROOT` in the bottle — exactly the same thing Explorer's own double-click depends on.
This is **not a bug in em Client's code**: it faithfully reproduces the same mechanism Explorer
uses (confirmed by decompiling `ShellInterop.OpenItem` — it's the textbook
`IContextMenu`-based "open like Explorer would" pattern, not a shortcut or workaround). The actual
problem is that **a fresh CrossOver bottle simply doesn't ship file-type associations for these
extensions.** `.pdf` happens to be associated (`pdffile` → `winebrowser.exe "%1"`) out of the box;
office documents and a long list of other common attachment types are not.

Confirmed the exact error text ("There is no Windows program configured to open this type of
file.") is a **Wine `shell32.dll` built-in string**, not something em Client generates — found by
disassembly, sitting in the same string-resource cluster as the Run dialog's "Browse"/"Executable
files" strings (`objdump`/offset-to-RVA mapping against
`/opt/cxoffice/lib/wine/i386-windows/shell32.dll`). This confirms the failure genuinely
originates from Wine's own shell resolving "no association", not from any em Client-side logic
error.

## Investigation

1. **Wrong initial hypothesis, ruled out.** Suspected em Client's internal PDF-preview feature
   was checking for an external PDF viewer and failing that check. Decompiled
   `UIUtils.OpenItemStream` — its type switch has no PDF case at all (PDF is `FileItemType.
   Unknown`, same bucket as everything else not in the internally-parsed list above). There is no
   "is a PDF viewer available" check anywhere in the attachment-open path. This hypothesis was
   dropped once PDFs were actually re-tested and confirmed working.
2. **`cxwinassoc` (CrossOver's own file-association tool) tried first, found unreliable** for
   this — didn't correctly land working associations. Abandoned in favor of direct registry
   `.reg` import via `wine regedit /S`.
3. **Confirmed `wine regedit /S` is safe here**, unlike the `FontLink\SystemLink` case documented
   in `reports/splash-tip-icon-findings.md` — that bug was specific to `REG_MULTI_SZ` import;
   these are plain `REG_SZ` default-string values, and manual test-imports matched the working
   `.pdf` entry's format byte-for-byte (`wine reg query`, not just a `.reg` re-export).
4. **Systematically checked every common email-attachment extension** against a bottle's live
   registry (`wine reg query "HKCR\.<ext>" /ve`) rather than guessing. Found three distinct
   broken states, not just "missing":
   - Extension key doesn't exist at all: `.webp`, `.heic`, `.rar`, `.7z`, `.json`, `.md`, `.m4a`,
     `.ogg`, `.mp4`, `.mov`, `.wmv`, `.mkv`, and every office-document extension checked.
   - Extension key exists but its default value is unset (`(value not set)`, no ProgID at all):
     `.bmp`, `.tif`/`.tiff`, `.ico`, `.zip`, `.tar`, `.gz`, `.mp3`, `.wav`, `.avi`.
   - Extension key exists **and has a ProgID**, but that ProgID's own `shell\open\command` key
     doesn't exist — a subtler broken case: `.svg`, which points at `svgfile`, a ProgID with no
     command definition anywhere in the bottle.
   - Already working out of the box, deliberately left untouched: `.jpg`/`.jpeg`, `.png`, `.gif`,
     `.txt`, `.htm`/`.html`, `.xml`.

## Fix

Two tracked `.reg` files under `file-associations/`:

- `office-associations.reg` — 46 extensions: Word (`doc/docx/docm/dot/dotx/dotm/rtf`), Excel
  (`xls/xlsx/xlsm/xlt/xltx/xltm/xlsb/csv`), PowerPoint (`ppt/pptx/pptm/pot/potx/potm/pps/ppsx/
  ppsm`), Visio (`vsd/vsdx/vsdm/vss/vssx/vssm/vst/vstx/vstm`), Publisher (`pub`), and OpenDocument
  — both zipped (`odt/ott/ods/ots/odp/otp/odg/otg`) and flat-XML (`fodt/fods/fodp/fodg`) variants.
- `common-attachments.reg` — 23 extensions: images (`bmp/tif/tiff/ico/webp/heic/svg`), archives
  (`zip/rar/7z/tar/gz`), audio (`mp3/wav/m4a/ogg`), video (`mp4/mov/avi/wmv/mkv`), and two text
  formats that turned up missing too (`json/md`).

Every extension gets its own `EMClientWinePort.<ext>` ProgID — a namespace deliberately not
reused from any real Microsoft Office/LibreOffice ProgID, so these entries can never collide with
(or be mistaken for) a genuine Office suite installed in the same bottle later — pointed at the
exact same handler already proven working for `.pdf`:
`"c:\windows\system32\winebrowser.exe" "%1"` (byte-for-byte matching the pre-existing `.pdf`
entry's command string, confirmed via `wine reg query`). `winebrowser.exe` hands the file to the
host's own `xdg-open`/MIME-configured Linux application, exactly like the already-working `.pdf`
association does.

`file-associations/parse-reg-associations.py` splits each `.reg` file into its individual
per-extension block pairs (the `.ext` → ProgID stanza and the ProgID's `shell\open\command`
stanza). `releases/<version>/deploy.sh` uses this to import **selectively**, not via a blind
`wine regedit /S` of the whole file: for each extension, it checks the target bottle's live
registry (`wine reg query "HKCR\.<ext>" /ve`) and only imports the extensions with no existing
association (a missing key, or a present key whose value is `(value not set)`), building one
filtered merged `.reg` file and importing that via `wine regedit /S`. This means a bottle with a
real Office/LibreOffice install already correctly associating some of these extensions is left
untouched. `--force-associations` overwrites everything regardless (needed for the `.svg` case
specifically — see below); `--install-associations`/`--no-associations` answer the prompt
non-interactively.

One accepted limitation, matching the project's own stated preference for simplicity here: the
check is "does the extension already have *a* value", not "does it point somewhere that works".
`.svg`'s pre-existing dangling `svgfile` ProgID counts as "already associated" under this check
and is skipped by default — `--force-associations` is required to actually fix `.svg`.

Two incidental bugs found and fixed in `deploy.sh` while wiring this in (unrelated to the
association fix itself, but would have silently broken it):

1. The pre-existing "already patched" short-circuit exited the whole script before ever reaching
   the fonts/associations sections — meaning neither could ever run against an already-patched
   bottle, the normal case for any bottle that's been through this pipeline before. Fixed so only
   the DLL patch pipeline itself is skipped; fonts/associations now always run.
2. `wine reg query`'s expected non-zero exit for a genuinely-missing key was propagated through
   `set -o pipefail` into a script-aborting `set -e` failure, since the query result was captured
   via `existing=$(... | sed ... | tr ...)`. Fixed by wrapping the pipeline in a subshell with
   `|| true`.

## Verification

- Manual `.reg` import into `emClient_win_8_x64`: all 46 + 23 = 69 extensions confirmed correct
  via `wine reg query` (both the extension → ProgID mapping and the ProgID's command string,
  byte-for-byte matching the working `.pdf` entry), pre-existing `.pdf` association confirmed
  undisturbed.
- `deploy.sh --install-associations`: after deliberately removing `.docx` and `.zip`'s
  associations, reported "2 new, 0 forced-overwrite, 67 already present (unchanged)" — confirmed
  both extensions correctly restored, everything else left alone.
- Re-run with no changes needed: reported "0 new, 0 forced-overwrite, 69 already present
  (unchanged)" and correctly skipped the `wine regedit` call entirely — idempotent.
- `deploy.sh --force-associations`: reported "0 new, 69 forced-overwrite, 0 already present" —
  confirmed all 69 re-imported.
- Live-tested in the actual em Client UI on `emClient_win_8_x64`: opening `.docx`, `.csv`, `.odt`,
  and other attachment types via right-click → Open now launches the correct external viewer.

## Status

Fixed, deployed via `deploy.sh`, and confirmed working. Testing bottle shifted permanently from
`emClient_win_7_x64` to `emClient_win_8_x64` for this and future investigations (already set up
with test data). User confirmed after live-testing multiple attachment types: "It worked."
