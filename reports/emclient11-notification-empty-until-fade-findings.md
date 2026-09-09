# eM Client 11 (beta) notification toast: same empty-until-fade bug, same 7-stage fix

**Status: fixed and deployed as release/11.0.196-3.** Verified via decompile + `--dump-handlers`
at every stage and a full fresh-from-pristine chain re-run before deploying, then deployed live
to `emClient_11_beta_win_11` and confirmed working visually by the user.

Separate product line from the rest of this repo's fixes — see CLAUDE.md's "eM Client 11
(beta) — separate release line" section. This is the third fix for that line (after
`release/11.0.196-1`, the PBKDF2 startup crash, and `release/11.0.196-2`, the splash tofu boxes).

## Symptom

Same as the already-fixed eM Client 10.4.5674 issue
(`reports/notification-empty-until-fade-findings.md`): new-mail notification toasts show their
chrome (header strip + body background) but no title/content text, and no close/settings/reply/
flag/delete/previous/next icons, until the toast starts to fade out.

## Root cause — confirmed identical before porting anything, not assumed from the matching symptom

Decompiled and diffed `FormGenericNotification`, `FormMailNotification`, `LayeredBaseForm`,
`LayeredForm`, `BaseThemeForm`, and `ControlToolStripButton` between the two versions'
`original/em-10.4.5674/` and `original/em-11.0.196/` snapshots before writing or reusing a single
patch. Findings:

- `FormGenericNotification.OnPaint`/`OnPaintBackground`/`OnPaintTitle`/`OnPaintContent` are
  **byte-for-byte identical** between versions (down to the same `-9, -9` magic-number shadow
  offset). Title/content text is still drawn live via `TextRendererEx.DrawText` directly on
  `e.Graphics` during `this` form's own paint cycle — never baked into `backgroundBitmap`. Same
  root cause, unchanged.
- `updateBackgroundBitmap()`'s header/gradient/border-drawing logic is unchanged; the only real
  difference is the drop-shadow is now built via a single `CommonPaintUtils.CreateShadowBitmap()`
  call instead of v10's manual tiled-PNG drawing — doesn't touch the bug.
- The double-click-fire bug's own cause (`layeredWindow.Click += layeredWindow_Click;`
  subscribed unconditionally on every `Show()`, no unsubscribe) is present unchanged.
- `ControlToolStripButton.OnPaint`/`OnMouseEnter`/`OnMouseLeave` keep the same `protected
  override` signatures, so the v10 fix's additive `RaisePaint`/`RaiseMouseEnter`/
  `RaiseMouseLeave` wrapper technique (declaring new public members on the SAME class that call
  the protected ones — legal, zero blast radius, see CLAUDE.md's IL-patching lesson 10) still
  applies unmodified.

Two real structural differences were found (both required actual code changes, not just
re-running the existing flags):

1. **`LayeredForm` moved assemblies.** In 10.4.5674 it's declared in `MailClient.dll`'s own
   `MailClient.UI.Forms` namespace; in 11.0.196-beta it moved into the shared
   `MailClient.Common.UI.dll`, under `MailClient.Common.UI.Forms` — confirmed via decompile diff
   that the class body itself is untouched, only its assembly/namespace changed. This meant the
   hover-forward fix (below) needed to patch a method living in a *different* module than the one
   declaring `FormGenericNotification`, something none of the existing patches had needed before.
2. **`FormMailNotification`'s toolbar buttons were redesigned.** 10.4.5674 has 3 fixed, separately
   named action buttons (`button_Reply`, `button_Flag`, `button_Delete`), each with its own click
   handler. 11.0.196-beta replaced these with a configurable "quick actions" feature: 5 generic
   `button_action0`..`button_action4` fields, all sharing one click handler (`MailAction`) that
   reads the clicked button's own `.Tag` to know which action fired. `button_Previous`/
   `button_Next` (navigation, unrelated to the actions feature) are unchanged in both versions.

## Fix

All 7 stages of the original chain (v10's Stages 8–14) apply, in the same order:

1. `--patch-notification-click-resubscribe` — unmodified, applied as-is.
2. `--patch-notification-content-padding` / `avatar-title-gap` / `title-singleline` /
   `title-vcenter-fix` — unmodified, applied as-is (layout fixes; note the pixel offsets are
   empirically-measured corrections from the v10 investigation — still structurally correct here,
   but their exact values are worth a visual sanity check once this is actually deployed, since
   they were tuned against v10's rendering specifically).
3. `--patch-notification-text-in-bitmap` / `refresh-on-content-change` / `periodic-reblit` /
   `suppress-self-text-only` — unmodified, applied as-is. This is the core fix.
4. `--patch-notification-icon-bitmap` / `title-icon-clip` — unmodified, applied as-is.
5. `--patch-notification-text-drawstring` — unmodified, applied as-is.
6. `--patch-notification-hover-forward` — **needed a real fix**, see below.
7. `--patch-notification-toolbar-icons` — **needed real adaptation**, see below.

### `--patch-notification-hover-forward`: made version-agnostic, and two real tool bugs fixed

Rewrote the LayeredForm lookup to resolve `MailClient.UI.Forms.NotificationForms
.FormGenericNotification`'s own `layeredWindow` field's declared type (`layeredWindowField
.FieldType.Resolve()`) instead of a hardcoded `"MailClient.UI.Forms.LayeredForm"` string —
Cecil's resolver follows the cross-assembly reference transparently (same
`AssemblyResolver`/search-directory already in use), so this now works unmodified against either
version's assembly layout, and the resolved `TypeDefinition`'s own `.Module` says which physical
file to write the patch back into.

Making this actually work correctly surfaced two genuine, previously-latent bugs in `il-patcher`
itself — neither had ever mattered before because every prior patch touched exactly one assembly:

1. **Cross-module `MethodReference` import.** IL instructions inserted into `LayeredForm.WndProc`
   (which now lives in `MailClient.Common.UI.dll`) used `MethodReference`s imported via
   `module.ImportReference(...)` where `module` was the *other* module (`MailClient.dll`) — Cecil
   accepted this silently at patch-build time but threw `ArgumentException: ... is declared in
   another module and needs to be imported` at `module.Write()`. Fixed by importing
   `mouseEventArgsCtorRef`/`controlOnMouseMoveRef` into `layeredFormTypeDef.Module` specifically
   (a *second*, separately-imported reference where the same member is also called from
   module-side code, since each module needs its own import-table entry regardless of whether the
   underlying member is the same).
2. **File-write-ordering bug in the patch's own output pass.** The original loop-based
   "`File.Copy` everything that isn't `targetAssembly`, patch-and-write whichever file *is`"
   structure silently assumed `Directory.GetFiles` enumerates in a stable, alphabetical order —
   it doesn't (confirmed on this filesystem: not alphabetical). When `MailClient.dll` happened to
   be enumerated *before* `MailClient.Common.UI.dll`, the patched `LayeredForm` write landed
   first and a later, unconditional plain `File.Copy` of `MailClient.Common.UI.dll` (from the
   loop reaching that filename's own "not targetAssembly" branch) silently clobbered it back to
   the unpatched original — no error, no warning, just a byte-identical-to-input output file.
   Caught by literally diffing the patched output against the pristine input by hand after the
   tool reported success (worth doing by default any time a patch's own success message is the
   only signal checked). Fixed by restructuring: do all the patch work up front (outside any file
   loop), THEN do a single copy-everything-else pass that explicitly skips both special
   filenames, THEN write both patched modules — removing the ordering dependency entirely rather
   than trying to control iteration order.

Both fixes verified via a full v10 regression run (byte-identical decompiled output, confirming
no change in behavior for the already-shipped release) plus the new v11 path (confirmed the
`MailClient.Common.UI.dll` output's checksum actually differs from its input, and decompiles to
the exact intended `WndProc`/`OnShown` shape).

### `--patch-notification-toolbar-icons`: generalized to auto-detect either button set

Replaced the hardcoded 5-name `button_Reply`/`button_Flag`/`button_Delete`/`button_Previous`/
`button_Next` field/handler-name arrays with a small list of *candidate* button sets (v10's
5-button shape, v11's 7-button shape — 5 action buttons plus the 2 unchanged navigation buttons),
auto-detected by checking which set's field names actually exist on the target's
`FormMailNotification` type. Every loop and array previously hardcoded to `5` now derives its
count from the detected set's length instead, so a future button-count change wouldn't even need
new code, just a new entry in the candidate-set list.

This surfaced one real, necessary correctness fix along the way, not just a mechanical
generalization: the click-dispatch code (`performMouseClick`'s override) previously always passed
`this` (the form) as the `sender` argument to each button's click handler — harmless for v10,
whose 3 per-button handlers never inspected `sender`, but **wrong** for v11's shared `MailAction`
handler, which reads `sender.Tag` to know which configured action was clicked; passing the form
instead of the actual button would have silently no-op'd every quick-action button click. Fixed
by pushing the clicked button field itself as `sender` instead — strictly more correct for both
versions (v10's handlers simply ignore the now-correct value), not a version-conditional branch.

### Verification

- Every one of the 7 stages verified individually: decompiled output matches the intended shape,
  `--dump-handlers` confirms no exception-handler-region corruption on every method touched.
- A full fresh run of all 7 stages, `original/em-11.0.196/` straight through to a final output
  directory, succeeded end to end with no errors — confirmed both touched assemblies
  (`MailClient.dll`, `MailClient.Common.UI.dll`) differ from their pristine inputs (non-trivial
  checksums) and the output directory has the same file count as the input (472 files both ways
  — nothing missing, nothing extra).
- The hover-forward fix specifically was regression-tested against `original/em-10.4.5674/` too
  (not just v11), confirming the version-agnostic rewrite didn't change v10's already-shipped
  behavior.
- Deployed live via `releases/11.0.196-beta/deploy.sh` (correctly resumed from revision 2 straight
  to Stage 3, then a second run correctly skipped the whole pipeline at revision 3) and confirmed
  working visually by the user — the notification toast now shows its title/content text and
  icons immediately, not just at fade-out.

### Update: confirmed still applies unmodified to eM Client 11.0.282

When eM Client's beta updated from `11.0.196` to `11.0.282`, re-ran the same "decompile-diff
both versions before touching anything" discipline this fix itself was built with, against a
fresh `11.0.282` install (`original/em-11.0.282/`). Both types this fix touches decompile
byte-for-byte identical between the two builds: `MailClient.UI.Forms.NotificationForms
.FormMailNotification` (`MailClient.dll`) and `MailClient.Common.UI.Controls.ControlToolStrip
.ControlToolStripButton` (`MailClient.Common.UI.dll`). `MailClient.Common.UI.Forms.LayeredForm`
(also touched by `--patch-notification-hover-forward`) differs by exactly one removed, unused
convenience overload (`UpdateWindow(Bitmap, byte)`) — unrelated to `WndProc`, the only method of
that type this fix actually patches, which is untouched.

Also re-ran the full 7-stage pipeline itself (not just a structural diff) against `11.0.282`:
every sub-patch applied with the same messages as the known-good `11.0.196` run, `--dump-handlers`
reported correct nesting on every exception-handler-touching stage, and a decompile spot-check on
`FormGenericNotification.OnShown` read clean. No adaptation needed — `releases/11.0.196-beta
/deploy.sh` now supports both builds from the same script (see CLAUDE.md's eM Client 11 section
and that script's own header comment for why this is a deliberate exception to the usual
fork-a-new-folder-per-version convention). Confirmed working live against a real `11.0.282`
install by the user, on a separate machine.

## Tooling notes worth keeping for future cross-assembly patches

- **Decompile-diff both versions' relevant types BEFORE writing or reusing any patch code.**
  This is what turned "I suspect this will need the same patch" into a confident, itemized plan
  (5 of 7 stages: no change needed at all; 2 of 7: specific, scoped adaptations) rather than
  discovering each mismatch one crash at a time.
- **A patch that touches more than one assembly needs its OWN write-ordering discipline** — don't
  rely on `Directory.GetFiles`' enumeration order for anything. Do all patch work first, copy
  everything untouched afterward while explicitly excluding every file that got patched, then
  write the patched files last.
- **Every `MethodReference`/`FieldReference` used inside a method body must be imported into THAT
  method's own declaring module** — not whichever module happened to be open when the reference
  was built. When the same underlying member is called from code in two different modules (as
  `mouseEventArgsCtorRef` was here), that's two separate `ImportReference` calls, one per module,
  even though it's conceptually "the same" reference.
