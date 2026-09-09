# eM Client 11 (beta) message preview pane intermittently goes blank

**Status: mitigated, confirmed working live.** The real Wine/Chromium mechanism was never fully
pinned down (see "What was ruled out" and "What was confirmed but not explained" below) — this is
a pragmatic periodic-repaint mitigation, the same shape already proven for an analogous bug (see
`reports/notification-empty-until-fade-findings.md`), not a root-cause fix. Fixed by
`releases/11.0.196-beta/deploy.sh` Stage 4, `--patch-preview-pane-periodic-repaint`.

Separate product line from the rest of this repo's fixes — see CLAUDE.md's "eM Client 11 (beta) —
separate release line" section.

## Symptom

The message preview pane (a single Chromium/CEF-hosted window inside `ControlMessageDetail`)
intermittently fails to display content. The blank region varies between occurrences: sometimes
just the header (sender/recipient/date), sometimes a white square somewhere in the body,
sometimes the entire email. Reproduces on a real physical machine as well as inside a VM. Moving
the mouse into the window reliably (and instantly) brings the content back. Reproduces on
already-read mail, not just unread — ruling out an early theory involving the mark-as-read timer.

The clearest, most reliable trigger found this round: in a VM accessed over SPICE remote display,
the pane reliably goes blank and *stays* blank for as long as the mouse cursor is outside the
VM's display, recovering the instant the cursor re-enters.

## Investigation

### Video-frame analysis

Screen recordings (not screenshots — screenshots don't capture the freeze/recovery transition)
were captured while reproducing the bug, then diffed frame-by-frame (crop to the content pane,
downscale, sum per-pixel absolute difference between consecutive frames). This established two
facts before any tracing was done:

- The frozen pane is genuinely static — pixel-identical across many consecutive frames, not a
  slow degrade.
- Recovery happens within a single video frame (≤0.25s in a 4fps capture) the moment the mouse
  becomes active in the window again — not a gradual re-render. This means the correct content
  was already fully composited and sitting in memory; it just wasn't being presented.

(Hover-preview popups that appear when hovering over an email in the list, or over an attachment,
are a separate, unrelated UI element and were explicitly excluded from every recording/analysis
pass here.)

### CX_DEBUGMSG trace

A `CX_DEBUGMSG="+message"` trace of the unpatched build, captured specifically around a
reproduction of the "mouse leaves the VM" trigger, showed:

- A specific hwnd (confirmed to be the CEF-owned `Chrome_WidgetWin_*` child window —
  `CefWebBrowserEx.NativeBrowserWindowHandle`, not the outer WinForms control) went a clean
  19.54 seconds with **zero `WM_PAINT` dispatches**.
- That silence lines up essentially exactly with a period where **no mouse input of any kind**
  (`WM_MOUSEMOVE`, `WM_MOUSELEAVE`, `WM_NCHITTEST`, `WM_SETCURSOR` — all checked) reaches the
  process at all, consistent with "the mouse left the VM's display" being a real, total
  input-delivery gap at the Wine/X11 level, not just an app-level focus change.
- `WM_ACTIVATE`, `WM_NCACTIVATE`, `WM_SETFOCUS`, `WM_KILLFOCUS`, `WM_ACTIVATEAPP` never appear
  **anywhere** in the entire trace (count = 0 each) — this Wine build's message-spy either doesn't
  surface these for this window, or they genuinely never fire, either way ruling out a
  focus-message-driven explanation.

### Two red herrings ruled out by frequency, not accepted on first appearance

Two signals appeared right at the recovery moment and looked promising at first:

1. A `WM_TIMER` on a window titled `"TimerNativeWind"` (a .NET WinForms internal
   `Timer.TimerNativeWindow` implementation detail, unrelated to Chromium's occlusion tracking
   despite the superficially similar name).
2. A burst of `libcef.dll` `THREAD_ATTACH` events (new CEF worker threads spinning up).

Both were checked against the OTHER, smaller `WM_PAINT` gaps elsewhere in the same trace and found
to occur constantly, every ~25ms (timer) or routinely (thread spawn) throughout the *entire*
trace, freeze or not — coincidental noise, not a real signal.

## What was ruled out: Chromium's native window occlusion tracking

The leading root-cause theory going in: Chromium has shipped a genuine, documented "Native Window
Occlusion Tracking" feature since ~v85 (`ui/base/win/window_event_target.cc` et al.) that
periodically decides whether its own native window is occluded using native Win32 visibility
signals, and stops presenting composited frames to a window it believes occluded — re-evaluating
on signals like the window regaining focus or receiving mouse input. This is a real Windows/DWM
heuristic with a documented history of misfiring under emulated/virtualized window-manager setups,
and it matched every observed detail: instant recovery (frame already composited, just not
presented), arbitrary freeze duration (occlusion state is sticky until reconsidered), a mouse-enter
event (not a click) triggering recovery, only the Chromium-hosted pane ever affected, and the
specific blanked region varying between runs.

Chromium exposes a kill switch for this: `--disable-features=CalculateNativeWinOcclusion`.
`il-patcher --patch-disable-native-win-occlusion` (MailClient.dll,
`MailClient.Program/DemoApp.OnBeforeCommandLineProcessing`) prepends this to the app's existing
`--disable-features` list via a plain `Ldstr` operand rewrite — the lowest-risk patch shape in this
project (no instruction insertion, no exception handlers touched). Confirmed identical IL (same
string, same offset) across every supported build (11.0.196, 11.0.282 x86, 11.0.282 x64).

**Deployed live and empirically refuted.** Confirmed active in the running process's actual
command line (`--disable-features=CalculateNativeWinOcclusion,...` visible via `ps aux`), and the
user reproduced the identical "mouse leaves the VM → pane freezes → mouse re-enters → instant
recovery" behavior with the flag applied: *"I seems to behave exactly the same."* Whatever the real
mechanism is, it isn't (solely) Chromium's own documented occlusion-tracking feature. The flag
remains in `il-patcher`, unused by any `deploy.sh`, as a documented dead end.

## What was confirmed but not explained

The trace work above pins down *when* the freeze happens and confirms the mouse-input-starvation
correlation, but the actual mechanism that stops (and later resumes) `WM_PAINT` delivery to the
CEF child window was never identified. Neither of the two candidate signals found at the recovery
moment held up under scrutiny, and no other Windows message appeared to distinguish the recovery
instant from any other moment in the trace. Two live possibilities that would need lower-level
tooling (e.g. a native debugger breakpoint on `RedrawWindow`/`InvalidateRect`/`SetWindowPos` in the
target process, or a `+bitblt,+region` trace) to actually settle:

- Something in Chromium's own compositor/presentation path (not the occlusion-tracking feature
  specifically) is deciding not to present frames while it believes the window uninteractive.
- This is a Wine/X11 input-plumbing effect below the Windows-message layer entirely — the X server
  genuinely stops delivering any events to the Wine process while the SPICE pointer is outside the
  display, and nothing app-visible ever signals "resume painting" at all; the pane simply needs an
  externally-forced repaint to show what's already correctly composited.

## Fix: periodic forced repaint (mitigation, not root cause)

Rather than keep chasing the exact mechanism, this reuses the same pragmatic approach already
proven for an analogous Wine paint-staleness bug in this codebase
(`--patch-notification-periodic-reblit`, see `reports/notification-empty-until-fade-findings.md`):
periodically force a real repaint of the already-correct content, independent of whatever normally
would (or wouldn't) trigger one.

Unlike the notification form (a layered bitmap blit), the CEF browser here is a real native child
window, so "force a repaint" means the actual Win32 API, not a WinForms-level `Invalidate()` (the
CEF child window paints itself directly — it isn't reachable through WinForms' own paint
pipeline) and not `SendMessage(WM_PAINT)` (does nothing useful without an actual invalid region
already queued).

`--patch-preview-pane-periodic-repaint` (MailClient.dll,
`MailClient.UI.Controls.ControlMessageDetail.ControlMessageDetail`):

- Adds a new, private `user32.dll` P/Invoke, `__RedrawWindow(nint, nint, nint, uint)`, declared
  directly on `ControlMessageDetail` — self-contained within one assembly/one type rather than
  extending the shared `WinApi.dll` `Win32` class (which has `SetWindowPos`/`SendMessage`/
  `PostMessage` but no `RedrawWindow`/`InvalidateRect`), avoiding any cross-module
  `ImportReference` complication (IL-patching lesson 16) or shared-type blast radius (lesson 10)
  entirely.
- Adds a `private System.Windows.Forms.Timer __previewPaneRepaintTimer` (400ms), its `Timer` type
  resolved from the existing `timerMarkRead` field's own `FieldType` rather than `typeof()`
  reflection (System.Windows.Forms is app-deployed, not a CoreLib facade — lesson 5).
- Adds `__previewPaneRepaintTick`, which no-ops if `webBrowser` is null, not `Visible`, or its
  `NativeBrowserWindowHandle` is `IntPtr.Zero` (mirroring the property's own internal null-guard),
  otherwise calls `__RedrawWindow(handle, NULL, NULL, RDW_INVALIDATE|RDW_ERASE|RDW_ALLCHILDREN|
  RDW_UPDATENOW)` against that handle — the real `Chrome_WidgetWin_*` hwnd, obtained via CEF's own
  public `NativeBrowserWindowHandle` property rather than guessed from a WinForms `Control.Handle`.
- One-time creation/start of the timer, appended at the very end of `ControlMessageDetail`'s public
  constructor, immediately before its final `ret`. Confirmed via `--dump-il` and `--dump-handlers`
  that this constructor has no exception handlers and nothing branches to that final `ret` (plain
  sequential code, no early `return;` anywhere in the body) — a safe insertion point needing no
  retargeting (lessons 2 and 3 don't apply). The constructor runs exactly once per control
  instance (the pane is created once and reused as the user selects different messages), so no
  re-entry guard is needed the way the notification form's `OnShown` needed one.

### Verification

Decompiled cleanly (`ilspycmd -t` on the patched type shows the new field, P/Invoke declaration,
and tick method exactly as intended) and `--dump-handlers` reported no violation on the
constructor. Applied identically against both a pristine `original/em-11.0.196/MailClient.dll` and
the live, already-Stage-1–3-patched bottle copy (different starting bytes, same patch content,
same success message) — confirmed the interpolation-mode scan (Stage 1) still reports all 13 sites
correctly afterward, i.e. this patch doesn't disturb earlier stages.

Deployed live to `emClient_11_beta_win_11`. User reproduced the freeze deliberately (moved the
mouse out of the VM for 20-30 seconds) and confirmed the pane now recovers on its own within about
a second, without needing a mouse move: *"It seems to work. I've seen it go blank, then within a
second come back and stay pretty stable visible. This looks like the fix for this issue."*

## Takeaway

When a paint-staleness bug's exact trigger mechanism resists identification even after ruling out
the most plausible documented cause (occlusion tracking) and confirming the behavioral shape in
detail (instant recovery, arbitrary duration, total input-delivery correlation), a periodic forced
repaint of already-correct content is a legitimate, low-risk mitigation — not a cop-out — as long
as the underlying content is confirmed already-correct (via frame diffing) rather than papering
over an actual rendering bug. This is the second time this exact class of fix has worked in this
project (see the notification toast's `--patch-notification-periodic-reblit`); if a third
Wine paint-staleness bug turns up, look here first before re-running a full trace investigation
from scratch.
