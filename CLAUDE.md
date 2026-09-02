# MailClient — CrossOver stabilization (not a native Linux port)

- original/**      — untouched, read-only. Drop a fresh eM Client install here (overwrite
                      wholesale) when the app updates; everything below regenerates from it.
- decompiled/**     — ilspycmd -p output. If it rebuilds clean, edit here, swap the DLL in.
                      Not used by any fix so far — every fix to date has gone through the IL
                      patcher instead (see "Patch pipeline" below), since none of the touched
                      assemblies (MailClient.dll, MailClient.Common.UI.dll) round-trip cleanly
                      through ilspycmd -p at their size.
- il-patches/**      — ~/tools/il-patcher (Cecil-based patcher/scanner) plus this project's
                      patch specs and a synced copy of the tool's own source
                      (il-patches/il-patcher-Program.cs — copy back to
                      ~/tools/il-patcher/Program.cs and `dotnet build -c Release` before use;
                      the ~/tools/ copy is gitignored, this copy is the tracked source of truth).
                      A second, smaller Cecil tool lives alongside it for the license-activation
                      fix (Stage 5 below): il-patches/license-oaep-patcher-Program.cs (copy to
                      ~/tools/license-oaep-patcher/Program.cs; console app, net10.0, references
                      Mono.Cecil 0.11.6 — same shape as il-patcher's own untracked .csproj) and
                      il-patches/license-oaep-patcher-patch-deps-json.py (copy anywhere, it just
                      needs a path argument). It's a separate tool rather than a new il-patcher
                      mode because this fix's shape — redirecting a call to a newly-added sibling
                      assembly — doesn't fit the existing tool's per-fix flag pattern as cleanly.
                      il-patches/MailClient.Licensing.BouncyCastlePatch/ is the full tracked
                      source (OaepPatch.cs + its .csproj) of the new helper assembly Stage 5
                      builds and deploys — this one *is* shipped into the bottle, unlike the
                      patcher tools, so it's tracked in full rather than as a "copy back" source
                      file. Its .csproj HintPaths BouncyCastle.Cryptography.dll from
                      `original/` by absolute path — adjust if building from a different checkout
                      location.
                      il-patches/font-systemlink-writer/ (tracked in full, same reason as the
                      BouncyCastlePatch helper — it's built and run against a live bottle, not
                      just a dev-time tool) registers fonts/*.ttf in a target bottle's registry
                      and adds Wine FontLink\SystemLink fallback entries, via the real Win32
                      registry API rather than a `.reg` file import — `wine regedit /S` has a
                      confirmed multi-string-value import bug in this Wine build (see
                      reports/splash-tip-icon-findings.md). Optional, wired into
                      releases/<version>/deploy.sh's `--install-fonts` step; not part of the core
                      IL-patch pipeline and not required by any of it.
                      il-patches/notification-repro-stub/ (tracked in full, same reason as
                      font-systemlink-writer/ above) is a minimal standalone WinForms app
                      replicating FormGenericNotification's layered-window mechanism piece by
                      piece, built to test hypotheses about the notification empty-box bug (see
                      Status below and reports/notification-empty-until-fade-findings.md) far
                      faster than patching MailClient.dll via Cecil for every experiment — normal
                      C# source, `dotnet publish` + `wine <exe>`, no IL patching involved. Not
                      wired into deploy.sh or any pipeline; a standalone diagnostic tool, run
                      manually. Five iterations tried against the real bug, none reproduced it —
                      see the findings report for what that ruled out.
                      il-patches/output*/ and il-patches/backup/ are gitignored build output —
                      regenerate via the pipeline below, don't hand-edit or commit them.
- reports/crossover-backlog.json — one entry per CONFIRMED freeze point
                                    (cxlog + VS thread stacks), not a static API scan.
                                    Not populated yet — the two bugs fixed so far were found via
                                    the investigation method below, not a pre-existing backlog.
- reports/cxlog.txt — CX_DEBUGMSG trace, most recent run. Large (100–250MB); gitignored.
  reports/cxlog-*.txt.bak — earlier trace runs kept during investigation, also gitignored.
- reports/*-findings.md — one file per confirmed bug, each documenting root cause, the fix, and
  (importantly) fix attempts that turned out to be wrong and why, so the same dead end isn't
  re-explored next time. Read these before starting new investigation.
- supporting/** — reference screenshots (both "broken" and "known-good/working" states) used to
  confirm fixes visually. Tracked in git; add new ones here when reporting or confirming a bug.
- fonts/** — genuine Microsoft TrueType fonts (Segoe UI family, Segoe UI Emoji/Symbol, Segoe
  MDL2 Assets/Fluent Icons, Tahoma, Calibri — 30 files) the app expects but the bottle doesn't
  ship. Tracked in git on the basis that whoever adds files here holds a valid license to use
  them (these are not open-licensed — don't add font files here without one). Installed into a
  bottle only on explicit opt-in — see `releases/<version>/deploy.sh --install-fonts` and
  `il-patches/font-systemlink-writer/` above. Known to improve general font availability/fidelity
  but confirmed NOT sufficient by itself to fix Wine's emoji-glyph rendering gap — see
  `reports/splash-tip-icon-findings.md`.
- file-associations/** — `.reg` files fixing attachment types eM Client can't launch an external
  viewer for because a fresh CrossOver bottle has no OS-level file association for them (not an
  eM Client bug — its own attachment-open code faithfully mirrors Explorer's own mechanism; see
  `reports/office-file-associations-findings.md`). `office-associations.reg` covers MS Office
  (Word/Excel/PowerPoint/Visio/Publisher + CSV) and OpenDocument formats;
  `common-attachments.reg` covers images/archives/audio/video plus `.json`/`.md`. Every extension
  gets its own `EMClientWinePort.<ext>` ProgID pointed at the same `winebrowser.exe` handler
  already proven working for `.pdf`, deliberately namespaced so these entries can never collide
  with a real Office/LibreOffice install added to the bottle later. `parse-reg-associations.py`
  splits each file into per-extension blocks so `releases/<version>/deploy.sh` can import only
  the extensions actually missing an association on the target bottle (see its own bullet below)
  — the `.reg` files are also valid to import as-is by hand
  (`wine regedit /S file-associations/<file>.reg`) if you want every entry unconditionally.
- releases/<version>/deploy.sh — the actual, practical way to deploy: a single self-contained
  script that finds installed eM Client bottles, reads and reports each one's version, warns (and
  asks) before proceeding against a version other than the one this release was built and tested
  against, backs up the live files, regenerates every patch stage fresh from whatever assemblies
  it actually found (never copies pre-built DLLs out of this repo — an update-prone app makes a
  stale pre-built copy actively dangerous, not just inconvenient), runs every "Verify before
  deploying" check from the "Patch pipeline" section below automatically, and only then deploys —
  rolling back (renaming whatever it wrote to `.new`, restoring the backup) if any step from that
  point on fails. `./deploy.sh --list` reports found bottles + versions without patching anything;
  `./deploy.sh --bottle NAME` skips interactive bottle selection; `-y`/`--yes` skips the
  version-mismatch confirmation prompt; `--install-fonts`/`--no-fonts` answer the fonts/ license
  consent prompt (see below) non-interactively, for scripted/repeated runs. Safe to re-run —
  checks whether the target is already patched (an assembly-reference marker, not file size —
  see `il-patcher --check-patched`'s doc comment) and exits cleanly if so; `--force` skips that.
  Note this short-circuit only skips the DLL patch pipeline itself — fonts and file-type
  associations (below) are independent of it and always still run, since most bottles this
  script targets will already be patched from a previous run. `--install-associations`/
  `--no-associations` answer the file-associations/ prompt non-interactively;
  `--force-associations` also overwrites extensions that already have some association set (see
  file-associations/** above and `reports/office-file-associations-findings.md`) — by default
  only extensions with no existing association are touched, checked individually against the
  target bottle's live registry, so a bottle with a real Office/LibreOffice install isn't
  disturbed.
  Needs dotnet SDK (prints per-distro install instructions and exits if missing) and python3;
  fetches ilspycmd itself into a temp dir if not already installed. **When eM Client updates:** don't edit an existing `releases/<version>/deploy.sh` in
  place — copy the whole `releases/<version>/` folder to a new `releases/<new-version>/`, retest
  by hand against the new build (same process as "Investigation method" below — diff what
  actually changed rather than assuming), and adjust whatever patch logic broke in the new
  folder's copy. Each version's script stays a frozen, working reference for that version.
  `releases/*/backups/` (gitignored) is where each deploy run's pre-patch backup lands.

Main bottle: emClient_win_7_x64. Exe: MailClient.exe. Bottle path:
`~/.cxoffice/emClient_win_7_x64/`; `Z:\` inside the bottle maps to `/` on the Linux side, which
is how diagnostic instrumentation (see below) writes logs readable directly from outside Wine.
A secondary bottle, `emClient_win_7_x64_2` (same layout, same drive_c path shape), exists for
testing changes that would disturb the main bottle's state — e.g. the License Activation fix was
tested there once the main bottle had already been activated, to keep a never-activated instance
available for any future licensing-related testing without needing yet another fresh bottle.
Deploy commands need the `$BOTTLE` path swapped accordingly when testing there instead of main
(or use `releases/<version>/deploy.sh --bottle NAME`, which handles this automatically).

**Multi-OS testing:** additional bottles per Windows version the installer might target
differently — `emClient_win_8_x64`, `emClient_win_10_x64`, and `emClient_win_11_x64`, all
confirmed working with the full deploy.sh pipeline (Windows 10/11 run the Microsoft Store variant
of the app, with no classic License menu entry — licensing there is presumably handled via
`MicrosoftStoreLicenseSource` instead, not yet investigated). `original/` is split into
per-OS-version subfolders (`original/7/`, `original/8/`, more to come) — confirmed `original/7`
and `original/8` are byte-for-byte identical (2408 files, zero diffs, matching hashes on every
touched assembly), i.e. the eM Client installer doesn't differentiate its payload by target OS
version, at least for these two. Worth re-confirming for each new OS subfolder as it's added, in
case a later Windows version does differ.

**Primary testing bottle shifted permanently from `emClient_win_7_x64` to `emClient_win_8_x64`**
(already set up with test data) — use bottle 8 for new investigation/testing going forward unless
there's a specific reason to use another bottle (e.g. `emClient_win_7_x64_2`'s never-activated
license state, kept available for licensing-related testing).

git is available in this environment (it wasn't in earlier sessions — if CLAUDE.md you're
reading elsewhere says otherwise, this note supersedes it). Local commit identity for this repo
is set to `Claude Code <ai@cdmdotnet.com>` (`git config --local`, not global — never set global
git config without being asked). Commit a checkpoint before starting a new patch attempt, and
after each confirmed-working fix.

Test loop: run under CrossOver with tracing (see "Investigation method" below), reproduce the
target bug's steps, check the target fixme/err/symptom is gone with no new freeze or crash. No
UI automation tooling exists in this environment (no xdotool/ydotool/xte) — a human has to
actually click through the app. `xwininfo`, `wmctrl`, `xprop` (read-only X11 inspection) ARE
available and useful for checking live window geometry without needing automation.

## Patch pipeline — regenerate everything from a fresh `original/`

Run in order; each stage's output directory is the next stage's input. All commands assume
`~/tools/il-patcher` is built (`cd ~/tools/il-patcher && dotnet build -c Release`) from
`il-patches/il-patcher-Program.cs` (copy it to `~/tools/il-patcher/Program.cs` first if rebuilding
from a clean checkout — the tool itself isn't tracked in this repo's git history under its own
path), and `~/tools/license-oaep-patcher` + `~/tools/MailClient.Licensing.BouncyCastlePatch` are
built the same way from their `il-patches/` tracked sources (Stage 5 below needs both).

```bash
# Note: use $HOME, not ~, inside these quoted assignments -- ~ does not expand inside double
# quotes in bash, so ILP="dotnet ~/tools/..." silently fails to resolve when $ILP is invoked.
ILP="dotnet $HOME/tools/il-patcher/bin/Release/net10.0/il-patcher.dll"
ILP2="dotnet $HOME/tools/license-oaep-patcher/bin/Release/net10.0/license-oaep-patcher.dll"

# Stage 1: InterpolationMode.Bilinear fix (splash screen banner + Settings icon resize).
# Touches MailClient.dll (FormSplashScreen.OnPaintBackground) and
# MailClient.Common.UI.dll (CommonPaintUtils.ResizeImage's lambda).
$ILP --patch il-patches/stage1-interpolation-mode.json original/ il-patches/output/

# Stage 2: AllPaintingInWmPaint fix (Wine BeginPaint/WM_NCPAINT clip-region bug workaround).
# Touches MailClient.Common.UI.dll only (ControlDataGrid.initialize()).
$ILP --patch-allpaintinginwmpaint il-patches/output/ il-patches/output-stage2/

# Stage 3: Settings panel Load-event fix.
# Touches MailClient.dll only (formSettings's public constructor).
$ILP --patch-settings-refresh il-patches/output-stage2/ il-patches/output-stage3-final/

# Stage 4: category-click crash fix (Integration.IsDefaultClientVista).
# Touches MailClient.dll only. If you add any further exception-handler patch after this
# one, verify with --dump-handlers before deploying -- see "IL-patching lessons".
$ILP --patch-default-client-notimpl il-patches/output-stage3-final/ il-patches/output-stage4/

# Stage 5: License Activate RSA-OAEP decrypt fix (redirects DecryptAndVerify's OAEP calls to a
# new BouncyCastle-backed helper assembly instead of Wine's broken native RSA.Decrypt path).
# Touches MailClient.dll only, and adds a new sibling assembly. Build the helper first:
(cd ~/tools/MailClient.Licensing.BouncyCastlePatch && dotnet build -c Release)
mkdir -p il-patches/output-stage5
$ILP2 il-patches/output-stage4/MailClient.dll \
  ~/tools/MailClient.Licensing.BouncyCastlePatch/bin/Release/net8.0/MailClient.Licensing.BouncyCastlePatch.dll \
  il-patches/output-stage5/MailClient.dll
# output-stage5/ needs the rest of the stage-4 tree too (everything Stage 5 doesn't touch) plus
# the new helper assembly, to be a complete deployable directory:
rsync -a --ignore-existing il-patches/output-stage4/ il-patches/output-stage5/
cp ~/tools/MailClient.Licensing.BouncyCastlePatch/bin/Release/net8.0/MailClient.Licensing.BouncyCastlePatch.dll il-patches/output-stage5/

# Stage 6: License dialog "Get a license" button icon fix. NOT a Wine gap -- the button's own
# Text resource has two literal U+0083 control characters baked into MailClient.dll's embedded
# resource data (see reports/license-icon-findings.md for how this was confirmed isolated, not
# a font-substitution issue). Touches MailClient.dll only, a raw byte-level resource edit (no
# IL, no exception handlers) -- verify by confirming output size is byte-identical to the input
# (see "Verify" below), which proves the resource container's offset table wasn't disturbed.
$ILP --patch-license-icon il-patches/output-stage5/ il-patches/output-stage6/

# Stage 7: splash-screen tip label icon fix. IS a Wine gap (unlike Stage 6) -- the label's own
# Text resource is a genuine emoji (U+1F4A1) Wine has no glyph for; see
# reports/splash-tip-icon-findings.md for the font-linking fix that was tried first and why it
# didn't pan out. Same raw byte-level resource edit pattern as Stage 6.
$ILP --patch-splash-tip-icon il-patches/output-stage6/ il-patches/output-final/

# Deploy: swap all three touched/added files into the live bottle (back up the bottle's current
# copies first if you haven't already — see "Rollback" below), then patch deps.json so the new
# assembly resolves at runtime (this is a real deps.json-managed deployment, 247+ libraries
# listed — a dropped-in DLL isn't found without an explicit entry; the script is idempotent).
BOTTLE="/home/$USER/.cxoffice/emClient_win_7_x64/drive_c/Program Files (x86)/eM Client"
cp il-patches/output-final/MailClient.dll "$BOTTLE/MailClient.dll"
cp il-patches/output-final/MailClient.Common.UI.dll "$BOTTLE/MailClient.Common.UI.dll"
cp il-patches/output-final/MailClient.Licensing.BouncyCastlePatch.dll "$BOTTLE/"
python3 il-patches/license-oaep-patcher-patch-deps-json.py "$BOTTLE/MailClient.deps.json"
```

**In practice, use `releases/<version>/deploy.sh` instead of typing the above by hand** — it runs
every stage above (against whatever's actually installed, not a pre-built copy), every check in
"Verify" below, backs up, deploys, and can roll back on failure, all in one command. The manual
commands above remain the reference for understanding/debugging what each stage actually does;
see the release script's own header comment and `il-patches/**`'s bullet above for details on
when/why you'd still reach for the individual `$ILP`/`$ILP2` invocations directly (mainly:
investigating why a specific stage failed, or building a brand new stage before it's proven
enough to fold into a release script).

Verify before deploying, every time — this has caught real bugs (see "IL-patching lessons"
below):
```bash
# Interpolation sites should show exactly Bilinear at the two known offsets, 13 total unchanged:
$ILP il-patches/output-final/    # (no --patch flag = scan mode)

# Decompile-and-read anything you just patched with instruction insertion (not just an
# operand rewrite) before deploying — ilspycmd will surface invalid IL as a decompile error:
export DOTNET_ROOT=~/.dotnet   # ilspycmd needs this set in this environment (dotnet-install.sh
                                # layout specifically — this dev environment's dotnet lives under
                                # ~/.dotnet; on a global/apt/dnf install, derive DOTNET_ROOT from
                                # `readlink -f "$(command -v dotnet)"`'s directory instead, as
                                # deploy.sh now does, rather than hardcoding this path)
ilspycmd -m "M:MailClient.UI.Forms.formSettings.formSettings_Load(System.Object,System.EventArgs)" il-patches/output-final/MailClient.dll
ilspycmd -t "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid" il-patches/output-final/MailClient.Common.UI.dll | grep AllPaintingInWmPaint
ilspycmd -t "MailClient.Licensing.DecryptAndVerify" il-patches/output-final/MailClient.dll | grep OaepPatch

# Any patch touching exception handlers (Stage 4 and beyond) additionally needs the handler
# table itself checked — decompiling clean is not sufficient (see "IL-patching lessons"):
$ILP --dump-handlers il-patches/output-final/MailClient.dll MailClient.Utils.Integration IsDefaultClientVista

# Stages 6-7 are raw resource byte edits, not IL -- verify with a size check (must match the
# stage-5 output exactly, proving the .resources offset table wasn't disturbed by either) plus a
# content check on each:
stat -c%s il-patches/output-stage5/MailClient.dll il-patches/output-final/MailClient.dll   # must be equal
ilspycmd --resource "MailClient.UI.Forms.formLicense.resources/buttonGetLicense.Text" -o /tmp il-patches/output-final/MailClient.dll && xxd /tmp/buttonGetLicense.Text | tail -2   # should end c2 a0 c2 a0, not c2 83 c2 83
ilspycmd --resource "MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text" -o /tmp il-patches/output-final/MailClient.dll && xxd /tmp/labelTip.Text   # should read c2 a0 c2 a0, not f0 9f 92 a1
```

**Rollback:** back up the bottle's current DLLs before the first-ever deploy in a fresh bottle
(`cp "$BOTTLE/MailClient.dll" il-patches/backup/MailClient.dll.orig`, same for
MailClient.Common.UI.dll) — verify they're byte-identical to `original/` first via `md5sum`. No
git checkpoint substitutes for this: the bottle's live files are outside the git repo.

## Status

**Fixed and confirmed working (visually, by the user):**
- **Splash screen banner rendering.** Wine's `gdiplus` doesn't implement
  `InterpolationMode.HighQualityBicubic` (7) — confirmed by disassembling
  `/opt/cxoffice/lib/wine/i386-windows/gdiplus.dll`'s `resample_bitmap_pixel`: only `Bilinear`
  (3) and `NearestNeighbor` (5) are implemented; everything else (0, 1, 2, 4, 6, 7) hits an
  "Unimplemented interpolation %i" stub. Two wrong guesses before landing on 3 — `HighQualityBilinear`
  (6) and `High` (2, which internally aliases to 7) are *also* unimplemented; don't reuse either
  without new evidence. Full history: `reports/gdiplus-interpolation-findings.md`. **Correction:**
  this fix was originally marked "confirmed working" on the strength of the disassembly evidence
  alone — the banner image itself was never actually visually re-checked by the user under
  Windows 7 at the time. It was properly visually confirmed only later, incidentally, while
  testing the splash-tip-icon fix below (same screen, both bugs visible together in
  `supporting/splash-broken.png` before either fix, both gone after).
- **Splash screen tip label showing tofu boxes before the tip text.** Not a Wine font-substitution
  gap in the way originally guessed, and not the same class of bug as the license icon below:
  `FormSplashScreen.labelTip`'s baseline text is a genuine emoji (U+1F4A1, light bulb) that Wine
  has no glyph for. A proper fix was attempted first — vendoring real, user-licensed Segoe UI/Segoe
  UI Emoji fonts and configuring Wine's `FontLink\SystemLink` registry fallback (see `fonts/` and
  the release script's `--install-fonts` step) — and a real `wine regedit` multi-string import bug
  was found and worked around along the way, but the glyph still didn't render even with correct
  registry configuration confirmed via trace: the gap is deeper, in Wine's glyph-shaping code
  itself, not something registry configuration alone fixes. Fixed instead with the same narrow,
  safe resource-string patch used for the license icon (`--patch-splash-tip-icon`, Stage 7).
  Full history: `reports/splash-tip-icon-findings.md`.
- **Settings dialog left category panel (was completely blank).** Root cause: `formSettings`'s
  `Load` event never fires under Wine at all (not a paint bug, not a data bug — the event
  handler's first instruction never executes). Fixed by calling
  `formSettings_Load(this, EventArgs.Empty)` directly from the public
  `formSettings(string tabName)` constructor, after its `SwitchToTabPanel(...)` call. Two wrong
  intermediate fixes and why they failed (a Wine clip-region bug that was real but not the
  cause; calling the same fix from the wrong constructor, which crashed on unset
  `CurrentPanel`) are documented so they aren't re-tried: `reports/settings-panel-clip-region-findings.md`.
- **Settings category click crash.** `NotImplementedException` from
  `IApplicationAssociationRegistration.QueryAppIsDefaultAll` — a Windows Shell "is this the
  default mail client" COM API Wine doesn't implement, thrown unhandled instead of failing
  gracefully. Fixed by adding a sibling `catch (NotImplementedException) { return false; }`
  handler next to `Integration.IsDefaultClientVista`'s existing `catch (COMException) { return
  false; }` — the app already anticipated this class of COM failure, Wine just throws a
  differently-typed exception for it. User confirmed: moved between all categories, changed and
  saved settings, no further crash. Full history (including two exception-handler IL-correctness
  bugs hit and fixed along the way): `reports/default-mail-client-notimplemented-findings.md`.
- **License Activation failure** (spinner, then silent revert, no visible error). Root cause: an
  RSA-OAEP private-key decrypt (`DecryptAndVerify.DecryptAndVerifyString`/`...StringV1` in
  `MailClient.dll`, behind the License dialog's Activate button) fails inside Wine's `bcrypt.dll`
  — `gnutls_x509_privkey_set_spki` hits an internal GnuTLS assertion specific to OAEP's parameter
  encoding, throws, and is caught by the app's own blanket handler in `ReporterMode.Silent` (why
  nothing visible happens). Fixed by redirecting both call sites to a new pure-managed
  BouncyCastle-backed RSA-OAEP implementation (`MailClient.Licensing.BouncyCastlePatch.dll`,
  built against the app's already-vendored `BouncyCastle.Cryptography.dll`) instead of the native
  CNG-backed `RSA.Decrypt` — BouncyCastle has no P/Invoke, so it never touches the broken Wine
  code path. Cryptographic correctness verified independently before deploying (32/32 checks
  across 4 key sizes × 4 plaintext shapes, byte-identical to native .NET RSA-OAEP decrypt). User
  confirmed: Activation completed successfully. Full history, including the standalone
  verification harness and the trace evidence: `reports/license-activation-oaep-findings.md`.
- **License dialog "Get a license" button showing two tofu boxes.** Root cause: **not a Wine
  bug** — the button's own `Text` resource (`MailClient.UI.Forms.formLicense.resources`'s
  `buttonGetLicense.Text`, embedded in `MailClient.dll` itself) contains two literal Unicode C1
  control characters (U+0083) baked into the app's own resource data, confirmed isolated (occurs
  exactly once in the whole assembly) and confirmed not font-substitution-related (the button's
  paint code does plain `TextRenderer.DrawText`, no icon-glyph logic anywhere). Almost certainly
  an upstream eM Client resource-authoring/encoding bug that would show the same way on real
  Windows. Fixed anyway — cheap and safe regardless of platform — via a raw byte-level resource
  edit (`--patch-license-icon`, `~/tools/il-patcher`): U+0083 U+0083 → U+00A0 U+00A0
  (non-breaking space), chosen to keep the resource's byte length exactly unchanged so the
  `.resources` container's offset table needs no adjustment. User confirmed: "That's fixed the
  icon issue on button... There's no icons visible... just text... but that's nice and clean."
  Full history: `reports/license-icon-findings.md`.
- **Attachments (office documents, images, archives, audio/video) failing to open** with "There
  is no Windows program configured to open this type of file." Root cause: **not an eM Client
  bug** — its own attachment-open code (`UIUtils.OpenFileInDefaultApp` →
  `ShellInterop.OpenItem`) faithfully mirrors the same `IContextMenu`-based mechanism Explorer
  itself uses; a fresh CrossOver bottle just doesn't ship file-type associations for these
  extensions (`.pdf` is associated out of the box, most others aren't). Confirmed the error text
  itself is a Wine `shell32.dll` built-in string, not something eM Client generates. Fixed by
  importing `.reg` files (`file-associations/**`) that associate each extension with its own
  `EMClientWinePort.<ext>` ProgID pointed at the same `winebrowser.exe` handler `.pdf` already
  used — `deploy.sh` imports only extensions missing an association on the target bottle, so a
  bottle with a real Office/LibreOffice install isn't disturbed. Initially misreported as
  PDF-specific; PDFs were already working — the real, confirmed failure was `.docx`, and a
  systematic registry check turned up a much wider set of broken attachment types. User confirmed
  after live-testing multiple attachment types: "It worked." Full history:
  `reports/office-file-associations-findings.md`.
- **New-mail notification click firing its handler twice per single click.** Found while
  instrumenting the still-open empty-box notification bug below (not the same bug — see that
  bullet's own note on scope). Root cause: **not a Wine bug** —
  `FormGenericNotification.OnShown()` runs `layeredWindow.Click += layeredWindow_Click;`
  unconditionally every time it runs, with no matching `-=` anywhere in the class, and `OnShown()`
  runs at least twice per notification shown (confirmed via instrumentation: once early while the
  form is still blank, once again once Title/Content are populated) — each adding another
  subscription of the same handler to the same event, so a single click invoked
  `layeredWindow_Click`/`notificationForm_Click`/`PerformAction`/`ShowMailForm` twice. This also
  explained why real clicks never fire `FormGenericNotification.OnMouseClick` at all — clicks land
  on the `layeredWindow` drop-shadow companion window, whose own `layeredWindow_Click` handler
  calls `performMouseClick()` directly. Fixed via a new `--patch-notification-click-resubscribe`
  mode (Stage 8, not yet folded into `releases/<version>/deploy.sh` — apply manually via
  `~/tools/il-patcher` on top of Stage 7's output for now) inserting the missing
  `layeredWindow.Click -= layeredWindow_Click;` immediately before the existing `+=`, the standard
  unsubscribe-then-subscribe idiom (a no-op on the very first call, since removing a
  never-added delegate is a documented .NET no-op). User confirmed on a live bottle (with
  diagnostic logging still layered on top for direct verification): one notification, one click →
  `layeredWindow_Click`/`notificationForm_Click`/`Hide()` each fired exactly once (previously:
  twice), fade proceeded cleanly, no hang or crash. Full history:
  `reports/notification-empty-until-fade-findings.md`'s "Sixth/Seventh round" sections.
- **New-mail notification toast showing an empty box until it started to fade out.** Root cause:
  `this` form's title/content text is drawn live by `OnPaintTitle()`/`OnPaintContent()` from its
  own `OnPaint`/`OnPaintBackground`, directly onto `this`'s own device context — and `this`'s own
  client-area painting only succeeds while real Wine message-loop ticks are actively running (the
  same class of Wine gap chased throughout this investigation), which is why content only ever
  flashed into view during the brief Appearing/Disappearing tick bursts. The `layeredWindow`
  drop-shadow companion window, by contrast, receives its `backgroundBitmap` via a genuine, always-
  forced native `UpdateLayeredWindow` blit — reliable from the very first frame — but that bitmap
  never contained the title/content text at all, only the background/border/avatar. Fixed with a
  four-patch chain (`--patch-notification-text-in-bitmap`, `--patch-notification-refresh-on-
  content-change`, `--patch-notification-periodic-reblit`, `--patch-notification-suppress-self-
  text-only`): draws title/content directly into `backgroundBitmap` itself (reusing the real
  `OnPaintTitle`/`OnPaintContent` methods against a `Graphics` built on the bitmap, so any subclass
  override and the existing ellipsis/bounds/image-offset logic all still apply unchanged), forces
  an immediate rebuild-and-blit through the already-reliable `layeredWindow` path the moment
  `ShowNotification` sets new content (instead of relying on `this`'s own unreliable paint cycle to
  ever pick it up), periodically re-asserts that same blit every 300ms while visible, and removes
  the now-redundant (and ghost-prone — a faint, ~9px-offset duplicate could flash behind the real
  text right at the fade transition) `OnPaintTitle`/`OnPaintContent` calls from `this`'s own
  `OnPaintBackground`. User confirmed live ("I saw text the whole time... YAY") and against a
  genuine incoming real email, with a video-frame-extraction re-confirmation later in the same
  investigation. **This fix was lost for a time** — a later dev-testing session rebuilt its test
  chain starting from the plain Stage-1-7 output and never re-applied these four patches, so the
  bug silently reappeared in every build after that point until the user caught it live and this
  was corrected; see `reports/notification-empty-until-fade-findings.md`'s twenty-first round for
  the regression's own story. **Not yet folded into `releases/<version>/deploy.sh`** (same
  standing gap as Stage 8 above) — apply the four flags manually via `~/tools/il-patcher` on top of
  Stage 7's output (or Stage 8's, if also applying the click-dispatch fix, which is unrelated but
  commonly wanted together) until it is. Full history, including every dead end ruled out before
  landing on this fix: `reports/notification-empty-until-fade-findings.md`'s "Sixteenth/Seventeenth
  round" sections.

**Confirmed as a real, separate Wine bug, but not the cause of anything fixed above — patched
anyway since it's a real bug and the fix is cheap:**
- `ControlDataGrid`'s `WM_PAINT` could hit a Wine `BeginPaint`/`WM_NCPAINT` region-object-reuse
  bug that collapses the client-area clip to `(0,0)-(0,0)`, silently discarding every draw call.
  Worked around by adding `ControlStyles.AllPaintingInWmPaint` to `ControlDataGrid.initialize()`
  (missing from the original code; WinForms recommends it for owner-drawn double-buffered
  controls, and adding it changes the paint message sequence enough to avoid the bug). Confirmed
  via trace: clip region and content-blit both correct after this patch. Detail in
  `reports/settings-panel-clip-region-findings.md`.

**Not started (scanned, not confirmed, not patched):**
- Stage 2 of the interpolation fix — 7 more `HighQualityBicubic`(7) call sites, same pattern,
  not yet trace-confirmed as user-visible bugs: `PictureBoxEx.OnPaint`,
  `ControlDataGrid.DrawNoItems`, `formAbout.OnPaintBackground`,
  `formDataAccounts.panel_Details_Paint`, `ControlItemCardsWithImage.DrawItemForeground`,
  `DesktopAvatarManager.TryResizeAndSaveBitmap`, `ImageHandler.TryTranscodeAvatarAsPngOfOptimalSize`.
  If pursued, fix value is **3 (Bilinear)**, confirmed by disassembly — not 6 or 2.
- Stage 3 — 1 site in vendored `QRCoder.dll` (`QRCode.GetGraphic`), same pattern, optional
  (only affects Settings → QR export).
- Hold rows — `InterpolationMode.High` (2), 2 sites (`UIAvatar.GetImageSingleRes`,
  `QRCoder.ArtQRCode.Resize`). Originally "no evidence either way"; now **confirmed broken** by
  the same disassembly (High aliases to HighQualityBicubic internally). Promote to a stage if
  pursued — don't leave as "Hold, no evidence", that reasoning is now stale.

**Not started (scanned, not confirmed, not patched):** Stage 2/3/Hold interpolation items above
remain scanned-but-not-pursued if a future session wants to extend that work. No other bugs are
currently in an "investigated, unresolved" state — the empty-box-until-fade bug that used to be
the one open item here is now fixed and confirmed (see its bullet above); everything found and
fixed so far, including that one, is listed in the "Fixed and confirmed" section. The many dead
ends ruled out on the way to that fix (Invalidate() calls, forced SetLayeredWindowAttributes
changes, opacity dip-and-recover sequences, the Cinnamon "Map" animation and X11 Sync-extension
compositor hypotheses, a standalone repro app that never reproduced it, and more) remain fully
documented in `reports/notification-empty-until-fade-findings.md` so none of them get re-tried —
read that file's early rounds before starting any new notification-rendering investigation, even
though the bug itself is now closed.

## Investigation method (what actually worked this round)

Roughly in order of how cheap/reliable each was, cheapest first:

1. **Read the app's own crash bug reports.** When eM Client's error dialog appears, it writes a
   full XML stack trace to `C:\users\crossover\AppData\Local\Temp\bug.<timestamp>.txt` (=
   `~/.cxoffice/emClient_win_7_x64/drive_c/users/crossover/AppData/Local/Temp/bug.*.txt` on
   Linux) even when it fails to *send* the report (which it will, under Wine — the send itself
   throws on invalid-XML-character issues, a red herring, ignore it and read the file directly).
   This is the single best signal available: a real, symbolicated .NET stack trace, no tracing
   or instrumentation needed. Check for new files here after any crash.
2. **Disassemble the actual Wine binary** when a hypothesis is about what Wine does or doesn't
   implement, rather than inferring it from trace behavior. `objdump -d` on
   `/opt/cxoffice/lib/wine/i386-windows/gdiplus.dll` (or the relevant Wine DLL) settled the
   InterpolationMode question in one pass after two rounds of wrong trace-based guesses. Find the
   fixme string's file offset (`python3 -c "... data.find(b'...')"`), map it to an RVA via
   section headers (`objdump -h`), then find what code references that RVA
   (`objdump -d | grep <rva-hex>`) and read the dispatch logic around it.
3. **Instrument the app directly** (`~/tools/il-patcher --patch-diag`, or a one-off variant of
   it) when the question is about the app's *own* runtime state (field values, which code path
   ran, whether a method was even entered) rather than about Wine. Insert
   `File.AppendAllText(@"Z:\tmp\claude-diag.log", "...")` calls at the point in question via
   Cecil, deploy, have the user reproduce once, read the log directly — no `CX_DEBUGMSG` needed,
   far more precise than inferring app state from GDI trace noise. This is what actually found
   both the clip-region bug's real extent and the Settings-panel Load-event bug; trace-only
   investigation had produced two wrong fixes first, in both cases.
4. **`CX_DEBUGMSG` trace** (`+gdiplus,+region,+clipping,+bitblt,+message,+font,+seh` was the
   working channel set this round — see below) when the question is genuinely about Wine's own
   behavior (window messages, GDI region math, paint dispatch) and disassembly isn't practical.
   Channel names for a given Wine build aren't guessable — confirm via
   `strings /opt/cxoffice/lib/wine/i386-unix/<module>.so | grep -E '^[a-z]+$'` against a
   plausible short list; `CX_DEBUGMSG=help` does not enumerate them on this CrossOver build.
   Traces are large (100MB+ for even a short session) and need correlating by HWND/timestamp —
   slower and noisier than options 1–3, but sometimes it's the only option (e.g. confirming the
   clip-region fix actually changed Wine's behavior, not just app behavior).

For any option requiring the user to reproduce interactively: **always offer both** "you verify,
tell me what happened" and "I start a traced/instrumented launch first, you just reproduce" —
this was an explicit, repeated user preference this round. Coordinate launches: check
`ps aux | grep -i mailclient` before relaunching (never kill a running instance without asking —
it may hold live user data), start the traced/instrumented build, tell the user what to do, then
poll for the process to exit or the log file to gain content before reading results.

## IL-patching lessons (apply to any future Cecil-based patch in this project)

`~/tools/il-patcher` started as a read-only scanner (`--patch`, simple operand rewrite: flips an
`ldc.i4` constant, verified by pattern-matching the few instructions around it — low risk). It
grew instruction-*insertion* modes (`--patch-allpaintinginwmpaint`, `--patch-settings-refresh`,
`--patch-default-client-notimpl`, and the temporary `--patch-diag`) for fixes that need new code,
not just a changed constant, plus one raw-resource-data mode (`--patch-license-icon`, which
touches an embedded `.resources` blob's bytes directly rather than IL — see its own doc comment
in the source for why a length-preserving byte replacement was used instead of the more obvious
"just delete the bad characters"). Several real bugs were introduced and caught during this
session, all worth guarding against explicitly next time:

1. **Reusing a re-read anchor reverses insertion order.** `InsertBefore(anchor, x)` called three
   times with `anchor` re-read as `il[0]`/similar each time (instead of captured once into a
   local) picks up the *previous* insertion as the new anchor, so the three new instructions end
   up in reverse order — e.g. a `call` landing before its arguments are pushed, a stack
   underflow. Always capture the anchor **once** into a local and reuse that local for every
   `InsertBefore` call in the sequence.
2. **Inserting before a branch target without retargeting skips the new code.** If the chosen
   anchor instruction is itself the target of an earlier `br`/`brtrue`/`brfalse` elsewhere in the
   method (very common — it's often a natural "join point" after an `if` block, which is
   *why* it looked like a good insertion point), inserting new code immediately before it does
   **not** make branches "fall through" the new code — a branch jumps to the exact instruction
   object, bypassing anything spliced in before it. The fix landed inside a conditional's
   fall-through path instead of the intended unconditional join point twice this session before
   this was caught. Always search the whole method body (and its exception handlers) for any
   instruction/handler-region boundary whose `Operand`/`TryStart`/`TryEnd`/`HandlerStart`/
   `HandlerEnd` equals the original anchor, and retarget them to the new first inserted
   instruction.
3. **Inserting new code before an existing exception handler's own `HandlerEnd` silently grows
   that handler's range to swallow the new code.** `HandlerEnd` (also `TryEnd`) is an *exclusive
   boundary marker* — a reference to the instruction right after the region, not a fixed offset.
   Adding a sibling `catch` clause by inserting its body immediately before an existing handler's
   `HandlerEnd` extends that *existing* handler's own range to include the new code too, unless
   `HandlerEnd` is explicitly reassigned to the new code's first instruction. The result:
   two handlers with overlapping byte ranges — invalid IL that neither Cecil's writer nor
   ilspycmd's decompiler flags, but the CLR verifier does, as `InvalidProgramException` at JIT
   time (hit for real, deployed, crashed — see `--patch-default-client-notimpl`'s history).
   Separately, when a `try` has multiple sibling `catch` clauses, a *new* one must be inserted at
   that position in `body.ExceptionHandlers` (e.g. right after the existing handler it's modeled
   on) — appending to the end of the list can land it after an enclosing method's outer
   `finally`, violating the CLR's required most-nested-first handler ordering. Use
   `--dump-handlers <dll> <type> <method>` (prints the handler table with resolved offsets and
   flags nesting-order violations) to verify both **before** deploying, any time a patch adds,
   moves, or extends an exception-handler region — this class of bug doesn't show up in a
   decompile.
4. **Inserting enough new instructions can silently corrupt a nearby short-form branch.**
   `bne.un.s`, `br.s`, `brtrue.s`, `brfalse.s`, etc. encode their target as a single **signed
   byte** relative offset (±127 bytes). Mono.Cecil does **not** automatically widen these to their
   long-form equivalents (`bne.un`, `br`, ...) as a method body grows from inserted instructions —
   if enough new code lands between a short branch and its target that the true offset no longer
   fits in an sbyte, `module.Write()` still emits *something*, silently wrong, rather than
   erroring. Decompiles as garbled control flow with spurious "stack underflow" errors, often in a
   completely unrelated branch of the same method (hit for real: a 48-instruction insertion in
   `--patch-notification-invalidate`'s third revision broke a nearby `bne.un.s`; two earlier,
   much smaller insertions in the same method had stayed under the range by luck, which is why
   this wasn't caught sooner). Fix: call `body.SimplifyMacros()` (from `Mono.Cecil.Rocks` — needs
   `using Mono.Cecil.Rocks;`; the assembly ships inside the same `Mono.Cecil` NuGet package, no
   extra package reference needed) once, before making any insertions, on any method whose body
   might grow by more than a trivial amount. It converts every short-form branch in the method to
   long form up front, which has no functional downside and removes the range problem entirely.
   Cheap enough to just always call before inserting, rather than judging case-by-case whether a
   given insertion is "small enough".
5. **`typeof(X)` reflection to build a `MethodReference`/`FieldReference` bakes in the *patching
   tool's own* runtime's assembly version, not the target app's.** Several instrumentation helpers
   resolve BCL members via reflection on the running `il-patcher` process itself (e.g.
   `typeof(Environment).GetProperty("TickCount")`, `typeof(File).GetMethod("AppendAllText", ...)`)
   and this is safe for types in `System.Private.CoreLib`/its forwarding facades (`System.Runtime`,
   `System.Collections`, ...) because the CLR's default framework resolution unifies/forwards these
   across versions transparently. It is **not** safe for a type in an app-local, deps.json-pinned
   assembly the target app ships its own copy of alongside itself — `il-patcher` targets `net10.0`,
   so `typeof(System.Drawing.Rectangle).GetProperty("IsEmpty")` bakes in a
   `System.Drawing.Primitives, Version=10.0.0.0` reference, but MailClient.exe ships and runs on
   .NET 8 with its own `System.Drawing.Primitives.dll` at `Version=8.0.3026.36720` — no v10 copy
   exists alongside it, and unlike CoreLib-forwarded types this doesn't get silently redirected.
   Hit for real: a `--patch-diag` revision instrumenting `headerRect.IsEmpty` crashed the app with
   `FileNotFoundException: Could not load file or assembly 'System.Drawing.Primitives,
   Version=10.0.0.0...'` the moment the instrumented method ran — decompiled clean (ilspycmd
   doesn't validate assembly-reference versions against what's actually deployed) and only
   surfaced as a real runtime crash, exactly the "decompiling clean is not sufficient proof" shape
   as lesson 3's exception-handler bug. Fix: never reflect on the patching tool's own process for a
   type outside CoreLib/its facades — resolve it from a field/parameter/return type *already*
   present in the module being patched instead (e.g. `someExistingField.FieldType.Resolve()`, then
   find the member on that already-correctly-versioned `TypeDefinition`), the same technique
   already used elsewhere in this file for `System.Windows.Forms.Control` (walk the base-type
   chain rather than reflect on `typeof(Control)`).

**Always re-decompile and read the result before deploying** anything beyond a simple operand
rewrite — `ilspycmd -m "<doc-id>" <dll>` or `ilspycmd -t "<type>" <dll>` on the patched output.
This caught bugs 1, 2, and 4 above before they ever reached the bottle: bug 1 and bug 4 as an
explicit decompiler error ("Stack underflow"), bug 2 as the new code visibly appearing inside the
wrong `if` block in the decompiled C#. A clean scan-mode pass (`$ILP <dir>`, no flags) confirms
operand-rewrite patches but does *not* catch any of these — it doesn't attempt to
decompile control flow or validate exception regions. Bug 3 is the sharpest lesson here: it
decompiled perfectly cleanly (ilspycmd doesn't validate handler-region bounds) and only surfaced
as a real crash after deploying — `--dump-handlers` exists specifically because decompiling
clean is not sufficient proof for any patch touching exception regions.
