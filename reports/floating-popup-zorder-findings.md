# Floating-popup Z-order under Wine: three related bugs, one patch stage

Three separate, user-reported bugs. The first two share the same underlying root-cause class
(Wine failing to recognize a real native window-owner relationship the app's own WinForms popup
code correctly asks for); the third is a completely separate, pre-existing app-level mechanism
that only became visible once the first two were fixed enough to test properly. All three are
fixed together as one patch stage since they always deploy as a unit.

- `--patch-nested-popup-owned-by-ancestor` (bug 1)
- `--patch-colordialog-suppress-autoclose` (bugs 2 and 3)

Both touch `MailClient.Common.UI.dll` only. Confirmed applicable to all three supported
eM Client builds (10.4.5674, 11.0.196-beta, 11.0.282) via decompile-diff before porting, not
assumed — see "Cross-version verification" below.

## Bug 1: nested combo dropdown renders underneath its ancestor popup

**Report** (screenshot): while composing an email, opening the font-family dropdown inside the
"A" (font/style) toolbar popup causes the font-name list to render UNDERNEATH the popup's own
Text Color / Background Color panel instead of on top of it.

**Reproduction**: a throwaway, reflection-based diagnostic tool (not tracked in this repo) was
built to drive the exact UI sequence without any UI-automation tooling (none exists in this
environment): a background thread polling marker files under `Z:\tmp\claude-trigger-*`,
dispatching via `Control.BeginInvoke` onto the UI thread, walking the object graph via
`System.Reflection` (`formMain` → `formSendMail` → `ControlHtmlEditor` → `stripButton_Font`
(`ControlToolStripButtonFontSelector`) → `controlFontSelect` (`ControlFontSelect`) →
`comboBoxFontSelector` (`ComboBoxFontSelector : ComboBoxWithDataGrid`)), injected into a live
bottle's `formMain` constructor via a one-off Cecil patch (`Triggers.Start(this)`). This
reproduced the bug on demand and let every fix attempt be screenshotted and compared directly.

### Four fix attempts, in order, three of them wrong

1. **Hypothesis: Wine's own request-coalescing drops a raise-to-top request.** A
   `CX_DEBUGMSG="+win,+winpos,+event,+message,+nonclient,+x11drv"` trace showed the nested
   popup's window requesting to be raised to the top of the stack at creation
   (`window_set_config ... mask 0x40 above 1`), immediately followed by four consecutive
   `"is updating _NET_WM_STATE/_MOTIF_WM_HINTS, delaying request"` trace lines, then a final
   flushed config that was a pure resize with no stack-mode bit at all. Patched
   `ComboBoxControl.ShowDropDownNormal()` to re-issue
   `SetWindowPos(popupHelper.Handle, HWND_TOP, ...)` immediately after `.Show()`. **Result: zero
   visible effect, live.**
2. **Assumed attempt 1's own call landed inside the same coalescing window.** Deferred the same
   `SetWindowPos` call onto a one-shot 100ms `System.Windows.Forms.Timer` (more than double the
   observed ~47ms settle time). **Result: still zero visible effect**, and `xwininfo -root
   -tree`'s raw sibling order was IDENTICAL before and after the call — proving the call wasn't
   being coalesced, it was being ignored outright. Confirmed independently: even `wmctrl -i -r
   <id> -b add,above` (a strong EWMH "always on top" request, bypassing the app and Wine
   entirely) had no effect either, while the same tool correctly raised an unrelated, ordinary
   window in a sanity check — ruling out "coalescing" and "the screenshot tool is stale" as
   explanations for both failures.
3. **`xwininfo -id <hwnd>` (checked directly for the first time only after both restacking
   attempts failed) showed the real asymmetry**: the outer "A" popup is
   `Override Redirect: yes`, the nested font-list popup is `Override Redirect: no`. An
   override-redirect window is placed directly by the X server, completely bypassing the window
   manager — which is why it always wins against a managed sibling regardless of any stacking
   request, and why attempts 1 and 2 could never have worked no matter how they were timed.
   Hypothesized this was driven by `WS_EX_TOPMOST` and patched `PopupWindowHelper.CreateParams`
   (no prior override existed) to unconditionally OR it into `ExStyle`. **Result: still not
   override-redirect, still rendering underneath.** A follow-up `CX_DEBUGMSG="+win,+x11drv"`
   trace comparing the two windows' actual `NtUserCreateWindowEx` calls proved why: both are
   created with byte-identical `ex_style` (`0x10008` — `WS_EX_TOPMOST` already included by
   default, before any patch), identical `style`, identical (absent) parent and owner. Whatever
   decides override-redirect for this Wine build is invisible to, and unreachable from, anything
   in the `CreateWindowEx`/`SetWindowPos` surface the app can influence — ruling out any
   `CreateParams`-level fix entirely, not just the specific bit guessed.
4. **Sidestepped the stacking question by hiding the ancestor.** Since override-redirect
   couldn't be reached or controlled, tried `WinApi.Win32.ShowWindow(ancestor.Handle, SW_HIDE)`
   while the nested popup was open, restoring it (`SW_SHOWNOACTIVATE`) on close. This worked and
   was verified live (both the hide and the restore path) — but was **rejected by the user**: the
   requirement is both popups genuinely on-screen together with correct stacking, not one hidden.

### Root cause and real fix

A `CX_DEBUGMSG="+win"` trace (captured during attempt 3) shows both popups' `GWLP_HWNDPARENT`
(index `-8`) being set to a transient value (some internal WinForms/`ToolStripDropDown`
bookkeeping window) and then immediately cleared back to `0` by WinForms' own code, for EITHER
popup, before either is ever shown. Win32 has a core, window-manager-independent guarantee that
an OWNED window always draws above its owner in the z-order — and neither popup ends up with a
real native owner at all.

`PopupWindowHelper.SetVisibleCore` already computes `findParentPopupHelper()` (an existing
private method walking the source control's `Parent` chain for an ancestor `PopupWindowHelper` —
non-null exactly for this nested-popup case) for its own `OwnerItem` bookkeeping, but that only
affects WinForms' internal `ToolStripDropDown` cascade-closing logic, not the native HWND owner
chain — it evidently never propagates into a real
`SetWindowLongPtr(..., GWLP_HWNDPARENT, ...)` call for a popup owned by ANOTHER floating
`ToolStripDropDown` (as opposed to the common case of a real Form-hosted `ToolStrip`).

**Fix**: when a popup opens and `findParentPopupHelper()` finds an ancestor, explicitly call
`WinApi.Win32.SetWindowLong(this.Handle, GWL.HWNDPARENT, ancestor.Handle)` (both methods already
exist as public wrappers in the app's own `WinApi.dll`) to establish that native owner
relationship, THEN re-issue `SetWindowPos(HWND_TOP, ...)` so Wine's z-order enforcement
reconciles against the now-current owner rather than leftover pre-ownership state (the
`SetWindowLong` call alone was tried first and had no visible effect by itself — the companion
`SetWindowPos` immediately after is what actually makes Wine act on the new ownership).

Verified live end-to-end: both popups simultaneously mapped and visible (`xwininfo`'s Map State:
IsViewable for both), correct stacking (nested list fully visible and unobstructed, matching the
originally reported bug's opposite), and closing just the nested popup correctly restores the
ancestor with no side effects.

## Bug 2: opening the standard ColorDialog closes the ancestor popup

**Report** (with a real-Windows reference screenshot from a different session — same app,
different theme, both windows correctly shown together): clicking "Custom colour..." inside the
"A" popup's Text Color / Background Color picker opens the standard Windows `ColorDialog`, but
the "A" popup closes outright the instant it opens, instead of staying open in the background the
way real Windows does.

### Investigation

`CommonUIUtils.ShowColorDialog(IWin32Window owner, ref Color selectedColor)` is the single shared
entry point every "Custom colour..." link in the app funnels through
(`ControlFontSelect.controlColorSelect_CustomTextColor_Clicked`/`_CustomBackColor_Clicked`,
`ControlFontColorSelect.controlColorSelect_CustomBackColor_Clicked`, plus several unrelated
Settings/Category/theme-form callers with no `ToolStripDropDown` ancestor to begin with) — a
plain `using ColorDialog { ...; colorDialog.ShowDialog(owner); }`.

Reading (not guessing at) `System.Windows.Forms`' own decompiled source found the actual
mechanism: EVERY open `ToolStripDropDown` is auto-closed by a single, global, always-installed
`ToolStripManager.ModalMenuFilter.PreFilterMessage` `IMessageFilter`, which calls
`PInvoke.GetActiveWindow()` on every pumped message and, whenever the active window changes,
closes the currently-open dropdown chain via `ProcessActivationChange()` UNLESS the newly active
window is recognized as belonging to that same dropdown's own window hierarchy
(`IsChildOrSameWindow` against the dropdown's own hwnd and Wine's own `ActiveHwnd` tracking).

A `ColorDialog.ShowDialog(owner)` call passes a CHILD CONTROL of the popup as owner; on real
Windows the OS resolves that into a genuine native owner relationship with the popup's own
top-level window automatically, so `IsChildOrSameWindow` recognizes it and
`ProcessActivationChange()` never even fires. Under Wine, the same owner-chain recognition gap as
bug 1 above means the dialog's window isn't recognized as belonging to the popup's hierarchy,
`ProcessActivationChange()` DOES fire — and crucially, `ProcessActivationChange()`'s own source
shows it explicitly SKIPS closing any dropdown whose `AutoClose` property is `false`:

```csharp
if (GetActiveToolStripInternal() is ToolStripDropDown { AutoClose: not false } d) d.Visible = false;
```

Toggling `AutoClose` off for the duration of a self-spawned dialog and back on afterward is
WinForms' own sanctioned mechanism for exactly this scenario — real Windows just doesn't need it
here because its owner-chain resolution already succeeds; this app's own code never had reason to
reach for it before, since nothing else it does from inside a popup spawns a real modal dialog.

### AutoClose alone is not sufficient

Confirmed live via the same diagnostic tooling: with only the `AutoClose` suppression, the
ancestor popup's `Visible` property correctly stayed `true` and its native window stayed mapped
(`xwininfo`'s Map State: IsViewable) for the whole time the dialog was open — the LOGICAL close
was genuinely prevented — yet it rendered completely blank in a live screenshot. A separate
paint/stacking symptom, not the close bug: the window is mapped but never gets its content
repainted or its Z-order reconciled once Wine hands "active window" status to the spawned dialog.

Fixed by adding `PopupWindowHelper.RepaintAndRestackForSpawnedDialog()`
(`Invalidate(true)`, `Update()`, then a plain
`WinApi.Win32.SetWindowPos(Handle, HWND_TOP, 0,0,0,0, SWP_NOSIZE|SWP_NOMOVE|SWP_NOACTIVATE)`), and
having `ShowColorDialog` queue it via
`ancestor.BeginInvoke((Action)ancestor.RepaintAndRestackForSpawnedDialog)` immediately after
suppressing `AutoClose`, BEFORE calling `colorDialog.ShowDialog(owner)`. This relies on a
confirmed-live behavior: a `BeginInvoke` posted just before a blocking `ShowDialog()` call still
gets processed by that dialog's own nested modal message loop once it starts pumping, not only
after the dialog closes — the same mechanism the diagnostic tooling itself used to interact with
the app while a modal dialog it opened was still on screen.

Verified live end-to-end: both `AutoClose` suppression alone (dialog opens, ancestor logically
stays open but invisible) and the full fix together (ancestor visibly renders alongside the
dialog, matching the user-supplied real-Windows reference screenshot) were screenshotted and
compared before folding both into one patch. Closing the dialog afterward correctly leaves the
ancestor open and fully visible, exactly matching real-Windows behavior end-to-end.

## Bug 3: picking a color and clicking OK closes the ancestor popup anyway

**Report**: "When a custom colour is picked from the colour picker (or the cancel/ok button
pressed in it)… the font/style dialog closes… which it shouldn't because we're still setting
styles." Reported as a follow-up after bug 2's fix above had already shipped.

**Investigation, and a real methodology gap found along the way**: the FIRST reproduction attempt
used `wmctrl -c "Colour"` (the same tool used to dismiss the dialog throughout bug 2's own
investigation) to simulate Cancel — this did NOT reproduce any closing of the ancestor, with
either the old or the (then-current) new code. This was a genuine test-methodology blind spot:
`wmctrl -c` sends an asynchronous X11-level `WM_DELETE_WINDOW` request, incurring enough of a
round-trip delay before Wine's message loop ever sees it that any timing-sensitive behavior a
REAL, instantaneous button click would trigger (same-thread, no scheduling gap between
`ShowDialog()` returning and whatever runs next) never actually gets exercised. Discovering this
led to finding that **`python3-xlib`'s `Xlib.ext.xtest` module is available in this environment**
and can synthesize genuine mouse clicks (`XTestFakeInput` under the hood) at real screen
coordinates — a real UI-automation capability this project's own `CLAUDE.md` documented as
*absent* ("no xdotool/ydotool/xte"). See this file's own note in `CLAUDE.md` for the correction;
this changes what's possible for future investigation in this project, not just this one bug.

Reproducing with a real synthesized click on the dialog's own OK button (after also clicking a
basic color swatch) confirmed the bug: the entire ancestor "A" popup closed. Reproducing with a
real click on Cancel specifically did NOT close it, with either the old or new
`ShowColorDialog` code — ruling out the `AutoClose`/`ModalMenuFilter` mechanism (bugs 1/2's own
root cause) entirely for this symptom; it also retroactively means the earlier "reproduced on
Cancel" reasoning that motivated deferring the `AutoClose` restore via `BeginInvoke` (see bug 2's
own section) was never actually confirmed live with a real click — that deferral is kept anyway
as a low-cost defensive measure, but the doc comments were corrected to stop overclaiming it as a
confirmed fix for a confirmed bug.

**Real root cause**: entirely separate from bugs 1 and 2, and pre-existing (not something the
earlier patches introduced). `ControlToolStripButtonFontSelector`'s own
`controlFontSelect_SelectedTextColorChanged`/`_SelectedTextBackColorChanged`, and
`ControlToolStripButtonFontColorSelector`'s own `ControlFontColorSelect_SelectedColorChanged`,
each end with an explicit `base.popupHelper.Close();` whenever `TextColor`/`BackgroundColor`/
`Color` actually gets set — genuine, original app design, for the ordinary case of picking a
color from the quick swatch grid directly inside the popup (pick it, you're done, the whole
panel closes — confirmed still correct and unchanged, see the regression check below). The
custom `ColorDialog`'s own OK path raises the exact same `SelectedTextColorChanged`/
`SelectedColorChanged` event used by a quick swatch click, so it inherits the identical `Close()`
call — but the user is still mid-dialog-interaction at that point, not finished with the popup
the way a quick swatch click implies.

**Fix**: a new public field, `PopupWindowHelper.SuppressCloseOnColorPicked`, set `true` by
`ShowColorDialog` alongside the existing `AutoClose = false` (and reset via the same deferred
`BeginInvoke` restore bug 2 already added), checked by all three `Close()`-calling handlers above
— `if (!popupHelper.SuppressCloseOnColorPicked) { popupHelper.Close(); }` — rather than touching
`Close()` itself (a shared, heavily-called method; adding a guard at each of the three specific
CALLERS keeps the blast radius to exactly those three call sites, matching this project's own
"add alongside, don't mutate a shared member" convention, lesson 10, applied here to a caller
rather than a callee).

A second real IL-patching bug was caught and fixed while building this specific guard (see the
next section) before it ever reached a live test.

Verified live end-to-end via real synthesized clicks: picking a color and clicking OK now leaves
the ancestor popup open, with the picked color visibly applied (the "Custom colour…" swatch
itself updates to reflect it); Cancel already left it open with either code and still does;
clicking a quick swatch directly (bypassing the dialog entirely) still closes the panel
immediately, confirming the original, unrelated UX is unchanged.

## Two real bugs caught in the fix itself, before either ever shipped

While generalizing `--patch-colordialog-suppress-autoclose` to also handle 11.0.196-beta's
`ShowColorDialog` (which has an extra early-return branch 10.4.5674 doesn't — a
`ColorPickerFactory` extensibility hook checked before the classic `ColorDialog` path, confirmed
via `--dump-il`, not assumed from the decompile), the FIRST version of the "retarget other early
returns into the restore block" logic accidentally included the new restore block's OWN
instructions in its own retargeting sweep. This corrupted the "ancestor == null → skip to `ret`"
guard into a self-referencing jump, decompiling as an infinite loop:
`do { ancestor = findParentPopupHelper(); } while (ancestor == null);`. Caught by the mandatory
re-decompile step before ever deploying — fixed by excluding every instruction in the
newly-inserted block (not just its first instruction) from the retargeting sweep. The exact same
class of mistake IL-patching lesson 8 already documents (never reuse a real instruction as a
branch-target label without checking what else might target the same original anchor) —
recorded here since it recurred in a new shape (a *generated* label corrupted by a *later*,
unrelated pass over the same instruction list, not a hand-picked label reused across two paths).

While building bug 3's fix, a second, unrelated instance of IL-patching **lesson 2** ("inserting
before a branch target without retargeting skips the new code") turned up in a NEW shape.
`ControlToolStripButtonFontColorSelector.ControlFontColorSelect_SelectedColorChanged`'s own
`ColorChanged?.Invoke(this, EventArgs.Empty);` compiles to a null-conditional check whose "no
subscribers" path (`br.s`) branches PAST the invoke straight to the `ldarg.0` that begins the
`base.popupHelper.Close();` sequence — exactly the instruction the new guard was inserted before.
The first version of `GuardPopupCloseTail` inserted the guard without checking for this, so
whenever `ColorChanged` had no subscribers the branch would skip the guard entirely and call
`Close()` unconditionally, silently defeating the fix for that one specific method (confirmed by
decompiling the result: `goto IL_0043;` appearing INSIDE the `if (eventHandler != null)` block
rather than governing the `Close()` call, meaning the guard only applied on the "has subscribers"
path). Caught before ever deploying by re-decompiling and noticing the unexpected `goto` — not by
any static check, matching this project's own long-standing "decompiling clean is not sufficient
proof, but decompiling *unclean* is real signal" pattern. Fixed by making `GuardPopupCloseTail`
retarget any pre-existing branch or exception-handler boundary pointing at its own insertion point
to the new guard's first instruction instead, the same sweep already used for
`SetVisibleCore`/`ShowColorDialog` elsewhere in this same patch. The other two guarded methods
have no such null-conditional branch and were unaffected either way, but the retargeting logic
now applies uniformly regardless, rather than relying on which specific methods happen to need it
today.

## Cross-version verification

Confirmed via decompile-diff against all three supported builds before porting, not assumed:

- **10.4.5674**: `PopupWindowHelper.SetVisibleCore`/`findParentPopupHelper` and
  `CommonUIUtils.ShowColorDialog` match the shapes described above exactly.
- **11.0.196-beta**: `findParentPopupHelper()` is byte-for-byte identical to 10.4.5674's copy.
  `SetVisibleCore` is structurally identical (one added `HideShadowWindow()` call on the
  `!visible` path, and a renamed private field the patch doesn't touch, neither affecting the
  patch). `ShowColorDialog` has the extra `ColorPickerFactory` early-return branch described
  above — handled generically by making the "redirect other returns" logic apply to ANY `Ret`
  instruction in the method, not a version-specific carve-out.
- **11.0.282**: both patches applied and decompiled cleanly against this build's own pristine
  snapshot with no further adaptation, matching this pipeline's established pattern of sharing
  patch content across 11.0.196/11.0.282 whenever nothing needs to differ.
- `ControlToolStripButtonFontSelector`/`ControlToolStripButtonFontColorSelector` (bug 3's own
  guarded types) and their three targeted methods exist with the identical tail shape in all
  three builds; the null-conditional branch found in `ControlFontColorSelect_SelectedColorChanged`
  (see the lesson-2 account above) is present in all three too, and the fix's generic branch/
  handler retargeting sweep handles it uniformly without needing to special-case any one build.

Both patches, applied to all three builds, verified via decompile (clean, matching intended C#
shape) and `--dump-handlers` (no handler-ordering violations) before any live deployment. Full
end-to-end `deploy.sh` runs (not just manual `il-patcher` invocations) confirmed reaching the
expected stage mask and launching cleanly with zero new crash reports for 10.4.5674 (twice — once
before bug 3's fix, once after, both via a full fresh `deploy.sh` run against a bottle reset to
pristine) and 11.0.196-beta; 11.0.282 confirmed via the same decompile+handler verification (no
separate bottle was available for a live end-to-end `deploy.sh` run against that exact build this
session). Bug 3's own fix was additionally verified live via real synthesized mouse clicks
(`python3-xlib`'s `Xlib.ext.xtest`, not just the reflection-based diagnostic trigger used for bugs
1 and 2) on the actual dialog buttons at their real screen coordinates — see bug 3's own section
above for why this mattered and what it found that a `wmctrl -c`-based test had missed entirely.
