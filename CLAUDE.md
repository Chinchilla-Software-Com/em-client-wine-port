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

Bottle name: emClient_win_7_x64. Exe: MailClient.exe. Bottle path:
`~/.cxoffice/emClient_win_7_x64/`; `Z:\` inside the bottle maps to `/` on the Linux side, which
is how diagnostic instrumentation (see below) writes logs readable directly from outside Wine.

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
path).

```bash
ILP="dotnet ~/tools/il-patcher/bin/Release/net10.0/il-patcher.dll"

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
$ILP --patch-default-client-notimpl il-patches/output-stage3-final/ il-patches/output-final/

# Deploy: swap both touched DLLs into the live bottle (back up the bottle's current
# copies first if you haven't already — see "Rollback" below).
BOTTLE="/home/$USER/.cxoffice/emClient_win_7_x64/drive_c/Program Files (x86)/eM Client"
cp il-patches/output-final/MailClient.dll "$BOTTLE/MailClient.dll"
cp il-patches/output-final/MailClient.Common.UI.dll "$BOTTLE/MailClient.Common.UI.dll"
```

Verify before deploying, every time — this has caught real bugs (see "IL-patching lessons"
below):
```bash
# Interpolation sites should show exactly Bilinear at the two known offsets, 13 total unchanged:
$ILP il-patches/output-final/    # (no --patch flag = scan mode)

# Decompile-and-read anything you just patched with instruction insertion (not just an
# operand rewrite) before deploying — ilspycmd will surface invalid IL as a decompile error:
export DOTNET_ROOT=~/.dotnet   # ilspycmd needs this set in this environment
ilspycmd -m "M:MailClient.UI.Forms.formSettings.formSettings_Load(System.Object,System.EventArgs)" il-patches/output-final/MailClient.dll
ilspycmd -t "MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid" il-patches/output-final/MailClient.Common.UI.dll | grep AllPaintingInWmPaint

# Any patch touching exception handlers (Stage 4 and beyond) additionally needs the handler
# table itself checked — decompiling clean is not sufficient (see "IL-patching lessons"):
$ILP --dump-handlers il-patches/output-final/MailClient.dll MailClient.Utils.Integration IsDefaultClientVista
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
  without new evidence. Full history: `reports/gdiplus-interpolation-findings.md`.
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

No open items as of this update — the two bugs originally reported (blank Settings panel, and
the category-click crash found once the panel worked) are both fixed and confirmed. Stage 2/3/
Hold interpolation items above remain scanned-but-not-pursued if a future session wants to
extend that work.

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
not just a changed constant. Several real bugs were introduced and caught during this session,
all worth guarding against explicitly next time:

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

**Always re-decompile and read the result before deploying** anything beyond a simple operand
rewrite — `ilspycmd -m "<doc-id>" <dll>` or `ilspycmd -t "<type>" <dll>` on the patched output.
This caught bugs 1 and 2 above before they ever reached the bottle: bug 1 as an explicit
decompiler error ("Stack underflow"), bug 2 as the new code visibly appearing inside the wrong
`if` block in the decompiled C#. A clean scan-mode pass (`$ILP <dir>`, no flags) confirms
operand-rewrite patches but does *not* catch any of these three — it doesn't attempt to
decompile control flow or validate exception regions. Bug 3 is the sharpest lesson here: it
decompiled perfectly cleanly (ilspycmd doesn't validate handler-region bounds) and only surfaced
as a real crash after deploying — `--dump-handlers` exists specifically because decompiling
clean is not sufficient proof for any patch touching exception regions.
