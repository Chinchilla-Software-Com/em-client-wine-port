# New-mail notification toast shows empty until it starts to fade out — UNRESOLVED

> Extensively investigated across multiple rounds (direct app instrumentation, three independent
> fix attempts, three separate trace configurations, a desktop-compositor test, pixel-level
> screenshot analysis, and a frame-by-frame screen-recording analysis). Every mechanism checked is
> confirmed working correctly. No root cause found. Documented in full so this doesn't get
> re-investigated from scratch, and so the next session has a clear list of what's already ruled
> out.

## Symptom

A new-mail notification toast appears showing its chrome (header strip + body background) but
with no visible text (sender/subject) for several seconds — matching the notification's
configured display-hold duration — then the text becomes visible right as the toast starts to
fade out. Same box, same screen position throughout (confirmed by the user, not two separate
notifications).

## Environment

- `emClient_win_8_x64` bottle, CrossOver/Wine, eM Client 10.4.5674, fully patched (all 7 existing
  stages applied and confirmed working).
- Host desktop: Linux Mint 22.3 (Cinnamon), Muffin (Mutter-derived) window manager/compositor.

## Confirmed, via direct app instrumentation (`il-patcher --patch-diag`, instruments
`FormGenericNotification.OnPaint`/`timer_OnTimer`)

- Exactly one notification window is involved (`FormMailNotification`, 310×125) — an earlier
  trace-only investigation mistakenly chased a second, unrelated window; corrected once app-side
  instrumentation gave an unambiguous identity for the real notification.
- `title`/`content` fields (the data `OnPaintTitle`/`OnPaintContent` draw from) are correct from
  the very first `OnPaint` call. Not an async/data-loading timing bug.
- The animation state machine behaves exactly as designed: `Appearing` → `Visible` (a multi-second
  silent hold — the timer is deliberately reconfigured to a single long interval for this hold,
  matching the `NotificationsHideTimeout` setting, so seeing no ticks during it is expected) →
  `Disappearing` (fires every ~25ms as fade-out proceeds).

## Two fix attempts tried, both confirmed ineffective (user-tested)

Both landed in `il-patcher`'s `--patch-notification-invalidate` mode (kept in the tool, marked as
ineffective, per this project's convention of keeping failed attempts documented rather than
re-explored):

1. **Add `Invalidate()` at the `Appearing`→`Visible` transition** (the one branch in
   `timer_OnTimer` that changes Opacity without also calling `Invalidate()`, unlike every other
   opacity-changing branch in the same method). No observable difference.
2. **Force a genuine `SetLayeredWindowAttributes` call** at that same transition (nudge `Opacity`
   to a value that rounds to a different native alpha byte, then immediately back to 1.0), on the
   theory that a real alpha *change* — not just a repaint request — was needed. No observable
   difference.

Both were later confirmed, via trace, to have been patching mechanisms that were never actually
broken (see below) — which is why neither had any effect.

## Confirmed, via combined `CX_DEBUGMSG` trace + app instrumentation, correlated on the correct,
unambiguous window (matched by its confirmed 310×125 size)

- `window_surface_flush` pushes a full-window dirty region (`(0,0)-(310,125)`) to the X11 surface
  promptly after each paint — not a small/partial region. Wine's surface-push mechanism works
  correctly.
- `SetLayeredWindowAttributes` is called correctly throughout: alpha+colorkey blending during
  fade-in/fade-out, switching to colorkey-only mode once the window is fully opaque (a normal,
  correct WinForms optimization when both `Opacity` and `TransparencyKey` are in play — not a
  bug).

## Confirmed, via a further trace with `+gdiplus,+font` channels added

- `NtGdiExtTextOutW` fires with identical text, position, and flags on the very first paint as on
  a later, visually-working paint.
- Individual glyph rasterization (`NtGdiGetGlyphOutline`, one call per character) fires correctly
  for every character of the real notification text on that same first paint.

GDI genuinely processes and rasterizes the correct glyphs, at the individual-character level, on
the very first paint.

## Third fix attempt, also confirmed ineffective — and why the first two couldn't have worked

The working fade-out phase isn't just "opacity changes" — it's dozens of *separate* ticks ~25ms
apart, each a genuine pass through the OS message loop, giving Wine's X11 compositor real
wall-clock time and repeated opportunities to catch up. A single synchronous property-setter pair
(attempts 1 and 2) can't replicate that. Third attempt: deliberately trigger a brief, real
dip-and-recover using the actual opacity/`Invalidate()` machinery, with real elapsed time between
each step (`Application.DoEvents()` to pump the message loop, then `Thread.Sleep()` to give the
compositor genuine wall-clock time), right at the `Appearing`→`Visible` transition.

Hit a real Mono.Cecil bug while building this one, documented as a new lesson in `CLAUDE.md`'s
"IL-patching lessons": the patch's 48 inserted instructions pushed a nearby short-form branch
(`bne.un.s`, 1-byte relative offset) out of range, and Cecil silently emitted a corrupted branch
rather than erroring — caught by decompiling before deploying (garbled control flow, spurious
"stack underflow"). Fixed with `body.SimplifyMacros()` (from `Mono.Cecil.Rocks`) before inserting.

**Confirmed ineffective via a full-screen video recording of a live repro** (not just user report
this time — frame-by-frame pixel analysis of the recording): the notification box appears blank at
~25.1s into the recording, stays *completely static* (zero pixel change across ~30 sampled frames)
until ~28.2s, when text becomes visible as the box starts fading out — the same multi-second
blank-then-fix pattern as the unpatched build. The fix's dip-and-recover sequence executes within
~150ms of the box appearing (confirmed present in the deployed build's decompiled IL), yet nothing
observable happens for another ~3 seconds.

The sharper finding from the recording: during that blank window, the fix forces `Opacity` from
1.0 down to 0.1 and back — a drastic, deliberate change, several times larger than anything
naturally occurring during the hold. **Zero visible flicker in any sampled frame.** Not just the
text — the entire window's on-screen appearance is unresponsive to a large forced opacity swing
during this period. That's a stronger, more specific result than "text doesn't render": the
window's visual state appears completely frozen from the compositor's perspective, not just its
content.

## Ruled out: Cinnamon/Muffin's window-open animation effect

A documented, older Cinnamon bug (linuxmint/cinnamon#3547: the "Fade" map effect for newly-opened
windows leaving them with broken opacity until manually restored) looked like a strong candidate
for a compositor-level explanation, and would fit the general shape of the symptom. **User tested
by disabling all Cinnamon desktop effects entirely — no change.** Ruled out.

## Confirmed, via pixel-level analysis of the existing screenshot
(`supporting/notifications-no-text-until-fade-out.png`, captured during the broken state)

Sampling the notification's body region: only ~22 distinct colors across ~22,000 pixels, all
within a narrow, smooth range — consistent with a plain gradient background, not text. Real
anti-aliased text produces hundreds of distinct colors with sharp bright/dark edges; there is none
of that here. **The content is genuinely absent from what's composited to screen at that moment —
not a low-contrast/color problem, not something subtly present.**

## Confirmed, via reading Wine's actual `winex11.drv` source (`dlls/winex11.drv/window.c`,
wine-mirror/wine)

`X11DRV_SetLayeredWindowAttributes` does **no compositing itself** — it only sets the standard
X11 `_NET_WM_WINDOW_OPACITY` property (`sync_window_opacity()`): `XChangeProperty` for any
alpha value other than fully-opaque, or **`XDeleteProperty`** when opacity resolves to exactly
0xffffffff (alpha=255, i.e. `Opacity = 1.0`). Actually compositing the window — reading that
property plus the window's damage/content and blending it onto the screen — is entirely the
window manager/compositor's job, not Wine's. This is consistent with everything already
confirmed: Wine's own responsibility (paint into its surface, set the property, flag damage) is
demonstrably done correctly and completely; the remaining step is out of Wine's hands.

This produced a specific, testable hypothesis: that the `XChangeProperty`-vs-`XDeleteProperty`
split (a real, different code path depending on whether opacity is *exactly* 1.0) causes
different compositor behavior. **Tested directly and found not to be it** — see the third fix
attempt above (the dip-and-recover sequence deliberately avoids ever landing on exactly 1.0
mid-sequence and still produced zero effect).

## Five minimal-repro-app iterations, none reproduce the bug

To get a much faster iteration loop than patching `MailClient.dll` via Cecil for every experiment,
built a small standalone WinForms app (`notif-repro.exe`, self-contained `net8.0-windows`/`win-x86`
publish, run directly under the same Wine bottle) replicating the real mechanism piece by piece.
Each iteration was checked via a cropped/full-screen `ffmpeg` recording of a live run (frame
extraction + distinct-color-count sampling, not just a single screenshot), so "does it show text
immediately" is a measured result, not a guess:

1. **Base mechanism**: `WS_EX_LAYERED` (`AllowTransparency` + `TransparencyKey`), `Opacity`
   animated by a `Timer` (fade in → hold → fade out, same 25ms tick interval), `UserPaint` +
   `AllPaintingInWmPaint` + `OptimizedDoubleBuffer`, `TextRenderer.DrawText` content (matching the
   real `NtGdiExtTextOutW` call, not GDI+'s `DrawString`). **Text rendered correctly and
   immediately** — no blank period at all.
2. **+ drop-shadow companion window** (separate `WS_EX_LAYERED` hwnd driven by
   `UpdateLayeredWindow`, matching `LayeredBaseForm.layeredWindow`) **+ raw
   `ShowWindow`/`SetWindowPos(HWND_TOPMOST)` show sequence** (bypassing WinForms' own `Show()`,
   matching `FormGenericNotification.Show()`'s real implementation). Still immediate.
3. **+ avatar/icon image** (a filled circle + initials, matching `FormMailNotification`'s
   `Image`/avatar area) drawn synchronously in `OnPaint`. Still immediate.
4. **+ secondary window while a large "main window" is already active** (an
   `Application.Run`-hosted mock main form shows first and stays open; the notification is created
   and shown ~1.5s later as an independent `Form.Show()` on the same UI thread) — closer to the
   real scenario where the notification is never the process's only/first window. Still immediate.
5. **+ cross-thread avatar delivery** (a background `Thread` sleeps ~180ms then marshals back via
   `Invoke()` to set the avatar and call `Invalidate()`, matching the real
   `AvatarManager_AvatarUpdated` → `SafeInvoke` → `Invalidate()` pattern instead of a same-thread
   synchronous draw). Still immediate.

None of these — individually or combined — reproduce the bug. This is a real, useful negative
result: it rules out the WinForms layered-window mechanism, the shadow window, the raw show
sequence, the avatar (both sync and async-via-cross-thread-Invoke), and "being a secondary window
alongside an active main window" as the cause in isolation. Whatever's different about the real
app is something this project's tooling hasn't been able to isolate yet — most likely something
about em Client's actual process complexity (multiple CEF subprocesses, genuine background
load from IMAP sync/indexing, or a specific Win32 call sequence not yet replicated) rather than a
flaw in the notification's own code shape.

The stub's source is tracked nowhere in this repo yet (built and iterated in a scratch directory)
— worth promoting into `il-patches/` (as a new "not itself deployed, but built and run against a
live bottle" tool, following `font-systemlink-writer/`'s precedent) if a future session picks this
back up, rather than rebuilding it from scratch.

## Ruled out: Muffin's X11-Sync-extension frame-freeze mechanism

Reading `linuxmint/muffin`'s actual compositor source (`src/compositor/meta-window-actor.c`,
`src/x11/window-x11.c`) turned up a real, named freeze/thaw mechanism: a window actor can be held
`INITIALLY_FROZEN` (or frozen later) based on `meta_window_x11_are_updates_frozen()`, which is
driven by the **X11 Sync extension "frame sync" protocol** — the compositor can hold a window's
displayed texture frozen (ignoring all further damage/content changes, and even opacity changes,
until it explicitly thaws) while waiting for the client to acknowledge frame completion via an
XSync counter, with a hard-coded 1-second timeout fallback if the client never responds. This
looked like an excellent candidate: it would explain the correct-content-but-frozen-display
symptom, and the "even a forced opacity swing does nothing" result from the third fix attempt,
in one mechanism.

**Ruled out directly via `xprop`, on both the stub and the real notification window.** This
protocol requires the client to advertise `_NET_WM_SYNC_REQUEST` in `WM_PROTOCOLS` and set a
`_NET_WM_SYNC_REQUEST_COUNTER` property; without both, the compositor never engages this freeze
path for that window at all (`send_sync_request()` explicitly checks
`window->sync_request_counter != None` first). Checked both: the real `LayeredBaseForm` window's
`WM_PROTOCOLS` is `WM_DELETE_WINDOW, _NET_WM_PING, WM_TAKE_FOCUS` only — no
`_NET_WM_SYNC_REQUEST`, no counter property anywhere. Wine simply doesn't participate in this
protocol for its windows (confirmed on both the stub's window and the genuine notification's
window, not just inferred from one). This specific mechanism cannot be the cause.

## Is this Wayland-specific (X11-only), i.e. would it go away under Wayland?

Very likely yes, though unconfirmed. Every mechanism traced in this investigation is deeply
X11-specific: `winex11.drv` setting the `_NET_WM_WINDOW_OPACITY` X11 property, X11
window-surface/pixmap flushing, and Muffin's X11-specific compositor code built on X11 extensions
(Sync, Shape). Under Wayland, Wine uses an entirely different driver (`winewayland.drv`) with a
fundamentally different compositing model — direct Wayland surface buffers, no X11 properties, no
X11 Sync extension. The specific interaction chased in this investigation wouldn't exist in that
code path at all. Two caveats: this hasn't been tested (there could be an analogous
Wayland-side bug waiting to be found), and CrossOver's own Wayland driver maturity may lag behind
or differ from upstream Wine's `winewayland.drv`.

## Where this leaves it

Every mechanism inspectable via app instrumentation and Wine API tracing — data correctness,
paint/timer state machine, GDI draw-call issuance, per-glyph rasterization, window-surface push,
and layered-window attribute handling — is confirmed working correctly, identically, on the first
(broken) paint as on a later (working) one. Yet the pixels genuinely don't reach the screen the
first time. The screen-recording evidence sharpens this further: the window's entire on-screen
appearance is frozen during the blank period, unresponsive even to a large forced opacity change
— not just its painted content. That's a real gap between "GDI/the app did the work" (confirmed,
down to individual glyphs, and confirmed the app-side opacity property genuinely changed) and "the
screen reflects it" — narrowed considerably from where the investigation started, but without a
specific mechanism identified, and three independent, reasonably-targeted fix attempts have not
moved it.

## Reference: confirmed-working native Windows behavior
(`supporting/notifications-working-example-from-windows.png`)

Captured on real Windows (not Wine) for comparison: a notification toast with sender/subject
text ("Test Sender" / "Re: Testing notifications") fully visible immediately, no blank period.
Confirms the expected behavior this project is chasing is standard/correct on native Windows —
i.e. this is purely a Wine/Linux-compositor-side gap, not a difference in how eM Client itself
behaves by design on different platforms.

## Fourth round: freeze duration tracks `NotificationsHideTimeout`, independent of app activity

Prompted by a user idea: rather than keep guessing at *what* unsticks the freeze, empirically
bisect *when* it lifts under deliberately different app behavior during the blank window, using
three new `il-patcher` modes (`--patch-notification-long-burst`, parametrized totalMs/stepMs/
minOpacity; `--patch-notification-silent-wait`, parametrized waitMs) plus live screen recordings
(`ffmpeg -f x11grab`, frame-extracted and diffed with Pillow) instead of relying on user-described
timing. All three tests below ran against `emClient_win_8_x64` with `NotificationsHideTimeout`
configured to 6000ms (confirmed directly: Settings -> Notifications -> "Hide after ___ seconds").

1. **6-second oscillating opacity burst** (`Opacity` triangle-waved between 1.0 and 0.05 every
   25ms, with real `Invalidate()` + `Application.DoEvents()` + `Thread.Sleep(25)` each step —
   241 steps, fully unrolled) inserted at the Appearing->Visible transition, ending pinned at
   Opacity=1.0 before falling through to the method's normal (unmodified) continuation. Recorded
   full-screen at 30fps. Result: blank from box-appear to **~t=5.9s**, then instantly stable with
   content visible for the rest of the ~26s recording, no further change.
2. **Same mechanism, 2-second burst** (81 steps) — the direct bisection test: if the burst's own
   activity were the cause, a 3x shorter burst should release 3x sooner. Result: blank until
   **~t=6.5s** (measured from box-appear, in a clean recording once the Claude Code terminal
   window -- which had partially occluded the first repeat of this test -- was moved off-screen).
   Essentially the *same* release point as the 6-second burst, not a proportionally shorter one.
3. **Silent wait -- no burst at all**: `timer.Stop(); Thread.Sleep(6000); timer.Start();` inserted
   at the same point, with the timer genuinely stopped (blocking any WM_TIMER reentrancy) and
   *zero* Invalidate/Opacity/DoEvents calls during the wait -- deliberately testing whether any
   paint activity is needed at all. Result: blank until **~t=7.4s**, content appearing roughly
   1-1.3s after the silent wait itself ended (the next real timer tick wasn't due for several more
   seconds at that point, so this wasn't a natural second tick firing early).

All three -- 2s of repainting, 6s of repainting, 6s of doing nothing at all -- released within
about a second of each other, at approximately `NotificationsHideTimeout`'s own value (6000ms),
**not** at a point proportional to or explained by what the app itself was doing in that window.
This is strong evidence against every fix attempt tried so far in this investigation (four now,
across two rounds): none of them were the actual cause of anything. The freeze increasingly looks
like a fixed wall-clock/compositor-side effect tied to elapsed time since the notification
appeared, which happens to track this setting rather than being fixed at some universal constant
(the original investigation's ~3.1s measurement was presumably taken under a different configured
`NotificationsHideTimeout` -- worth confirming if this gets revisited, but not re-tested this
round). Why a Wine/X11-side timing effect would track an eM-Client-internal setting value it has
no visibility into is not yet explained; one structural note worth chasing if this is picked up
again: `updateLayeredBackground(refreshBitmap: false)` is called on every tick regardless of
state, always with `refreshBitmap: false` at this transition -- worth checking whether that flag,
or something downstream of it, is gated by a duration that happens to be derived from
`timeToStay` (== `NotificationsHideTimeout`) rather than by wall-clock/compositor timing as such.

One incidental note from the recordings: at 30fps (33ms) resolution, the avatar and text both
became visible within the *same* captured frame in the burst tests -- I could not confirm the
user's separately-reported observation that the avatar renders before the text; that would need
a much higher frame rate to resolve.

All three experimental builds were reverted after testing; `emClient_win_8_x64` is back on the
confirmed-working stage-7 baseline (verified via `md5sum` against the pre-test backup taken in
`il-patches/backup/MailClient.dll.stage7-confirmed-working`).

## Fifth round: content becomes visible when `Hide()` runs, not at a fixed wall-clock mark

Two more tests, prompted directly by the user's own idea and an independent observation on a
second machine, sharpen (and partly supersede) the fourth round's "tracks `NotificationsHideTimeout`"
theory:

1. **Disable the fade animation entirely**, via the app's own existing `LayeredBaseForm.
   ShowWithoutFading` flag (`get; set;`, already used elsewhere for non-animated layered popups) --
   not a new mechanism, just forcing an existing one on. New `il-patcher` mode
   (`--patch-notification-no-fading`) sets `this.ShowWithoutFading = true;` in
   `FormGenericNotification`'s constructor, scoped narrowly there (not on `LayeredBaseForm` itself)
   so unrelated `LayeredBaseForm` consumers like `FormPopup` aren't affected. Result: the
   notification window is created at the correct position/size (confirmed via `xwininfo`: 310x125,
   `IsViewable`) but is **completely invisible for its entire lifetime** -- not even the chrome/
   background box that every other test showed immediately. Root cause of *that*: `OnShown`'s
   `ShowWithoutFading` branch never starts `timer` at all (only the fade-driven Appearing/
   Disappearing branches do), so `NotificationsHideAfterTimeout`'s auto-hide never engages either --
   the window is left permanently stuck with no scheduled future tick of any kind. Useful negative
   result: the repeated animation ticks aren't just cosmetic opacity changes, they're also
   apparently doing something necessary to make the layered window's content composite at all --
   removing them doesn't sidestep the bug, it just removes the one thing that (eventually) fixes it.
   **Re-tested and confirmed, focus ruled out as a confound:** eM Client is known to suppress
   notifications entirely when its own window has focus (normal, expected behavior), which raised
   a concern that this test's original result was really just that, by accident. Re-ran with the
   main window deliberately kept unfocused throughout (confirmed via `xprop -root
   _NET_ACTIVE_WINDOW` before and after the test email arrived) -- the notification window still
   appeared (`xwininfo`: correct 310x125 geometry, matching sender name as title, `IsViewable`) and
   was still **completely invisible** in a live capture of that exact screen region. The original
   conclusion holds: this is a genuinely permanent invisibility, not focus-suppression.
2. **The user's own test, independently, on a second machine**: with `NotificationsHideAfterTimeout`
   turned off via Settings (autoHide=false; normal fade animation still active, no patch involved)
   the notification displayed with no text and stayed that way *indefinitely* -- not ~6s, however
   long they left it. **Clicking it** made the avatar and text appear immediately, then it began
   fading out and opened the email. Reproduced on `emClient_win_8_x64` too (recorded): box blank
   from ~t=0.5s to ~t=16.5s (matching how long the user let it sit before clicking), content
   appearing right around the click at ~t=17s, then holding steady (the app then hung before it
   could finish opening the email/fading out -- see below).

This is materially better evidence than the fourth round's burst/silent-wait correlation: it shows
the release point moving to match an arbitrary, user-controlled event (a click, at whatever elapsed
time), not sitting at a fixed ~6s mark. The real trigger is calling `Hide()` (state -> Disappearing)
-- whether invoked by the timer naturally elapsing or by a mouse click -- not elapsed wall-clock
time as such. This reframes (without fully resolving) the fourth round's silent-wait result: that
test's ~7.4s release, notably *earlier* than the ~12s a naive reading of the inserted
`timer.Stop()`/`Sleep`/`Start()` sequence would predict before the next real tick could fire,
suggests some other path into `Hide()`/a tick fires sooner than modeled -- not re-investigated this
round, but worth reconciling if this picked up again, ideally alongside the `updateLayeredBackground`
angle noted above.

**New, separate wrinkle -- precisely characterized after further testing:** eM Client hangs
(stops responding, requiring force-quit -- which then forces an unavoidable DB-check dialog on the
next launch) when a no-text notification (auto-hide disabled) is clicked **after** the normal
~6-second `NotificationsHideTimeout` mark has passed. The user isolated this precisely: with
auto-hide back on, no freeze. With auto-hide off, clicking *within* the first ~6 seconds opens the
email cleanly, no freeze. Clicking *after* ~6 seconds elapses -- i.e. after the point where the
timer would naturally have fired and started the real fade-out, had auto-hide been on -- hangs the
app, reproduced twice in a row under this exact condition. This lines up neatly with everything
else in this investigation: something real changes in the notification's state right around
`timeToStay` elapsing (state was already suspected to matter more than wall-clock time as such,
per the click-triggers-`Hide()` finding above), and a user click arriving at/after that same moment
likely races with it -- plausibly a second, concurrent path into `Hide()`/the state machine (one
from the click handler, one from whatever fires at the timeout mark despite auto-hide being off)
deadlocking or corrupting shared state. Not yet root-caused; a strong, cleanly reproducible next
lead if this investigation continues, and probably worth its own instrumented test (e.g.
`--patch-diag`-style logging in `Hide()`/`timer_OnTimer`/the click handler) rather than more
screen-recording-based inference, since the mechanism is now narrowed enough to look at directly.

Both this round's builds/settings changes were reverted; `emClient_win_8_x64` is back on the
confirmed-working stage-7 baseline (`NotificationsHideAfterTimeout` should be re-confirmed still
enabled next session, since it was toggled off via Settings mid-investigation, not via a patch).

## Sixth round: extended `--patch-diag` with `Hide()`/`OnMouseClick`/click-handler logging; a
genuine crash surfaced, unrelated to the click test itself

Following directly from the fifth round's plan, `il-patcher --patch-diag` (`il-patches/
il-patcher-Program.cs`) was extended with three more instrumentation points beyond the existing
`OnPaint`/`timer_OnTimer` (same tick/handle/size/state/title/content logging pattern, reusing the
existing `Instrument()` helper unchanged for the two `FormGenericNotification` methods, plus a new
minimal `InstrumentMinimal()` — tick+type only — for `MailNotificationHandler`, which has no
state/title/content/`Control` fields to read):

- `FormGenericNotification.Hide()`
- `FormGenericNotification.OnMouseClick(MouseEventArgs)`
- `MailNotificationHandler.notificationForm_Click` (the shared handler behind
  `TitleClick`/`ContentClick`/`ImageClick`, which calls `PerformAction` → ... →
  `formMail.ShowMailForm(item)`)

Rebuilt the full pipeline from `original/8/` through all 7 stages (confirmed byte-identical to the
live `emClient_win_8_x64` bottle at stage 7 before patching further), applied `--patch-diag` on
top, decompiled all three new instrumentation sites before deploying (clean, no control-flow
corruption — none of the three methods have exception-handler regions to worry about either), and
deployed to `emClient_win_8_x64`.

**First logged event (tick 3206951, ~30s after a notification's first paint) turned out not to be
the click test at all.** The user clarified after reviewing the log: this notification arrived
while they had the Settings dialog open (independently confirming `NotificationsHideAfterTimeout`
was already off), and `Hide()` fired the moment they clicked Save-and-close on that dialog — not
from clicking the notification itself. This cleanly explains why neither `OnMouseClick` nor
`notificationForm_Click` logged anything before that `Hide()` call: the trigger wasn't a click on
the notification at all. Likely mechanism (not yet confirmed by instrumentation): closing Settings
reactivates the main window, and `MailNotificationHandler.UIUtils_ApplicationActivated` →
`handleAllNotifications()` → `OnNotificationHandled` for the pending tray/popup notification →
(presumably, via a presenter subscriber not yet traced) `removeCurrentNotification()` → `Hide()` —
i.e. app activation alone can dismiss a pending notification, independent of any click. Worth
confirming directly (instrument `UIUtils_ApplicationActivated`/`handleAllNotifications`) if this
activation-dismisses-notification path becomes relevant again. The `Hide()`→fade sequence that
followed (state 2→3, correct ~25ms ticks) looked entirely normal.

**Second event (tick 3285999, ~78.5s of total silence later) was the user's actual intended test
attempt (send a fresh notification, let it sit past 6s, then click) — but the whole Wine session
crashed before they got a chance to click.** The log shows exactly one `OnPaint` entry for this
new notification, and it's truncated mid-instrumentation: only the first of the four sequential
`File.AppendAllText` log writes for that `OnPaint` call landed before the process died (the label+
tick+type line; the handle/size/state, title, and content lines never got written). This pins the
crash to very shortly after entering this notification's first `OnPaint` — apparently before any
user click occurred at all, unlike every previous round's click-after-6s hang.

This is a **new failure mode**, more severe than anything hit in prior rounds: not a UI freeze
requiring force-quit, but the entire Wine session dying outright (`winewrapper.exe` and
`MailClient.exe` both gone from `ps`, confirmed via `ps aux`). Checked for the usual crash
artifacts and found none: no `bug.*.txt` in the bottle's Temp folder (the mechanism `MailClient`
itself uses to report *managed* exceptions), no `coredumpctl` entries, nothing in `journalctl -k`
or `dmesg` matching a segfault, no CrossOver-level crash log found. The absence of any of these
suggests either a very hard native-level crash CrossOver isn't configured to catch/report in this
environment, or the whole Wine process tree being torn down by something other than a single
faulting instruction (e.g. a fatal error in `wineserver` itself). Not yet explained.

Not yet reproduced a second time, so it's unknown whether this is: (a) a new, real crash bug
exposed only now because a notification finally got to sit through its full uninstrumented history
of state transitions with `Hide()`/`OnMouseClick` instrumentation freshly added (i.e. the new
instrumentation itself somehow contributing — though the *same* `OnPaint` instrumentation ran
successfully once already earlier in this exact session, for the first notification, arguing
against a simple instrumentation bug), or (b) a genuine pre-existing crash in the real app that
just hadn't been hit yet in this investigation, possibly connected to the same underlying
compositor-side gap this whole investigation is chasing. The bottle is back to idle (no Wine
processes running) and ready for another attempt with the same instrumented build already
deployed — next session/attempt should watch the process closely enough to catch the crash instant
live (rather than reconstructing it from a truncated log afterward) and immediately check
`coredumpctl`/`dmesg` right after it happens, in case the artifacts are still present momentarily.

## Seventh round: root cause of the double click-dispatch confirmed and fixed — `OnShown()`
double-subscribes `layeredWindow.Click` with no matching unsubscribe

Extended `--patch-diag` further to instrument `OnShown(EventArgs)` and `layeredWindow_Click`
directly, to settle the sixth round's open question about *why* one physical click fired
`notificationForm_Click` twice. Result, unambiguous:

```
OnShown tick=5554145 type=FormMailNotification   state=Hidden(0)     title="" content=""
OnShown tick=5554167 type=FormMailNotification   state=Appearing(1)  title="Test Sender" ...
OnPaint tick=5554178 ...
layeredWindow_Click tick=5555722 ...   (the click, 1st dispatch)
notificationForm_Click tick=5555725 ...
Hide tick=5555930 ...
layeredWindow_Click tick=5555933 ...   (the SAME click, 2nd dispatch, 211ms later)
notificationForm_Click tick=5555937 ...
```

`OnShown()` ran **twice** for this one notification — once early while the form was still blank
(`state=Hidden`, empty title/content), once again once `Title`/`Content` were populated — and both
runs executed `OnShown()`'s unconditional `layeredWindow.Click += layeredWindow_Click;`, with no
matching `-=` anywhere in the class. That subscribes the same handler to the same event twice, so
the next single `Click` raise invokes it twice. This fully explains the double-fire, and gives a
concrete, testable explanation for every previous round's flakiness (fine once, hung once, crashed
once): a `mailForm` singleton (see `MailNotificationHandler.EnsureValidNotificationForm`) reused
across a session accumulates one more subscription per `Show()`, so later notifications in a
longer session would double-fire (or worse) `PerformAction`/`ShowMailForm` more and more badly.
This also cleanly answers the fifth/sixth-round mystery of why `OnMouseClick` never once fired
(0/0 across every test): real clicks land on `layeredWindow`, whose own `Click` event dispatches
straight to `layeredWindow_Click`, entirely bypassing the main form's `OnMouseClick` override.

**Fix implemented and verified (not yet deployed to a live bottle at time of writing — pending
the user confirming eM Client is fully closed first, per this project's now-strict "always ask,
never close/kill it yourself" rule):** new `il-patcher --patch-notification-click-resubscribe`
mode inserts `layeredWindow.Click -= layeredWindow_Click;` immediately before the existing `+=` in
`OnShown()` — the standard unsubscribe-then-subscribe idiom. Removing a delegate that was never
added is a documented .NET no-op, so this is safe on the very first call too, and caps the
subscription count at exactly one regardless of how many times `OnShown()` runs. All reused
operands (the `layeredWindow` field, the `layeredWindow_Click` method reference, the
`EventHandler` ctor) are read back from the existing `add_Click` call's own instructions rather
than assumed, so the new `remove_Click` call is guaranteed to match. Verified before deployment:
decompiles clean (`layeredWindow.Click -= layeredWindow_Click; layeredWindow.Click +=
layeredWindow_Click;`, exactly as intended), the 13-site interpolation regression scan is
unchanged, and `--dump-handlers` confirms `OnShown`'s one exception-handler region (a
`catch(Win32Exception)` well before the insertion point) is untouched. Built as Stage 8, on top of
a freshly-regenerated, byte-identical-to-live stage-7 baseline. Still needs a live-bottle
functional test: confirm a single click now fires `notificationForm_Click` exactly once (via
`--patch-diag` still layered on top, or by observing real behavior — e.g. does clicking now
reliably open exactly one mail item instead of whatever double-`ShowMailForm` was doing before).

**Important scope note:** this is a genuine, real, independently-worth-fixing eM Client bug — not
a Wine gap — but fixing it is not expected to resolve the original empty-box-until-fade rendering
mystery by itself (that remains the unexplained compositor-side gap from rounds 1–5). It's
plausible it explains *some* of the instability found while testing that bug (the hang and the
crash both happened while probing click behavior with `NotificationsHideAfterTimeout` disabled,
a non-default diagnostic setting used specifically to keep the box on-screen long enough to
click), but there's no evidence yet linking it to the blank-rendering symptom itself.

**Deployed and confirmed working (user-tested, live bottle, `--patch-diag` still layered on top
for direct verification):** one notification shown, one click — `layeredWindow_Click`,
`notificationForm_Click`, and `Hide()` each fired **exactly once** (previously: twice each), and
the fade-out proceeded cleanly with no hang or crash, app still running normally afterward. Stage
8 is confirmed fixing the double-dispatch bug it targets. `emClient_win_8_x64`'s `MailClient.dll`
is currently the fix+diag combined build; next step is redeploying the fix alone (without
`--patch-diag`) for normal, non-instrumented use, once the user is ready.

## Eighth round: bitmap-rebuild timing confirmed via instrumentation -- correct content reaches
the window within ~30ms, yet stayed blank; also a genuine crash caused by the diag patch itself

Chased the user's request to pin down exactly what triggers text to become visible, by reading
`LayeredBaseForm.updateLayeredBackground(bool refreshBitmap)`/`updateBackgroundBitmap()` directly.
Found a real caching gate: `updateBackgroundBitmap()` only runs when `refreshBitmap || 
backgroundBitmap == null`, and `FormNotificationPresenter.ShowNotification()` calls `Show()` once
*before* Title/Content are set and once *after* (via `ShowNotification()`'s own `Reshow()`), with
every notification code path passing `refreshBitmap: false` -- a plausible caching bug (second
`Show()` skips the rebuild if a bitmap already exists from the first, blank one).

Extended `--patch-diag` with `updateBackgroundBitmap`/`doLayout` instrumentation to test this
directly. First attempt crashed the app for real, confirmed via the app's own `bug.*.txt` report:
resolving `Rectangle.get_IsEmpty` via `typeof(System.Drawing.Rectangle)` reflection baked in a
`System.Drawing.Primitives, Version=10.0.0.0` reference that doesn't exist alongside the app
(ships its own .NET 8 build) -- a bug in the diagnostic patch itself, not the app or Wine. Fixed by
resolving `Rectangle` from `headerRect`'s own already-correctly-versioned `FieldType` instead (see
CLAUDE.md's IL-patching lesson 5). The crash's stack trace incidentally confirmed
`updateBackgroundBitmap()` really is called from `FormMailNotification`'s constructor.

**Corrected instrumentation, redeployed, one live test (`NotificationsHideAfterTimeout` off):**
- `updateBackgroundBitmap()` ran 3 times during construction (confirms the crash's stack trace);
  the first two early-returned (`headerEmpty=1`, layout not ready), the third succeeded but with
  **blank Title/Content** (not yet set).
- `OnShown` #1 (blank content, `Show()` call #1) then ran; Title/Content were set shortly after.
- `OnShown` #2 (`Show()` call #2, real content already set) ran with `refreshBitmap:false` and a
  bitmap already existing from the third construction-time call -- confirming the predicted
  caching skip actually happens.
- **But ~30ms later**, `updateBackgroundBitmap` ran *again* (forced, `bgWasNull=0` yet it still
  ran, so `refreshBitmap:true` fired from somewhere -- likely `OnResize`'s guarded
  `doLayout(); updateLayeredBackground(refreshBitmap: true);` path, not `setAutoHeight()`'s
  resize-triggered one, since width/height never changed from 310x125 this run), this time with
  the **correct** Title/Content already set. Per `updateLayeredBackground`'s own code, this call
  also pushes the freshly-correct bitmap straight to the real on-screen `layeredWindow` via
  `UpdateWindow(...)`.
- Yet the user watched the box for ~20.8 seconds afterward (`layeredWindow_Click` at the point
  they finally clicked it) and it stayed grey/blank the entire time, **and this particular run the
  text never appeared even after the click** (contrast with every earlier round, where content did
  eventually show, whether via elapsed time or a click) -- a genuinely new negative data point,
  not yet explained, and one the user flagged as inconsistent with the tool's own tick-based
  timing narration, prompting a wall-clock-timestamp addition to the logging (see below) and a
  plan to correlate against an `ffmpeg` screen recording rather than relying on log-only inference.

**Conclusion: this closes off the caching-bug hypothesis as the explanation for the blank period.**
The correct bitmap, with real text, demonstrably reaches the actual visible window within ~30ms of
the notification appearing -- confirmed directly via instrumentation, not inferred. The multi-
second (or, this run, indefinite) blank period is not caused by the app failing to build/push the
right content; it is Wine/the compositor not displaying already-correct content, consistent with
(and now more strongly confirming) the original rounds 1-5 conclusion. There is no
earlier-rebuild fix to reach for here, because the rebuild already happens correctly and early.
The caching gate found in `OnShown` #2 is still a real, independent code smell (a rebuild that
should arguably happen still gets skipped) but is not what's producing the user-visible symptom.

**Tooling improvement made as a result:** `--patch-diag`'s log lines now include a wall-clock
`wall=<HH:mm:ss.fff>` timestamp (via `DateTime.Now`, safe to resolve via `typeof()` reflection
since it's a CoreLib-forwarded type -- unlike `System.Drawing.Rectangle` above) alongside the
existing relative `tick=<TickCount>`, specifically so a log line can be matched directly against
a `ffmpeg` screen recording's real timestamps rather than an arbitrary relative counter that's
easy to mis-narrate.

## Ninth round: frame-precise confirmation, via a wall-clock-correlated recording -- content
appears within ~1-2 frames (33-67ms) of `Hide()` running, after a full ~6s of confirmed-blank

Repeated the test with `NotificationsHideAfterTimeout` back on (default ~6s) and a full-screen
`ffmpeg -f x11grab -framerate 30` recording running throughout. `ffmpeg`'s own reported input
start timestamp (`start: 1788232159.962655`, a Unix epoch value logged by x11grab itself -- far
more reliable than noting the wall-clock via a separate `date` call before launching, which has
startup-lag error) gave an exact frame-0 wall-clock reference, letting every frame number convert
directly to a wall-clock time comparable against the diag log's new `wall=` field.

**First analysis attempt was wrong and the user correctly called it out**: cropped frames showed a
dark rectangle in the lower-right where the notification appears, and it was misidentified as the
Claude Code terminal window occluding the notification (a real confound in an earlier round, see
the fourth round above) -- it was not; it was the notification's own dark-themed header/body
chrome, just genuinely blank of text. Correctly cropping tighter into the actual notification
region (`crop=500:250:1180:900` on the 1680x1188 capture) and re-examining resolved this.

**Frame-by-frame result, precise:**
- Notification's first paint: wall ~15:09:50.5 (log: `OnPaint` at `15:09:50.602`, bitmap with
  correct content already built and pushed to the real window by `15:09:50.592`, ~30ms after
  appearing -- see eighth round above).
- Frames at wall ~50.6s, ~51.6s (frame 950), ~53.3s (frame 1000), ~55.9s (frame 1090), and
  **~56.56s (frame 1098, essentially the exact `Hide()` timestamp of `15:09:56.565`)**: all
  confirmed **fully blank** (header/body chrome only, no avatar, no text) by direct visual
  inspection of the cropped region -- a full ~6 seconds of confirmed-blank display despite the
  correct bitmap having reached the window within 30ms of appearing.
- **Frame 1099** (~33ms after `Hide()`): the avatar icon begins rendering (a partial dark circle,
  top-left) -- **text still not present**.
- **Frame 1100** (~67ms after `Hide()`): full text ("Test Sender" / "Re: Testing notifications")
  and the reply/flag/delete action icons are all visible.

This is the tightest, most direct confirmation yet of the round-5 theory (`Hide()` running is the
trigger, not a fixed wall-clock threshold) -- previously inferred from ~1s-resolution screen
recordings and indirect timing correlation, now pinned to a ~33-67ms window immediately following
the actual `Hide()` call, with a wall-clock-synchronized log line as ground truth for exactly when
`Hide()` ran. It also independently confirms, at real resolution for the first time, the user's
earlier informal observation that the avatar renders one frame before the text -- round four's
30fps analysis of a different test couldn't resolve this (avatar and text appeared in the same
captured frame there); this run's 30fps recording happened to land a frame exactly in the ~33ms
gap between the two.

**This also means the eighth round's framing ("closes off the caching-bug hypothesis... not what's
producing the user-visible symptom") undersold how solid the underlying compositor-freeze evidence
actually is** -- it doesn't just "remain consistent with" rounds 1-5's conclusion, this round
directly, frame-precisely reconfirms it: correct content sitting ready in the app/window for a
full ~6 seconds with nothing on screen, then appearing within two frames of `Hide()`, is about as
clean a demonstration of "the app did the work, the compositor didn't show it until told to
transition away" as this investigation has produced. The caching gate found in `OnShown` #2 is
still real as an independent code smell, worth fixing on its own merits, but is confirmed here to
have no bearing on the blank-period symptom.

## Not yet tried

- A live X11 pixmap dump (e.g. `xwd`/`import`) precisely synchronized with the broken window's
  timeframe, to inspect the actual server-side composited pixmap content directly, rather than
  inferring from Wine's own trace log of what it *believes* it did.
- Reading Wine's and Muffin's source did narrow things down (see above — Wine's role ends at
  setting `_NET_WM_WINDOW_OPACITY`; Muffin's X11-Sync-extension freeze mechanism is ruled out) but
  didn't find the actual mechanism. The remaining unread territory: Muffin's damage-tracking and
  texture-upload code specifically (`meta-surface-actor-x11.c` and the Cogl/Clutter texture-update
  path it feeds into) for anything else that could cause a window's displayed texture to go stale
  independent of the Sync-extension freeze path already ruled out.
- A minimal repro app was built and iterated five times without reproducing the bug (see above) —
  worth a sixth iteration simulating genuine background CPU/thread load (the one structural
  difference from the real app not yet tried) if this gets picked up again, though the diminishing
  hit-rate of the last five attempts makes this a lower-confidence next step than the Muffin
  source options above.
- Testing under a different desktop environment/window manager (non-Cinnamon) to determine
  whether this reproduces identically elsewhere, or is specific to this Mint/Muffin combination —
  strengthened as a lead now that Wayland is a plausible clean escape (see above), since Wayland
  and a different X11 WM/compositor (e.g. KWin, Picom+a non-compositing WM) are two different,
  separately informative tests.
- Testing against a different CrossOver/Wine build to bound whether it's version-specific.
- ~~The user separately observed the notification's sender avatar/icon appearing to draw before
  the text~~ — **confirmed in the ninth round above**: a wall-clock-synchronized 30fps recording
  caught the avatar rendering one frame (~33ms) before the text, both within ~67ms of `Hide()`
  running. Suggests the icon (a `DrawImage`/bitmap blit) and the text (`ExtTextOutW`) may take
  different paths to the screen, or simply that the compositor's "catch up" isn't a single atomic
  event — not yet investigated further.

## Status

**Unresolved as a root cause, but now characterized with frame-level precision, as of this session
(2026-09-01, second sitting).** The prior sitting's "freshest lead" (clicking a no-text
notification after ~6s hangs the app) was chased down and **fully resolved** -- not by finding a
race in this bug's own mechanism, but by finding and fixing a real, separate, independently-
confirmed bug: `FormGenericNotification.OnShown()` re-subscribes `layeredWindow.Click` on every
call with no matching unsubscribe, and runs at least twice per notification shown, so a single
click double-fired the entire click-handling chain (`layeredWindow_Click` →
`notificationForm_Click` → `PerformAction` → `ShowMailForm`). Fixed via
`--patch-notification-click-resubscribe` (Stage 8: `layeredWindow.Click -= layeredWindow_Click;`
before the existing `+=`), user-confirmed on a live bottle (one click now fires the chain exactly
once, no hang, no crash). **Not yet folded into `releases/<version>/deploy.sh`** -- apply manually
via `~/tools/il-patcher --patch-notification-click-resubscribe` on top of Stage 7's output until
it is. This bug is real and worth having fixed regardless, but is now confirmed **unrelated** to
the empty-box rendering bug itself (see below) -- it only ever affected click *handling*, not the
blank-display symptom.

**The empty-box rendering bug's root cause is still not found**, but this session's ninth round
(a wall-clock-synchronized `ffmpeg` recording, made possible by adding `wall=<HH:mm:ss.fff>`
timestamps to `--patch-diag`'s log output) sharpened the evidence considerably: the correct
bitmap, with real text, is confirmed built and pushed to the actual on-screen window within ~30ms
of the notification appearing (via direct `updateBackgroundBitmap`/`updateLayeredBackground`
instrumentation) -- yet the screen stayed confirmed-blank (verified by inspecting the actual pixel
region in extracted video frames, not inferred) for a full ~6 seconds, until content appeared
within **~33-67ms of `Hide()` running** (avatar one frame before text). This is the tightest,
most direct confirmation yet of "the app did the work correctly and early; the compositor didn't
reflect it until `Hide()`'s own machinery ran" -- previously inferred at ~1s screen-recording
resolution, now pinned to two video frames with a synchronized log line as ground truth. A
caching bug that looked like it might explain this (`OnShown` #2 skipping a bitmap rebuild because
one already exists from `OnShown` #1's blank pre-content call) was found to be real as a code
pattern but **directly ruled out** as the cause of the blank period, precisely because the correct
bitmap already reaches the window so early.

## Tenth round: the Timer re-arm alone unsticks the avatar (not the text) -- the icon and text
genuinely take different paths to the screen, confirmed directly

Built `--patch-notification-timer-kick` to isolate the Timer re-arm from everything else `Hide()`
does. It forces the Visible-hold timer to fire quickly (2s instead of the real `timeToStay`) via
an override write right after `OnShown`'s own `timer.Interval = timeToStay;`; the first time
`timer_OnTimer`'s Visible-branch would normally call `Hide()`, it's redirected (by swapping just
that one call instruction's operand, not restructuring control flow) to a new
`__diagKickOrHide()` method that, on its first invocation, *only* re-arms the timer (`timer.
Interval = timeToStay; timer.Start();` -- the same Win32-level `SetTimer` re-arm `Hide()` itself
does) and returns -- no state change, no `alphaIncrement`, no `Opacity` change, no `Invalidate()`.
The second invocation (the real `timeToStay` later) calls `Hide()` as normal.

Two recording attempts were lost to process/coordination issues before a clean one landed: an
`ffmpeg` file corrupted on stop (external `timeout` killing the process before the mp4 trailer/
moov atom could be written -- fixed by using `-t <duration>` as ffmpeg's own flag instead of an
external `timeout` wrapper, which finalizes the container properly), and one recording that
completed before the user had a chance to trigger the notification (needed a longer window and
tighter go/trigger coordination).

**Result, frame-precise, from the successful recording:** the kick tick was logged at
`15:39:19.578`. Frame 1067 (wall ~15:39:19.63) is still fully blank. **Frames 1068-1069 (wall
~15:39:19.65-19.69, ~70-110ms after the kick) show the avatar/icon rendering** -- with no `Hide()`
call, no state change, and no opacity change anywhere in the code path that produced it. The
recording continued for another ~4.5 seconds (up to frame 1199, wall ~15:39:24.05, still ~1.5s
before the real `Hide()` at `15:39:25.582`) and **the text never appeared in that window** --
still just the avatar, confirmed by direct visual inspection of the cropped notification region
across multiple frames. The user's own live observation matched exactly: "a very slight change in
opacity just a frame or so before the image, then text appeared [in the previous, unmodified
round]... this time the image appeared much earlier."

**This is decisive: the Timer re-arm alone is sufficient to unstick the avatar/icon, but not the
text.** It directly confirms the "Not yet tried" item from earlier in this document (the avatar
and text may take different paths to the screen) -- no longer a guess, now observed directly at
frame level with a controlled experiment that separates the two. The remaining difference between
the kick (avatar unsticks, text doesn't) and a real `Hide()` (both unstick) is state -> 
`Disappearing`, `alphaIncrement` becoming non-zero, and -- notably -- the fade-out ticks that
follow real `Hide()` each call `Invalidate()` explicitly (`timer_OnTimer`'s opacity-stepping
branch: `base.Opacity += alphaIncrement; updateLayeredBackground(refreshBitmap: false);
Invalidate();`), which nothing in the idle Visible-hold or the kick path ever does. `Invalidate()`
forcing a genuine `WM_PAINT` dispatch on the main form is the strongest remaining candidate for
what specifically unsticks *text*, since the avatar (blitted directly into the bitmap during
`updateBackgroundBitmap()`, independent of `WM_PAINT`) apparently doesn't need it.

**Follow-up test, same session: `Invalidate()` alone does not unstick text either.**
`--patch-notification-timer-kick-invalidate` added `this.Invalidate();` to the kick branch
(alongside the same timer re-arm), testing whether forcing a genuine `WM_PAINT` dispatch --
something every real fade tick does on each 25ms step but the idle Visible-hold never does -- was
the text-specific trigger. A clean, wall-clock-synced recording (kick logged at `15:53:55.840`,
real `Hide()` at `15:54:01.845`) showed: avatar renders again right around the kick (frame ~552,
consistent with the earlier result), and **text still does not appear at any point up to frame
720 (wall ~15:54:01.452, ~0.4s before the real `Hide()`)** -- confirmed by inspecting the cropped
notification region across many frames spanning the full ~6s gap. Text renders as usual within a
few frames of the real `Hide()` (frame 735, wall ~15:54:01.9). **This rules out `Invalidate()`/
`WM_PAINT` dispatch as the text-specific trigger** -- it isn't merely a missing repaint request;
whatever unsticks text specifically requires something else `Hide()`'s Visible→Disappearing
branch does that the kick (even now with `Invalidate()` added) still doesn't: the `state` field
actually changing to `Disappearing`, and/or `alphaIncrement` becoming non-zero (which then drives
`Opacity` down on each subsequent real fade tick).

**Follow-up test, same session: transiently flipping `state` to `Disappearing` and straight back
does not unstick text either -- but confirms the avatar's timing is directly controllable.**
`--patch-notification-timer-kick-state-flip` added `state = Disappearing; state = Visible;` to
the kick branch (no real fade, `Opacity` never moves) and, prompted by the user's question about
whether the avatar could render close to immediately rather than after an arbitrary wait, also
reduced the kick delay from 2000ms to 100ms. Wall-clock-synced recording result: the kick fired
at `18:26:53.545` (~94ms after the notification appeared at `18:26:53.451`), and the **avatar
rendered by frame 358 -- only ~28ms after the kick itself**, i.e. effectively within ~120ms of the
notification appearing at all. This confirms the ~70-110ms post-kick avatar latency seen in
earlier rounds is roughly constant regardless of when the kick fires -- moving the kick earlier
really does make the avatar appear correspondingly earlier, a real and usable fix lever for the
avatar specifically (fire the same re-arm immediately after the notification's real content is
set, instead of waiting on the natural 6s timeout or a click). **Text, however, still did not
appear** at any point checked up to frame 530 (wall ~18:26:59.31, just before the real `Hide()` at
`18:26:59.554`) -- confirmed by inspecting the cropped notification region across the full ~6s
gap. Text rendered normally at frame 545, right after the real `Hide()`, confirming the pipeline
itself worked correctly and this was a genuine negative result, not a test artifact.

**Status after three isolation attempts (plain re-arm, re-arm+`Invalidate()`, re-arm+transient
state-flip): the avatar's unstick trigger is now well understood and directly exploitable (the
Timer re-arm, fireable as early as desired); the text's unstick trigger remains unidentified.**
None of `Invalidate()`, a transient `state` write, nor the timer re-arm itself explain it in
isolation. Remaining candidates not yet tried: `alphaIncrement` actually holding a non-zero value
across at least one real timer tick (as opposed to being written and immediately made moot by
`state` flipping back before any tick observes it); or the combination of state+alphaIncrement
+timer-interval-25ms all together but stopped after one tick before any real fade completes (i.e.
a "one real fade tick, then abort" test, closer to what `Hide()` actually does but truncated
before the box visibly disappears). Given the depth already reached, the user's earlier-noted
alternative strategy -- rendering text through a normal, non-layered `WM_PAINT` path instead of
the cached layered-window bitmap, sidestepping the mystery for text specifically -- is worth
weighing against continuing to chase the exact native trigger.

**Eleventh round: text DOES render during a genuine active fade -- and reverts the instant the box
returns to a static Opacity=1.0/Visible state. The clearest, most specific signal yet.**
`--patch-notification-real-fade-abort` set `state = Disappearing` and `alphaIncrement = -0.05f`
exactly as `Hide()`'s Visible branch does, re-armed the timer to 25ms, then pumped the message
loop (`Application.DoEvents()` + `Thread.Sleep(30)`, the same proven-safe technique from the
third fix attempt's dip-and-recover) for ~120ms so several *genuine* `WM_TIMER`-dispatched ticks
ran through `timer_OnTimer`'s own **unmodified** Disappearing-branch code (real `Opacity`
decrement, real `updateLayeredBackground(false)`, real `Invalidate()` on each tick) -- then
aborted: `Opacity = 1.0; state = Visible; alphaIncrement = 0f;` plus one more
`updateLayeredBackground(false); Invalidate();` and a normal timer re-arm for the real remaining
`timeToStay`.

Frame-precise result: **frames 375-377 (wall ~18:42:00.05-00.11, spanning the real pumped ticks
through the abort) show full text and the action icons, correctly rendered.** Frame 378 (~35ms
later, right after `Opacity` was forced back to exactly `1.0` and `state` back to `Visible` in
the same call) is **blank again** -- reverted to the same stuck avatar-only state as before the
kick ever ran. This is a materially different, sharper signal than every prior isolation attempt:
text isn't simply "never shown without `Hide()` completing" -- it renders correctly while a real
fade is actively in progress, and stops the instant the box returns to steady Visible state at
full opacity.

This reopens a specific mechanism round 9 found in Wine's own source and had set aside:
`X11DRV_SetLayeredWindowAttributes` (`dlls/winex11.drv/window.c`) takes a genuinely different
path depending on whether the resolved alpha is *exactly* 255 (`Opacity == 1.0`) --
`XChangeProperty` for any other value, `XDeleteProperty` specifically at exactly 1.0. Round 9
ruled this split out as the cause of the *original* blank period (a dip-and-recover sequence that
deliberately avoided ever landing on exactly 1.0 mid-sequence still produced zero effect back
then) -- but this round's abort forced Opacity to exactly 1.0 in the very same step as flipping
`state` back to `Visible`, reproducing that specific delete-the-property transition right at the
moment blanking occurred. Round 9's ruling-out and this round's finding aren't necessarily in
conflict -- they tested different transitions (avoiding 1.0 during the *original* appear-and-hold
sequence, vs. deliberately returning *to* 1.0 after a period of real activity) -- but the overlap
is suspicious enough to warrant a direct, targeted test.

**Follow-up test, same session: landing at 0.999 instead of exactly 1.0 does not keep text
visible either -- the `XChangeProperty`/`XDeleteProperty` split is ruled out, `state` itself (or
something gated on it) is the better remaining candidate.** `--patch-notification-real-fade-abort-0999`
ran the identical real-pumped-fade-then-abort sequence, landing `Opacity` at `0.999` (still on
Wine's `XChangeProperty` path per its source, never triggering `XDeleteProperty`) while still
flipping `state` back to `Visible`. User-confirmed live result: **the same pattern as the exact-
1.0 test** -- text appeared during the pumped real fade, then disappeared again. This rules out
the specific `XChangeProperty`-vs-`XDeleteProperty` transition as the cause: text reverted
regardless of which Wine code path the final opacity write took, so `state` returning to
`Visible` (or something else common to both tests, e.g. `alphaIncrement` returning to exactly
`0`) is the better remaining explanation, not the opacity-property mechanism specifically.

**A real, separate bug surfaced during this test, worth fixing regardless of the fade
investigation:** landing at `Opacity = 0.999` with `alphaIncrement` reset to `0` breaks
`timer_OnTimer`'s own state machine. Its first branch is `if (Opacity + alphaIncrement >= 1.0)`;
with `Opacity = 0.999` and `alphaIncrement = 0`, this evaluates to `false` forever (`0.999 >=
1.0` is false, and `Opacity += alphaIncrement` never changes it since `alphaIncrement == 0`), so
every subsequent ~6s timer tick falls into the harmless-looking `else if (IsHandleCreated) {
Opacity += alphaIncrement; updateLayeredBackground(refreshBitmap: false); Invalidate(); }` branch
forever -- the notification never reaches `Hide()` again and sits, fully visible, looping
indefinitely (confirmed via the diag log: `state=2` repeating every ~6000ms with no further
`Hide()` line, matching the user's live report of the notification "still visible" and appearing
stuck). Not a Wine bug -- an artifact of this test patch choosing an abort opacity that doesn't
satisfy the real code's own `>= 1.0` re-entry condition. The user resolved it by terminating and
restarting eM Client (accepting the expected DB-repair-dialog consequence of a non-graceful
termination, per this project's established behavior). Any future variant of this test should
land the abort opacity at exactly `1.0` (satisfies `>= 1.0` cleanly) if avoiding this loop matters,
or explicitly research the `>= 1.0` condition's edge behavior first.

**Where this leaves the investigation:** the avatar's trigger is solved and directly exploitable
(fire the timer re-arm immediately instead of waiting). The text's trigger has now survived four
isolation attempts (plain re-arm, +`Invalidate()`, +transient state-flip, +real pumped fade
landing at two different opacity values) -- it reliably renders *during* an active real fade and
reliably reverts once `state` returns to `Visible`, but no single mechanism tried explains why.
Remaining candidates: `state` itself gating something not yet found in the code (worth a direct
search for any other code path that reads `state == Visible` specifically, beyond the ones
already read in this investigation); or `alphaIncrement` needing to remain non-zero, not just
`state` needing to stay `Disappearing`, which the transient state-flip test (round ten) didn't
distinguish from this round's (`alphaIncrement` was also reset to `0` in every abort tried so
far, including the state-flip-only one which set it to `0` as its "no-op" default rather than
leaving it at whatever the initial construction value was). A test that flips `state` back to
`Visible` while leaving `alphaIncrement` at its small non-zero fade value would separate these
two remaining candidates cleanly.

## Twelfth round: `alphaIncrement`, not `state`, is the gate -- text survives `state == Visible`
for 100ms+ as long as ticks keep running

Directly prompted by the user's question about whether the notification could just be held in
"whatever state it's in right before text hides" for the whole display duration, rather than
chasing the exact Wine mechanism further. Before building the test, walked through the risk with
the user: `OnMouseEnter` has `if (reShowOnMouseOver && ... && state == Disappearing) { Show(); }`,
and `Hide()`'s own switch treats `Disappearing` as a no-op case -- so holding `state ==
Disappearing` for the whole hold would risk a hover-triggered re-fade and break click-to-dismiss.
Holding `state == Visible` instead (the normal, side-effect-free value) would avoid both, *if*
`alphaIncrement` turns out to be the real gate rather than `state` itself. The user also separately
noted the animation itself isn't sacred -- a "just show it at full opacity" version with no visible
fade would be perfectly acceptable if that's what ends up working.

`--patch-notification-state-vs-alpha` ran the same real-pumped-fade opening as round eleven
(state=Disappearing, alphaIncrement=-0.05f, ~4 genuine ticks), then -- unlike every prior abort,
which reset `state` and `alphaIncrement` together -- flipped `state` back to `Visible` while
*deliberately leaving* `alphaIncrement` at a small non-zero value (`-0.01f`) for a second pumped
window of ~3 more genuine ticks, only doing the real cleanup (`alphaIncrement = 0`, `Opacity =
1.0` exactly) at the very end.

**Reconstructing the precise timeline from the diag log's wall-clock ticks (not just frame
counting, which is too coarse at this resolution) gives an unambiguous result:** `state` flipped
back to `Visible` at wall `19:15:58.224`, starting phase B's three real ticks (through `58.300`).
**Frame 124 (wall `58.338`, ~114ms after `state` became `Visible`) still shows full text** --
confirmed by direct pixel inspection, well past the ~33-70ms compositor-response lag seen
everywhere else in this investigation, ruling out "just hasn't caught up yet" as an explanation.
Cleanup (`alphaIncrement -> 0` exactly) ran immediately after phase B ended (~`58.300`-`58.305`).
**Frame 126 (wall `58.405`, ~100ms after cleanup) is blank again.** Text survived `state ==
Visible` for over 100ms as long as `alphaIncrement` stayed non-zero and real ticks kept running,
and only reverted once `alphaIncrement` actually reached exactly `0`.

**This is the clearest, most actionable finding of the investigation: `alphaIncrement` remaining
non-zero (i.e. real ticks actively running, whatever their sign or magnitude) is the gate --
`state` itself is not.** This directly answers the user's strategic question: a fix can safely
hold `state == Visible` (avoiding the `OnMouseEnter`/`Hide()` side-effect risks entirely) while
keeping a tiny persistent non-zero `alphaIncrement` for the whole display hold, periodically
nudging `Opacity` back so it doesn't visibly drift toward 0 over the ~6s (or however long
`NotificationsHideTimeout` is configured) -- rather than needing to hold the riskier
`Disappearing` state, and without needing to identify the exact underlying Wine/compositor
mechanism at all. Given the user's explicit openness to a no-visible-animation version, the
simplest concrete fix shape worth prototyping next: keep the timer ticking at its normal 25ms
cadence for the *entire* display hold (not just during Appearing/Disappearing) with a minuscule
non-zero `alphaIncrement` that gets corrected back toward 1.0 every tick (net-zero drift, but
never letting the value or the ticking itself go static), replacing the current design of a long,
inert `timeToStay`-interval wait.

Coordination notes for future rounds: (1) a screen-recording test needs the recording started
*after* the user confirms the test email is sent, not before -- mail delivery can take up to a
minute, and several attempts this round were wasted on fixed-duration recordings that expired
before the notification even arrived, or before the specific timed event being measured; cap
recording duration at 60s. (2) When waiting on `ffmpeg` to finish, poll directly (`ps`/`ffprobe`)
rather than a blocking wait-loop command, which can get silently backgrounded by the harness and
stall the turn.

`emClient_win_8_x64` currently has the timer-kick-state-flip test build (Stage 8 fix + full
`--patch-diag` instrumentation + the state-flip experiment, 100ms kick delay) deployed, from this
session's live testing. Reverting to a clean Stage 8 build (fix only, no diagnostic/experimental
logic) for normal use is a pending task, not yet done as of this writing.

## New test infrastructure: `--patch-auto-test-notification`

To avoid needing the user to send a real test email for every iteration, added a self-triggering
test notification (`RunPatchAutoTestNotification`, `il-patcher-Program.cs`): a one-shot
`System.Windows.Forms.Timer` inserted into `formMain.OnShown`, firing 3000ms after the main window
appears, that constructs a `FormMailNotification` with fixed `Title`/`Content` strings, sets
`Location` explicitly, and calls `FormGenericNotification`'s own `Show(IWin32Window)` override
(not the plain inherited `Control.Show()`, which skips the presenter's positioning and the
override's own topmost `SetWindowPos`/`ShowWindow` calls -- caught via a first live test where the
notification rendered at the default `(0,0)`, mostly hidden behind the main window). Combined with
launching eM Client and `ffmpeg` together from the same command (rather than waiting for the main
window to be detected first, which reliably arrived too late -- see next section), this makes the
whole test loop fully self-contained: no email, no waiting on the user to click or confirm
anything.

## Thirteenth round: v6 tested live via the auto-trigger -- no improvement; found *why*

First attempt at using the new auto-trigger started screen recording only after detecting the main
window via `wmctrl`, on the assumption that meant the notification hadn't fired yet. Wrong: the
diag log showed the entire notification lifecycle (`OnShown` through `Hide()`, ~7s) had already
completed *before* the detection loop even found the window, because by the time `wmctrl` reported
the window as mapped, eM Client had already been running long enough for the 3s auto-trigger delay
and the full notification hold to elapse. Fixed by starting `ffmpeg` and launching `MailClient.exe`
in the same command, both timestamped, so the recording covers the entire process lifetime from
before the main window even exists.

Re-run this way (recording from `21:00:28.87`, notification `OnShown` at `21:00:38.852`, `Hide()`
at `21:00:45.355`) reproduced the **exact original symptom** against the `output-keep-alive-v6`
build (monotonic `-0.01f`/tick fade with a `0.5` opacity floor, from the twelfth round's finding):
text visible in the frame immediately after `OnShown` (frame at ~10.0s), blank again within
~800ms (frame at ~10.8s), and not visible again until the frame coinciding with `Hide()`'s fade-out
(~16.4s) -- no different from the unpatched baseline. Zero improvement, exactly the case the user
asked to be probed about ("remember to probe me to switch if we make no improvement").

**Found the specific reason v6 didn't work, from the diag log's raw tick trace (not just frames):**
`timer_OnTimer` fired continuously (~25ms apart) during `Appearing` (`38.901`-`39.351`), then
**zero ticks of any kind for 6.003 seconds** -- no `timer_OnTimer`, no `OnPaint`, nothing -- until a
single `timer_OnTimer` at `45.354`, immediately followed by `Hide()` at `45.355`. That 6.003s gap
is `timeToStay` (the real configured hold duration) almost to the millisecond, not the 25ms
sustain interval v6 was supposed to keep running. Decompiling the deployed `OnShown` explains why:
the `Visible`/`Appearing` case sets `timer.Interval = timeToStay;` **first**, then (for `autoHide`)
calls `timer.Start()`, then only *afterward* sets `timer.Interval = 25;` and calls `timer.Start()`
again for the brief 4-cycle `DoEvents()+Sleep(30)` unlock pump, before resetting `alphaIncrement`
to the monotonic `-0.01f` value and returning -- with no further `Interval`/`Start()` call once the
pump ends. `DoEvents()` forces the queued `WM_TIMER` messages to be processed immediately
regardless of the programmed native interval, which is why the 4-cycle unlock pump still produced
real ticks -- but once execution returns to the normal message loop, the timer's *actual* native
re-arm cadence going forward is governed by whichever `Interval` value the underlying platform
timer was last armed with outside of a forced pump, which this evidence says was effectively still
`timeToStay`, not `25`. In other words: v6's sustain mechanism only ever ran during the four
synchronous, forced `DoEvents()` cycles (~120ms) immediately after `OnShown`, not for the rest of
the hold -- so it stopped mattering almost immediately, and the notification silently reverted to
the original inert `timeToStay`-interval wait for the remaining ~5.9s, reproducing the original bug
exactly.

**This closes out the "tune the Timer-driven sustain-tick" family of fixes (v1 through v6, six
iterations).** Every version that got a real per-tick native alpha change running did keep text
visible for as long as ticks were actually occurring (twelfth round's finding stands: it's
genuinely `alphaIncrement`/real ticking, not `state`, that gates visibility) -- but no version has
managed to keep the *timer itself* reliably ticking for the entire ~6s hold once it's not being
forced by a synchronous `DoEvents()` pump, and synchronously pumping for the *entire* hold (as
opposed to a brief unlock window) was already tried and rejected earlier in the investigation as
freezing the rest of the UI. Next step flagged to the user: pivot away from trying to keep the
existing layered-window/cached-bitmap timer mechanism ticking, toward the alternative discussed
earlier -- rendering title/content text via a normal, non-layered child control (a `Label` or
owner-drawn control sitting on top of the layered background) that Wine's compositor would paint
through its own independent, non-timer-gated path, rather than through the `alphaIncrement`-gated
cached-bitmap blit that every fix attempt so far has been routed through.

## Fourteenth round: the "separate rendering path" pivot (option 1) -- two fixes tried, neither works

Per the thirteenth round's finding and the user's explicit choice ("option 1" over both a
lighter-weight Timer tweak and reviving the fade entirely), pivoted away from tuning the Timer
sustain mechanism toward rerouting *what draws the text* instead.

**Fix attempt 1 -- `--patch-notification-text-in-bitmap`:** re-read `LayeredBaseForm`'s decompiled
source and found `FormGenericNotification` ("this") and its `layeredWindow` companion (a separate
`LayeredForm` window, declared on `LayeredBaseForm`) are two distinct windows. `layeredWindow`
receives `backgroundBitmap` -- confirmed by reading `updateBackgroundBitmap()`'s full body to
contain only background gradient/border/shadow/avatar, never text -- via a genuine per-pixel
`UpdateWindow()` blit forced fresh on every call, which is why box/border/avatar have been reliably
visible from frame one in every recording this whole investigation. Title/content text is drawn by
`OnPaintTitle()`/`OnPaintContent()`, and at the time this fix was written, the only known call site
for those two methods appeared to be `this`'s own paint pipeline -- so the fix added a new
`__drawNotificationTextIntoBitmap()` helper, called at the end of `updateBackgroundBitmap()`, that
invokes `OnPaintTitle`/`OnPaintContent` (via `Callvirt`, honoring `FormIMMessageNotification`'s
override) against a fresh `Graphics` on `backgroundBitmap` itself -- so text would ride along on
the same reliable blit as the background. Required retargeting one `leave` instruction and one
`finally` handler's `HandlerEnd`, both of which pointed at `updateBackgroundBitmap`'s original final
`ret` (IL-patching lessons 2 and 3), since the new call needed to run after the method's existing
`using` block completed, not get silently absorbed into it. Decompiled clean, `--dump-handlers`
confirmed correct nesting, interpolation-site scan unchanged (13). **Live-tested via the new fully
automated launch+`ffmpeg` loop (see below): no visible difference at all.** Text still appeared
only in the frame right after `OnShown` and the frame right at `Hide()`'s fade-out, blank for the
whole ~6.5s hold in between -- identical to baseline, despite text being verifiably baked into
`backgroundBitmap` (confirmed via decompile).

**Root cause of *that* non-result:** grepping the assembly's raw IL for actual
`OnPaintTitle`/`OnPaintContent` **call sites** (not just their declarations) found something the
earlier decompile-based reading had missed: they're invoked from `FormGenericNotification`'s *own*
`OnPaintBackground` override -- previously misread as design-mode-only, because a *different*,
base-class `OnPaintBackground` on `LayeredBaseForm` really is design-mode-only, and that was the
one actually decompiled and read in an earlier round. `FormGenericNotification`'s own override
calls the base method first, then -- at real runtime, not design mode -- draws its *own* redundant
copy of `backgroundBitmap` directly onto `this` form's device context, immediately followed by
`OnPaintTitle(e)`/`OnPaintContent(e)` painting text onto that same DC. So background/avatar/border
are actually painted **twice** per frame: once via `layeredWindow`'s reliable forced blit, and again
via `this`'s own `OnPaintBackground`, which -- per this whole investigation's central, repeatedly-
confirmed finding -- is itself subject to the SetLayeredWindowAttributes-only-updates-on-a-real-tick
compositing bug. Working theory: `this`'s own copy gets stuck showing a partially-flushed frame
(background painted, text not yet reached) sitting fully opaque on top of `layeredWindow`'s
already-correct, text-included content underneath, blocking it from view for the whole hold.

**Fix attempt 2 -- `--patch-notification-suppress-self-paint`:** since `layeredWindow` alone (with
fix 1 applied) should already provide background, avatar, border, and text, made `this`'s own
redundant `OnPaintBackground` drawing a no-op at real runtime, so there's nothing of `this`'s own
left to get stuck mid-composite -- a single `brfalse`->`br` opcode flip on the existing
`if (backgroundBitmap != null) { ... }` guard, same operand/target, no new instructions, no
branch-target or handler-boundary retargeting needed at all. Decompiled clean (confirmed the whole
block became `//Discarded unreachable code`), `--dump-handlers` and the interpolation scan both
still clean.

**Live-tested (fixes 1+2 together): still no improvement, plus a new artifact.** First attempt at
reading this test's recording produced a genuinely wrong conclusion worth recording as its own
lesson: the test notification's `Location` (bottom-right, `(1360, 1000)`) turned out to coincide
almost exactly with where this terminal's own inline image-preview widget renders (whatever
screenshot was last opened via the `Read` tool, complete with its own prev/next/reply/delete
icons) -- several minutes were spent reading that widget's stale content as if it were the
notification before this was caught. Moved the auto-trigger's `Location` to `(100, 100)` (top-left)
to eliminate the ambiguity, confirming the fix was general (not dependent on which screen corner)
by re-running the automated launch+recording loop again. With the unambiguous position: the
notification icon area now renders the classic Windows/GDI+ "broken/missing image" glyph (a gray
box with a red diagonal cross), a symptom **not present** in fix 1 alone or in any earlier round
-- new, and specific to fix 2. Root cause not yet investigated (a leading theory: something reads
`backgroundBitmap` on `this`'s side, or a disposed/mid-rebuild reference, that fix 2's removal of
the redundant draw call newly exposes rather than causes -- not confirmed). Given this is a
regression, not merely a non-improvement, both fixes were left in place in git (their code is
correct and clean by every static check) but the live bottle was reverted to the confirmed-working
Stage 8 build (verified via `md5sum` match against `il-patches/output-stage8/MailClient.dll`) rather
than left on this experimental state.

**Net result of the "separate rendering path" pivot: negative on both attempts.** Combined with the
Timer-sustain family's six failed iterations, eight total fix attempts across two structurally
different strategies have now failed to produce a readable result. Given the standing instruction
to probe the user when an iteration makes no improvement, and this round produced a new regression
on top of no improvement, this is a natural point to stop iterating solo and get the user's explicit
direction on whether to keep pursuing "separate rendering path" variants (e.g., figuring out the
broken-image glyph, or trying a genuinely new WinForms child control instead of routing through
`OnPaintTitle`/`OnPaintContent` at all) or pivot to a different strategy entirely.

## Fifteenth round: a full decompile pass finds a real ordering bug -- partial, measurable improvement, still not a full fix

Prompted directly by the user pushing back on reactive, hypothesis-then-patch investigation
("have you decompiled all the code involved in notifications and have a full picture... your
implementation can look the same but take a different approach") -- a fair challenge, since the
fourteenth round's `OnPaintBackground` surprise was exactly a case of an incomplete picture causing
a wasted cycle. Ran a full, systematic read-only decompile pass (forked out to keep the raw dump
volume out of the main session) across every type in the notification pipeline:
`FormGenericNotification`, `LayeredBaseForm`, `LayeredForm` (the `layeredWindow` companion, not
previously read at all), `FormMailNotification`, `FormIMMessageNotification`,
`MailNotificationHandler`, `FormNotificationPresenter`.

**The finding:** `FormNotificationPresenter.ShowNotification()` -- the real trigger for an incoming
mail -- calls `formGenericNotification.Show()` **first** (while the form is still blank), and only
*then* calls `formGenericNotification.ShowNotification(notification)`, which sets Title/Content/
Image via virtual dispatch into `FormMailNotification.OnDisplayedNotificationChanged`. Every fix
attempt in this investigation's auto-test trigger up to this point did the *opposite* -- set Title/
Content directly, then call `Show()` -- a real, consequential difference the user's question
anticipated exactly, not a cosmetic one. Confirmed by decompile: neither the `Title` setter
(`Text = (title = value);`) nor the `Content` setter (a no-op in practice, since
`FormMailNotification` sets `AutoHeight = false`) has any repaint side effect; the `Image` setter's
only effect (`updateBitmapPending = true`) is read solely by `this` form's own (Wine-unreliable)
`OnPaintBackground`. So on the real path, nothing ever forces a fresh rebuild-and-blit through
`layeredWindow` -- confirmed by finally reading `LayeredForm.UpdateWindow` directly (not just
inferred) to be a genuine, always-forced native `UpdateLayeredWindow` P/Invoke call, i.e. the
reliable half of this whole investigation's story really is reliable; the gap was entirely
upstream of it.

**Fix -- `--patch-notification-refresh-on-content-change`:** insert
`updateLayeredBackground(refreshBitmap: true)` immediately after `ShowNotification`'s
`OnDisplayedNotificationChanged(EventArgs.Empty)` call. Straight-line method, no branches or
handlers to worry about. Also fixed `--patch-auto-test-notification`, which previously both got the
order wrong *and* bypassed `ShowNotification()` entirely (setting Title/Content directly) -- meaning
it could never have exercised this fix. Rewrote it to call `Show()` first, then
`ShowNotification(new Notification(new NewMailsCount { Value = 3 }))` (the `NewMailsCount` branch of
`OnDisplayedNotificationChanged`, simpler to construct via raw IL than a full fake `IMail`), so the
synthetic trigger now genuinely exercises the same virtual-dispatch path a real notification uses.

**Live-tested (fix alone, on top of clean Stage 8, no other experimental patches): a real, visible
change -- but only a partial one.** Frame-by-frame review (correct crop confirmed this time via
`wmctrl -l -G` mid-flight rather than assumed, after two rounds of resolution changes -- see
environment note below) showed the icon **and title/content text together**, clearly readable, for
roughly the first second after `OnShown` (during the `Appearing` fade-in tick burst) -- the first
time in this entire investigation that text has been observed on-screen at the *start* of a
notification's life rather than only at the very end. It then goes blank again once the hold begins
(state settles to `Visible`, ticking stops) -- icon persists (as it always has, via `layeredWindow`),
text vanishes. A diag-log trace confirmed the mechanism is doing what was intended: the bitmap
rebuild (`bgWasNull=0`, i.e. a real rebuild of an existing bitmap, not the initial blank one) landed
*before* the `Appearing` tick burst even started, meaning every one of those ~18 ticks re-blitted the
already-correct, text-included bitmap via a genuine native call as `Opacity` climbed from 0.05 to
1.0 -- and text WAS visible throughout that burst, confirming the twelfth round's "real ticking is
the gate" finding still holds. But once ticking stops for the ~6s hold (same silent-gap pattern as
every prior round), the identical, never-rebuilt, still-correct `Bitmap` object -- last blitted at
full `Opacity = 1.0` -- stops showing its text on screen anyway, while its icon (drawn into the exact
same bitmap) keeps showing fine. This is a new, sharper puzzle: it rules out "was the bitmap ever
correctly populated" and "was it ever blitted via a real native call" as the gate, since both are now
independently confirmed true, and still doesn't explain the icon/text split within one static image.

**Combining with `--patch-notification-suppress-self-paint` made it worse, not better.** Theorized
that `this` form's own still-active redundant `OnPaintBackground` paint (background+text drawn a
second time directly onto `this`'s own DC, independently of `layeredWindow`) might be sitting on top
as a stale, text-less frame, blocking `layeredWindow`'s now-correct content -- so suppressing it
should let the fixed `layeredWindow` content show through unobstructed. Live-tested: text now never
appears at all, not even during the `Appearing` burst, and the same "broken image" red-X glyph from
the fourteenth round reappeared at the fade-out transition. Working theory revised: fully suppressing
`this`'s own paint doesn't turn it into a clean transparent pass-through -- it likely leaves Wine
never seeing `this`'s own window surface get realized/painted at all, and whatever Wine shows for an
unrealized layered surface may be what that broken-image glyph actually is. **Suppress-self-paint is
now a confirmed dead end, not just a non-improvement** -- reverted, not combined with anything going
forward.

**Environment note, unrelated to the app but cost real time this round:** the X11 display's
resolution changed twice mid-session (1680x1188 -> 1920x940, and the Cinnamon screensaver activated
once, its lock-like screen fully captured by an entire 35s recording before being noticed --
`cinnamon-screensaver-command -q`/`-d` confirmed it was just the screensaver, `loginctl`'s
`LockedHint=no` confirmed the session itself was never actually locked, and it was deactivated and
disabled for the rest of the session via `gsettings set org.cinnamon.desktop.screensaver
idle-activation-enabled false` plus `xset s off -dpms`). The resolution change also invalidated a
previously-assumed screen-coordinate mapping (`Location = (100, 100)` in code showed up as `(200,
200)` via live `wmctrl -l -G` at the old resolution, then somewhere else again after the resolution
changed) -- cost a full round of misread frames before being caught. Lesson for any future round:
query `wmctrl -l -G` for the live window geometry at test time rather than reusing a previously
-observed pixel position, and reconfirm the capture resolution (`xdpyinfo | grep dimensions`) before
every recording rather than assuming it's stable across a long session.

**Net assessment:** the fifteenth round is the first attempt in this whole investigation grounded in
an actual, decompile-confirmed application-level bug (not a Wine-timing guess), and it produced the
first-ever *measurable* improvement (text visible for ~1s at notification start, versus never before
Hide/click in every prior round) -- but it's a partial fix, not a resolution: the hold period is
still blank. Given repeated attempts to combine it with other ideas have made things worse rather
than better, this is a natural point to report back rather than keep stacking speculative patches.

## Sixteenth round: FIXED -- periodic re-blit closes the gap, text visible for the entire lifecycle

Directly answering the user's question of what's actually gating text visibility during the hold,
after the fifteenth round established that both the bitmap's content and its native blit mechanism
are independently correct and unchanged throughout: does the compositor need the *same, unchanged*
content periodically re-asserted via a real `UpdateLayeredWindow` call, or does it not matter?

`--patch-notification-periodic-reblit`: a new, completely independent `Timer` (300ms), started once
per form instance (guarded by a null check at the top of `OnShown`, since the form is reused across
notifications), whose only action is `if (state == NotificationFormState.Visible)
updateLayeredBackground(refreshBitmap: false)` -- no `Opacity` or `state` mutation at all, unlike
every version in the v1-v6 keep-alive family, which all changed `alphaIncrement`/`Opacity` and
repeatedly collided with `timer_OnTimer`'s own state machine as a result. This timer does nothing
but repeat the exact same already-correct, already-proven-reliable native blit, unconditionally,
every 300ms, for as long as the notification is sitting in its hold.

**Result, deployed as the full chain (`refresh-on-content-change` + `text-in-bitmap` +
`periodic-reblit`, live-tested via the automated auto-trigger + recording loop and independently
confirmed via extracted frames): text is visible for the ENTIRE notification lifecycle** -- at
`OnShown`, throughout the hold (confirmed at multiple points, including ~3.4s and ~6.3s after
appearing, both clearly readable: "eM Client" / "You have 3 new emails"), and through to `Hide()`.
This is the first fix in the whole investigation (sixteen rounds, counting from the start of this
session's work alone) that actually resolves the reported symptom rather than shifting or
partially mitigating it.

**This directly answers the open question:** the gate is not the bitmap's content (confirmed
correct throughout, since the fifteenth round), not whether it was ever blitted via a genuine
native call (also confirmed, since `layeredWindow.UpdateWindow` is unconditionally forced on every
call) -- it is specifically that the **compositor stops honoring previously-blitted content once
the native blit calls stop arriving**, regardless of whether the content or opacity value being
re-asserted has changed at all. A once-per-300ms heartbeat of the *identical* call is sufficient to
keep it honored. This is consistent with (and sharpens) every finding across this entire
investigation: real ticking was always the gate, but "real ticking" turns out to mean literally
"a `UpdateLayeredWindow`-equivalent call happened recently," not "a value changed" -- the whole
alpha-oscillation machinery of the v1-v6 family was solving the wrong half of the problem (forcing
a value change) when only the call itself, with any or no value change, was ever necessary.

**Three patches now needed together** for the full fix (a fourth,
`--patch-notification-suppress-self-paint`, was tried and is a confirmed dead end -- do not
combine it with these):
1. `--patch-notification-text-in-bitmap` -- bake title/content into `backgroundBitmap` itself
   (via the existing `OnPaintTitle`/`OnPaintContent`, honoring subclass overrides), so
   `layeredWindow`'s blit actually has something to show.
2. `--patch-notification-refresh-on-content-change` -- force an immediate rebuild-and-blit right
   after real content is set (`ShowNotification`), so the very first blit already has it, before
   the `Appearing` tick burst even starts.
3. `--patch-notification-periodic-reblit` -- keep re-asserting that already-correct blit every
   300ms for as long as the notification is in its `Visible` hold, since native blits stopping is
   what causes the compositor to stop honoring previously-shown content.

**Not yet done (at the time this section was first written):** live confirmation against a genuine
incoming real email; folding into `releases/<version>/deploy.sh` as a new stage; removing
`--patch-diag`'s instrumentation from the final deployed build. The `__periodicReblitTimer` is
deliberately never stopped/disposed (it just no-ops once `state != Visible`) -- accepted as
negligible overhead (one 300ms timer per app session, since the form instance is reused) rather
than adding complexity to tear it down on `Hide()`/disposal.

## Seventeenth round: real-email confirmation finds a real cosmetic bug (ghost text); fixed

Tested the three-patch chain against a genuine incoming email (not just the synthetic trigger) for
the first time. User confirmed live: "I saw the text come through" -- but also caught something
the synthetic-trigger testing had missed: "The formatting was wrong... I think I briefly saw the
original text in the background appear near the end." Frame extraction confirmed exactly one
frame, right at the `Hide()`-triggered fade-out transition, showing the correct text from
`layeredWindow`'s blit *and* a fainter, ~9px-offset duplicate behind it -- a real ghosting defect,
not a false alarm.

**Root cause:** `this` form's own `OnPaintBackground` -- deliberately left untouched by the
sixteenth round's fix, since `--patch-notification-suppress-self-paint` (which disables it
entirely) was already a confirmed dead end -- still independently draws a redundant copy of
background+title+content directly onto `this`'s own device context. Per this whole investigation's
central finding, `this`'s own paint only succeeds while real ticks are actively running, which is
exactly what's happening right at the `Disappearing` fade-out burst -- so it transiently succeeds
right at that moment, producing a second, duplicate text render. The ~9px offset comes from the
`ShadowVisible` `TranslateTransform(9, 9)` applied when building the padded `backgroundBitmap` (to
leave room for the drop-shadow border) -- `this`'s own unshadowed draw doesn't replicate that
offset, so its copy lands slightly up-and-left of `layeredWindow`'s.

Retried `--patch-notification-suppress-self-paint` once more, this time combined with
`periodic-reblit` (not present during the fourteenth round's attempt), on the theory that a
constantly-refreshed `layeredWindow` surface might avoid the earlier "broken image" regression --
it didn't: the same red-X glyph reappeared, now spanning a larger area. **Confirmed for the second
time that disabling `this`'s paint entirely does not produce a clean transparent pass-through under
Wine, regardless of what else is combined with it.**

**Fix -- `--patch-notification-suppress-self-text-only`:** narrower than full suppression. Leaves
`this`'s own background/avatar `DrawImage` call fully intact (so `this`'s own surface is still
genuinely painted with *something* every frame, avoiding whatever unrealized-surface state causes
the red-X glyph) and removes only the two `OnPaintTitle(e)`/`OnPaintContent(e)` call sites
immediately after it -- the first instruction-*removal* patch in this file (every prior patch only
inserted or flipped an opcode). Straight-line code, no branches into the middle of the removed
sequence, verified via the same branch-target/handler-boundary checks used for every insertion.

**Live-tested (four patches together: `text-in-bitmap` + `refresh-on-content-change` +
`periodic-reblit` + `suppress-self-text-only`) via the automated loop and confirmed via extracted
frames at 10fps through the entire fade-out transition: no ghosting at any frame, text stays clean
and singular throughout, and no broken-image regression anywhere.** This is now believed to be a
complete fix for both the original bug and the cosmetic side effect the real-email test surfaced.

**Unrelated environment note:** mid-round, the whole X session's process group (including
MailClient) was killed by what `dmesg` showed as a `systemd-logind`/`systemd-timesyncd`
crash-and-restart -- an infrastructure-level hiccup, not an app or patch bug (no `bug.*.txt` crash
report was written, consistent with the whole session being interrupted rather than the app
crashing on its own). This is likely related to the resolution changes and screensaver oddity
documented in the fifteenth round -- worth flagging if this environment continues to be unstable
across sessions. Triggered the DB-repair-on-next-launch behavior as expected from an unclean
termination; user re-opened and let it complete before continuing.

**Still not done:** folding the final four-patch chain into `releases/<version>/deploy.sh` as a new
stage, and producing a clean deploy build without `--patch-diag`'s instrumentation layered on top
(currently still present for observability -- fine for continued testing, not for normal use).

## Eighteenth round: cosmetic follow-up -- font, padding, corner radius, and no-avatar handling

Comparing the fixed rendering against real-Windows reference screenshots
(`supporting/notifications-working-example-from-windows*.png`) surfaced several apparent cosmetic
mismatches: content/title text sitting too close to the left edge, the avatar apparently clipped
against the top-left corner, the title looking top- rather than vertically-centered in the header
band, sharp corners instead of rounded ones, and text lacking anti-aliasing. Investigated each via
direct instrumentation (`--patch-notification-layout-diag`, reading back real field/property
values at paint time) rather than guessing, per this project's established method.

**Padding: not actually broken.** `defaultPadding` reads back as `(8, 6, 8, 6)` and
`getScaledPadding().Left` as `8` -- both correct, matching a resx-defined design value applied
unconditionally by `InitializeComponent()`'s `componentResourceManager.ApplyResources(this,
"$this")` call (no theme check, no conditional logic -- this runs identically regardless of active
theme or DPI). A careful re-crop with nearest-neighbor scaling (avoiding LANCZOS resampling blur,
which was distorting apparent edge positions in earlier comparisons) confirmed the actual on-screen
padding is in the right ballpark. The original "flush-left" impression was a measurement artifact
from imprecise reference-image cropping and lossy resampling, not a real defect. No padding patch
exists or is needed; the app's own values are used entirely unmodified.

**`headerFont` null -- a real, confirmed bug, now fixed.** `--patch-notification-layout-diag`
showed `headerFont` is `null` (`Font` is non-null but also not the intended custom font) at every
`doLayout()` call before `OnShown` fires, and *only* before -- proving `FormGenericNotification.
OnLoad()` (the only code that sets `headerFont`/`Font` from `FontManager.UIFont` at 11pt/10pt)
never fires on its own under Wine. This is the exact same class of bug already found and fixed
once in this project (`reports/settings-panel-clip-region-findings.md`: `formSettings`'s `Load`
event never firing) -- a WinForms lifecycle callback silently not running, not a rendering gap.
Since `TextRenderer.DrawText` doesn't throw on a null `Font` (native `DrawTextEx` just falls back
to whatever raw GDI stock font is already selected into the HDC), this plausibly explains both the
wrong-looking title font and its lack of anti-aliasing (a legacy GDI stock font, not a TrueType
one). **Fix -- `--patch-notification-force-onload`:** call `OnLoad(EventArgs.Empty)` directly from
`OnShown`, guarded by the existing `loaded` field (set `true` at `OnLoad`'s own end) so it runs
exactly once despite `OnShown` firing repeatedly across the form's reused lifetime -- necessary
because `OnLoad` unconditionally subscribes `ThemeManager.Instance.ThemeChanged` with no matching
unsubscribe, so calling it twice would reproduce the exact double-subscription bug already fixed
for `layeredWindow.Click` in Stage 8. **Live-tested and confirmed via diag log** (headerFontNull=1
before OnShown, =0 after, for the rest of the notification's life) **and visually** (title font
changed from a bold, mismatched stock font to a lighter weight matching the content font's style,
consistent with the intended UI font actually being applied now).

**`CornerRadius` = 0 -- confirmed by design, not a bug.** `CornerRadius`'s getter returns `0`
unless the active theme's `NotificationWindowUseRoundedCorners` is `true`. Checked every
`IColorTheme` implementation in `MailClient.Common.UI.dll`: only `SystemColorTheme` returns `true`;
every other theme (`Dark`, `Default`, and every named color theme) explicitly sets it `false`. The
reference screenshot's rounded corners come from a different active theme setting on the reference
machine, not a Wine gap -- no patch applied or needed.

**Vertical centering: resolved as a side effect of the font fix.** `OnPaintTitle` uses
`TextFormatFlags.VerticalCenter` without `TextFormatFlags.SingleLine`, which is documented
(MSDN) to make `VerticalCenter` have no effect -- a real latent issue in the app's own flags,
independent of any Wine gap. In practice, though, a precise pixel measurement after the font fix
(title text spanning roughly the middle third of a 47px-tall header band) shows it landing close
enough to center to not be a visibly distinct problem -- the earlier, more dramatic top-alignment
appearance was very likely a side effect of the *wrong* fallback font's different line-height/
ascent metrics interacting with this flag gap, not a separate bug needing its own fix. Not
patched; revisit only if a future check shows it's still visually off with the correct font in
place.

**No-avatar-image (monogram) handling: verified working correctly, no patch needed.** The user
asked to confirm eM Client's own initials-avatar logic is used correctly rather than reimplemented,
since the test account's contacts all have photos (`AvatarHelper.GetAvatarWithFallback` never hits
the monogram branch for any reachable real test). Full decompile of `UIAvatar.GetImageSingleRes`'s
monogram branch confirmed it's pure GDI+ vector drawing (`Graphics.FillEllipse` for the colored
circle, `Graphics.DrawString` with `TextRenderingHint.AntiAliasGridFit` for the initials) -- it
never touches `InterpolationMode` (unlike the *real-photo* scaling branch two lines below it, which
uses `InterpolationMode.High`, already a confirmed-broken "Hold row" item in this file's own
Status section -- a separate, pre-existing, not-yet-patched issue, unrelated to this fix) and
doesn't depend on `headerFont`/`Font` at all (it builds its own local `Font` directly from
`FontManager.UIFont.FontFamily`). The resulting `Image` feeds into the exact same `Image` property
and `updateBackgroundBitmap()` avatar-drawing code already confirmed reliable for real photos.
Built `--patch-test-monogram-avatar` (a new public `__showTestMonogramNotification` method calling
`UIAvatar.FromMonogram` directly, bypassing the unreachable-in-testing higher-level fallback logic)
to verify live rather than trust the code review alone -- **confirmed working correctly**: a
purple/lavender circle with bold white "JW" initials, cleanly anti-aliased, matching
`supporting/email-with-no-image.png` closely. No patch needed for this case.

**New dev-only tool, not part of the real fix:** `--patch-close-listener` -- lets this session
close eM Client gracefully (via the real `menuItem_File_Exit_Click`/File>Exit code path, triggered
by a polled marker file at `Z:\tmp\claude-close-signal`) without needing to ask the user to close
it by hand before every redeploy. Confirmed via a live close+relaunch cycle to produce no DB-repair
prompt, unlike killing the process. See `feedback_close_listener_tool` in this session's memory for
reuse in future rounds; must be stripped before any release build.

## Nineteenth round: direct font/AA pixel comparison against the real-Windows reference (no new patch)

User directly challenged this whole line of investigation: several rounds had chased font
*registration* hypotheses (headerFont null, OnLoad never firing) without ever doing the cheapest
possible check first -- put the working-Windows screenshot and the Wine screenshot side by side and
look at the actual pixels. Also flagged two process problems, both fixed before this round's actual
comparison could even happen:

1. **The dev/testing rule allowing this session to start and close eM Client itself
   ([[feedback_close_listener_tool]]) was being under-used** -- this session had been asking the
   user to launch/watch the app each cycle instead of just doing it. Corrected; see that memory's
   own note.
2. **`--patch-test-monogram-avatar`'s fixed one-shot delay timer lost its race against a real
   capture attempt** -- a delay-timed screenshot found the notification window already
   `Map State: IsUnMapped` (auto-hidden) by the time it ran, producing nothing useful. Per the
   user's own suggestion, converted it to the same file-trigger polling idiom already used by
   `--patch-close-listener`: it now polls every 150ms for `Z:\tmp\claude-trigger-notification`
   instead of firing after a guessed delay, so a screen recording can start first (with no time
   pressure at all) and the notification fires on this side's own schedule by touching the trigger
   file once ready. Confirmed via decompile: the new `__monogramTestTick` reads exactly like
   `__closeListenerTick`'s own `if (File.Exists(...)) { File.Delete(...); ... }` shape. No branch/
   handler-region concerns (a plain `Brfalse` to the method's own final `ret`, verified by
   decompile).

**Method:** started an `ffmpeg -f x11grab` recording of the notification's screen region *first*,
waited 1.5s for it to actually start, then `touch`ed the trigger file, and let the recording run
8s (long enough to cover the trigger, the still-open empty-box-until-fade gap, and the fade-out
transition). Extracted frames with `ffmpeg -vf fps=...` and compared against
`supporting/notifications-working-example-from-windows-with-long-subject.png` at matched, upscaled
(nearest-neighbor, to avoid the resampling blur that caused the eighteenth round's own padding
false alarm) crops.

**Confirms the still-open empty-box bug reproduces exactly as documented:** the test notification's
content area was genuinely blank for the entire ~6.5s hold, and both title and content text only
became visible once the fade-out transition began -- not a new finding, but a live, direct
re-confirmation with the file-trigger method now in hand for any future round of that
investigation.

**Font family/size: no mismatch found.** At matched zoom, the Wine-rendered title ("Jaguar Workshop
Auto Resto...") and the Windows-rendered title ("Test Sender Full Name") show the same letterform
style -- proportions, x-height, and two-story lowercase 'a' all consistent with the same font
family (Segoe UI or a close match), not a fallback/substituted typeface. Sizing relative to the
avatar circle is in the same ballpark on both sides. No font-substitution bug.

**Initial false lead, caught and ruled out before being reported as a bug: apparent "blue content
text" on Windows.** Pixel-sampling the Windows reference's content line at first turned up
strongly blue-skewed pixels (e.g. RGB (86,158,224), (158,224,255)) against a near-neutral Wine
sample (e.g. (152,159,143), (110,120,114)) -- looked exactly like a real, distinct
content-vs-title color bug. Checked the app's own code before reporting it as one (per this
project's standing "decompile and verify before concluding" discipline): grepped every
`IColorTheme` implementation in `MailClient.Common.UI.dll` (all 20 -- Aqua, Arctic, BlueLight,
Bordeaux, Classic, Custom, Dark, Default, Green, IceWarp, Industry, Mystic, Pink, RedEffect, Rose,
System, TurkCellDark, TurkCellLight, Viola, YellowJacket) for `NotificationWindowForeground`
(content color, used by `OnPaintContent`'s `ForeColor`) vs `NotificationWindowHeaderForeground`
(title color) -- **every single theme sets both to the identical value**, with no exception. There
is no theme, built-in or otherwise, in which this app's own design would show title and content in
different colors, let alone make content specifically blue. So a theme-selection mismatch (the
eighteenth round's `CornerRadius` false-alarm shape) couldn't be the explanation either.

Re-examined the Windows reference at higher zoom (4x, nearest-neighbor) with this in mind, and the
answer became visually obvious: the "blue" pixels are **ClearType/subpixel anti-aliasing fringing**
-- a thin orange/red fringe on one side of each glyph stroke and a blue/cyan fringe on the other
(classic LCD-subpixel AA signature on a dark background), not a solid blue fill. The underlying
text is the same white/gray as the title; ClearType's per-subpixel rendering just colors its AA
transition pixels. The same zoom level on the Wine-rendered title showed pure grayscale AA
gradients at glyph edges -- no color fringing at all, confirmed by sampling (R≈G≈B throughout,
e.g. (84,89,89), (78,77,82) -- no orange/blue split at any edge pixel).

**Conclusion: the real, confirmed rendering difference is anti-aliasing method, not font
substitution or color.** Windows renders notification text with ClearType (subpixel/LCD-aware AA,
producing colored edge fringing); Wine's GDI+ text rendering uses plain grayscale AA (no
subpixel/LCD awareness). This is a rendering-*engine* characteristic of Wine's `gdiplus`/font
stack as a whole (consistent with this project's very first finding --
`gdiplus-interpolation-findings.md` -- that Wine's GDI+ implements only a subset of GDI+'s
rendering features) rather than anything specific to this notification code or fixable via an
app-level IL patch: ClearType requires knowing the physical subpixel layout of the actual display
and rendering per-subpixel, which is a font-rasterizer/compositor capability, not something
`MailClient.dll` or `MailClient.Common.UI.dll` control at all. No patch applied or planned for
this -- flagging it here so a future round doesn't re-open the same "font looks wrong" question
without first re-reading this comparison.

## Twentieth round: user directly disputed padding/line-height "not a bug" -- geometry checks out, but a new negative result on the open bug's own mechanism

User pushed back hard on the nineteenth round's scope: two specific things still look wrong to
them -- left padding, and the two content lines' height/spacing not matching the Windows
reference -- and asked for those to actually get fixed, not waved off.

**Checked the real geometry, not just pixels this time.** `--patch-notification-geometry-diag`
(built in the nineteenth round but not yet actually read) had been logging `doLayout()`'s real
`headerRect`/`contentRect`/`imageRect` all along -- `/tmp/claude-diag.log` (note: `Z:\tmp\...` is
`/tmp/...` on the Linux side directly, **not** under the bottle's own `drive_c/tmp` -- wasted a
few minutes checking the wrong path first). Actual values for this test: `header={X=0,Y=0,
Width=310,Height=40}`, `content={X=8,Y=45,Width=294,Height=75}`, `image={X=8,Y=4,Width=32,
Height=32}`. Read `doLayout()`'s and `OnPaintTitle()`'s full source alongside this: content text
and the avatar image intentionally share the same `X = scaledPadding.Left` (8px) -- confirmed by
code, not inferred -- and title text's bounds separately add `imageRect.Width + 8px` on top of
that same base padding. Overlaying vertical reference lines at these exact computed X positions
onto an annotated, nearest-neighbor-zoomed capture confirmed the *rendered* text starts almost
exactly where the code says it should on both sites of that check.

**Directly compared the same computed relationship against the Windows reference and it matches.**
Measured the Windows screenshot's own box/avatar/content-text edges the same way (pixel dumps, not
eyeballing): avatar diameter ~31px there vs Wine's ~30-32px (matches, ruling out a DPI/scale
mismatch skewing the comparison); content text starts ~3px right of the avatar's own left edge on
Windows vs exactly flush with it on Wine -- a few-pixel difference, not the kind of gap someone
would describe as "wrong" padding. **Did not find a left-padding bug** -- the numbers match the
app's own coded design intent and are close to the Windows reference. Line-spacing (content line1
peak-row to line2 peak-row): measured ~17-18px on Wine (reproduced twice, across two independent
recordings) vs ~19px on Windows -- a real but modest (~10%) difference, plausibly just a font-
metric/hinting difference between Wine's rendering of the font and real Segoe UI rather than
anything `doLayout()`/`OnPaintContent()` controls (neither sets an explicit line height anywhere;
both let `TextRenderer.DrawText` use the font's own natural metrics).

**Real methodological gap acknowledged, and a genuine attempt to fix it that produced a useful
negative result instead.** Every measurement above (this round and the nineteenth's) was taken
from a screen recording during the *fade* transition, because of the still-open empty-box-until-
fade bug -- meaning every frame is a partial-opacity blend with whatever's behind, not a clean,
fully-opaque render, which is real noise on top of true pixel positions. Tried to fix this
properly rather than keep measuring through it: found `timeToHide`/`timeToShow` (protected `int`
fields on `FormGenericNotification`, default 500ms, directly driving `alphaIncrement = ±25f /
timeToHide|timeToShow` per `timer_OnTimer`'s own decompiled code) and added two field-writes
(`timeToHide = 5000; timeToShow = 5000;`) to the very top of `__showTestMonogramNotification` --
dev-test-only, 10x slower fade, expecting many more high-opacity ticks to capture from.

**It didn't help, and that's itself useful evidence.** A second recording confirmed via the diag
log that the notification really did sit in `NotificationFormState.Disappearing` for ~10s of
ticks (401 ticks at the expected ~25ms cadence) instead of a near-instant fade -- but content only
became visible in the last ~0.5s before the window fully disappeared, exactly as brief a window as
in the original, fast-fade capture. **Slowing the fade down does not widen the reveal window --
the reveal stays pinned to a narrow moment right before the notification fully closes, regardless
of how long the fade itself takes.** This weakens any theory that more/slower ticks alone would
fix the open empty-box bug (already suspected from the fourteenth round's periodic-reblit
experiment, but now confirmed directly against the *unpatched* mechanism too) and points more
specifically at whatever happens right at the `Hide()`/`setFormHidden()` transition itself as the
real trigger for a genuine repaint, not sustained ticking at any speed. Kept the
`timeToHide`/`timeToShow` slowdown in `--patch-test-monogram-avatar` anyway since a longer window
is still marginally easier to catch on camera even if not meaningfully clearer, and it's harmless
and dev-only.

**Where this leaves the two complaints:** no code-level bug found for either, and the numbers
back that up better than the nineteenth round's font/AA comparison did (that one was pixel-
eyeballing; this one cross-checked against the actual `doLayout()` source and its own logged
output). Given the measurement is still going through a partial-opacity capture no matter how the
fade is tuned, treating "no bug found" as final would repeat this project's own standing worry
about false confidence -- flagging both as unresolved-but-probably-not-bugs rather than closed,
pending either a genuinely opaque capture (which would need the real empty-box fix first) or the
user's own direct, live-eyes comparison to point at something more specific than these frames
show.

## Twenty-first round: the empty-box fix regressed out of the dev chain, then came back -- and with it, real, confirmed padding/alignment bugs

Between rounds, the user noticed live that text had stopped showing at all, at any point in the
notification's lifecycle -- a regression from the sixteenth/seventeenth rounds' confirmed fix.
Root cause: this session's own earlier rebuilds (rounds nineteen and twenty) built their dev/test
chain starting from the plain Stage-1-7 `output-final/` tree and never re-applied the four-patch
chain (`--patch-notification-text-in-bitmap`, `--patch-notification-refresh-on-content-change`,
`--patch-notification-periodic-reblit`, `--patch-notification-suppress-self-text-only`) that fixed
it -- an honest process gap, not a code regression. Rebuilt the dev chain with all four patches
included again, verified through the full pipeline (interpolation scan, `--dump-handlers`,
decompile of every touched method), deployed, and confirmed via `ffmpeg` screen recording: text
visible continuously through the whole appear-hold-fade lifecycle this time, not just glimpsed at
the fade transition. See CLAUDE.md's own Status section for the corrected long-term record of this
fix; this paragraph is the regression's own story for future reference.

Fixing this unlocked something the twentieth round explicitly couldn't do: a genuinely opaque
capture. Redid the padding/line-height measurement from scratch against clean, non-faded frames
(`ffmpeg` recording, `fps`-based frame extraction, no fade blending at all) -- and this time found
three real, confirmed layout bugs the twentieth round's fade-blended measurements had missed or
gotten wrong:

1. **Title text not vertically centered in its header band** -- sits with its ink concentrated in
   the upper half, closer to the top than genuinely straddling the header's vertical center.
   Traced to `TextRendererEx.DrawTextInternal`'s own decompiled source: for non-emoji text (this
   title) it falls through to plain `System.Windows.Forms.TextRenderer.DrawText` -- so this is
   real native Win32 `DrawTextEx` under Wine, not a custom rendering path, and it isn't honoring
   `TextFormatFlags.VerticalCenter` correctly for this flag combination. A genuine Wine gap, not a
   design-constant tweak.
2. **Content text flush against the avatar's own left edge**, where the real-Windows reference has
   a small (~4px, measured precisely via pixel-edge-detection on both images, not eyeballed) inset
   past it.
3. **Content text starting with no top padding**, right at the header/content boundary, where the
   reference has visible breathing room above the first line.

(The twentieth round's own conclusion -- "no bug found, probably a Wine font-metric artifact" --
was wrong for reasons directly traceable to its own acknowledged methodology gap: every
measurement in that round came from a partial-opacity fade-blended frame, because the empty-box
bug was still live at the time and no opaque capture was possible. Padding measurements are
edge-position-sensitive in a way font/AA comparisons aren't, and partial alpha blending shifts
apparent edge positions enough to hide a real ~4-6px offset. Confirms the round's own stated
worry -- "no bug found" from a compromised capture method isn't the same as "no bug" -- was
justified.)

**Fixes**, all built against raw IL dumped via a new `--dump-il <dll> <type> <method>` utility
mode (same rationale as `--dump-handlers`: planning an edit against decompiled C# is guesswork
about what the compiler actually emitted for compact vs. general opcode forms; dumping the real
instruction stream removes that guesswork):

- `--patch-notification-content-padding`: edits `doLayout()`'s `contentRect` construction --
  `X` gets `+4` (with `Width` trimmed by the same 4 so the right edge doesn't move), and the `Y`
  offset changes from `+5` to `+11` (with `Height`'s trailing `-10` becoming `-16`, again to keep
  the bottom edge fixed). Four independently-verified edits within the same
  `getScaledPadding()`...`stfld contentRect` instruction range, not a wholesale rewrite.
- `--patch-notification-avatar-title-gap`: `OnPaintTitle`'s avatar-to-title gap constant, a
  compact-form `ldc.i4.8` (not a general `ldc.i4 8` -- compact-form operands are implicit in the
  opcode and can't be edited in place, so this replaces the instruction outright), becomes
  `ldc.i4 14`.
- `--patch-notification-title-vcenter-fix`: stops relying on Wine's `VerticalCenter` at all.
  Measures the title's actual rendered size via `TextRenderer.MeasureText(title, headerFont, new
  Size(bounds.Width, int.MaxValue), SingleLine | NoPrefix)` -- the same native call, just asked
  what size it needs instead of asking it to center -- then explicitly sets `bounds.Y`/
  `bounds.Height` so the measured text exactly fills the bounds passed to `DrawText`; top-alignment
  within a rect sized to match the text is mathematically identical to true centering, without
  depending on `DrawTextEx`'s own (here, wrong) centering math. Drops `VerticalCenter` from the
  final flags constant (34852 -> 34848) since it's now a no-op by construction.

  Two real implementation bugs were caught and fixed before this one worked, both worth recording
  as their own lessons (added to CLAUDE.md's "IL-patching lessons" -- see there for the canonical
  version): (a) the `bounds.Height = measured.Height` sequence was initially missing the `Ldloca_S
  boundsLocal` push before the value, calling `set_Height` with the wrong stack shape entirely;
  (b) far more subtly, the final flags-constant edit was originally done via a captured `int`
  index into the instruction list rather than a captured `Instruction` object -- since ~20 new
  instructions were inserted earlier in the same list first, the stale index ended up pointing at
  one of those new instructions instead, and `.Operand = 34848` silently corrupted it (a
  `VariableDefinition`-typed operand overwritten with an `int`), surfacing only as an
  `InvalidCastException` deep in Cecil's writer at `module.Write()` time -- decompiling the
  pre-write state wouldn't have caught this at all, since the corruption only existed after
  insertion shifted indices. General lesson: never hold an instruction position as a bare `int`
  index across any insertion/removal on the same list -- capture the `Instruction` object itself.

Also extended `--patch-test-monogram-avatar` with a new `__showTestNotificationButtons()` method
(added directly to `FormMailNotification`, not the base class, since `button_Reply`/`button_Flag`/
`button_Delete` are `FormMailNotification`'s own private fields and the CLR verifier only allows a
type's own methods to touch its private members) to check whether the new content-padding
adjustments would collide with the reply/flag/delete controls -- user-requested, to answer "will
this fit?" without needing a real email. Calling `.Show()` on the three buttons alone produced no
visible change (consistent with this whole project's throughline: a visibility/state change alone
doesn't reliably trigger a Wine repaint); added `PerformLayout()`/`Invalidate()` calls after the
`Show()` calls, same as the main test method already does, but the buttons still didn't appear in
the time available this round. Left as an open, low-priority test-tooling gap -- the actual
positioning fixes above were confirmed independently via the video-recorded, non-faded frames
without needing the buttons visible, so this doesn't block anything, but overlap with those
controls specifically has NOT yet been confirmed either way and should be checked against a real
incoming email before considering this fully done.

**Confirmed via `ffmpeg` recording against a clean, non-faded frame, compared directly to
`supporting/notifications-working-example-from-windows-with-long-subject.png` at matched zoom**
(saved as `supporting/notification-layout-fix-comparison.png`): title now sits with visible space
above and below within its header band, matching the reference's centering; avatar-to-title gap
visibly widened; content text now starts with both a small left inset past the avatar and a top
gap before the first line, matching the reference's proportions closely. All three fixes verified
through the standard pipeline (interpolation regression scan, `--dump-handlers`, decompile of
every touched method) before deploying, per this project's standing discipline.

**Correction (twenty-second round, immediately after): this "confirmed" claim was wrong, caught
by the user, and the actual cause was a real measurement gap this project should have caught
itself.** The user tested with a genuine incoming real email (not this session's synthetic
Jaguar-Workshop test) and reported "made no difference. All alignments are still wrong" --
directly disputing the round-twenty-one claim above. Re-measured properly this time: precise
per-column and per-row pixel-deviation profiles (not eyeballing a 4x-nearest-neighbor crop, which
is what produced the false "confirmed" claim above) against a freshly-deployed, freshly-launched
build, cross-checked against the app's own logged `contentRect`/`headerRect` values. Found the
title-vcenter-fix's `TextRenderer.MeasureText`-based centering, while decompiling exactly as
designed, was **not producing a visually centered result at all** -- the title's ink stayed
entirely in the upper half of the header band (measured span: frame-relative Y 24-34, dead center
~29, against the header's true center at Y 35 -- essentially zero ink below center).

**Root cause of why the "fix" didn't fix anything:** `TextRenderer.MeasureText`'s returned
`Size.Height` is the font's full line-height metric (ascent + descent + internal leading), not the
visible glyph span. Mathematically centering a box sized to that full metric still visually favors
the ascent region for ordinary Latin text, because the reserved descender space below the baseline
mostly goes unused (this title's few true descenders -- the 'g' in "Jaguar", the 'p' in
"Workshop" -- don't reach anywhere near the metric's full reserve). The box was centered exactly
as coded; the ink within it wasn't, because the box and the ink don't coincide. Computing "true"
centering from the font's actual cap-height-to-baseline span (`FontFamily.GetCellAscent`/
`GetCellDescent` design-unit metrics) would risk hitting the exact same Wine-font-metric-reporting
gap that caused this in the first place, so this project's own established pragmatic-empirical
style was used instead: measured the actual gap directly (target ink-center Y=20 minus observed
ink-center Y=14, in window-relative units, = 6) and added that as a flat `+ 6` to the computed
`bounds.Y`, live-verified against pixel measurements rather than assumed.

The same live remeasurement also caught a second instance of the identical bias: content text's
top padding (from `--patch-notification-content-padding`) rendered ~5px higher than its coded
`contentRect.Y`, matching the title's own pattern closely enough to suggest this is a **general
Wine text-positioning characteristic** (rendered ink sits systematically above whatever Y
coordinate a text draw call is given, for both `VerticalCenter` and plain top-aligned text) rather
than anything specific to `VerticalCenter`. Corrected the same way: measured the gap directly (ink
onset moved from frame-Y 61 under the round-twenty-one coding to a target of 66) and bumped the Y
offset by that same +5 (contentRect's `+11` -> `+16`, `Height`'s trailing `-16` -> `-21` to keep
the bottom edge fixed).

**Re-verified from scratch after both corrections**, same rigor as the disproof: fresh app launch
(confirmed genuinely fresh via a ~9s startup, not an instant-reuse of a stale instance -- CrossOver
bottles single-instance-activate rather than truly relaunch on a second `cxstart` of an
already-running app, a real trap hit repeatedly this round while chasing this measurement, see
below), `ffmpeg` recording, per-row/per-column pixel profiles cross-checked against the app's own
logged `contentRect` values. Title ink band now spans frame-relative Y 27-37 (center ~32) against
the true center at Y 35 -- a 3px residual, down from the original ~11px and the first "fix"
attempt's ~9-11px (itself no improvement at all). Content ink onset now lands at frame-relative Y
66, exactly matching where the correction's math predicted (confirming the +5 bias is a real, flat
offset, not noise -- moving the coded target by 5 moved the rendered result by exactly 5). Visual
comparison against the reference screenshot at matched zoom (title vertically centered similarly,
content indented and top-padded similarly) sent to the user for direct confirmation alongside
these numbers, not instead of them.

**Live-capture lessons hit hard this round, worth recording for next time:**
- **A second `cxstart` of an already-running bottle instance does not relaunch the app -- it
  activates the existing window and returns almost instantly (~1s vs. a genuine ~9-10s cold
  start).** Burned significant time this round assuming a fresh launch when the instance was
  actually hours-old and in an unknown state (window z-order, timer state) from many prior test
  cycles. Check for this by timing the "main window loaded" poll -- a ~1s result means it's the
  same instance, not a new one; if a genuinely fresh state is needed, close first (graceful
  `close-listener` signal, confirmed via `pgrep` returning nothing) and only then relaunch.
- **The main application window can end up stacked in front of the notification popup**,
  hiding it completely from any screen region grab even though the notification window genuinely
  exists and is mapped (`xwininfo` shows `IsMapped`) -- confirmed by taking a full-desktop
  screenshot mid-investigation and seeing the main window's own mail list rendered exactly where
  the notification should have been. Restacking it programmatically (`ConfigureWindow` with
  `stack_mode=Above`, with or without an explicit sibling) did **not** reliably fix this in this
  Cinnamon/Muffin session -- the WM did not honor it. What worked: moving the main window itself
  out of the way (`wmctrl -ir <id> -e 0,x,y,w,h`) before triggering the notification, or --
  ultimately more reliable -- doing a genuinely fresh app launch (previous bullet), which reset
  whatever z-order state had accumulated.
- **The notification's own visible window is short-lived enough that any multi-step Python script
  (import Xlib, connect to the display, poll, capture) risks missing it entirely**, even when the
  poll loop itself detects the window within one 200ms iteration -- Python/Xlib startup overhead
  alone can eat the whole window before a `get_image()` call ever runs. An `ffmpeg` recording
  started *before* the trigger and left running for several seconds afterward, then frame-extracted
  after the fact, was far more reliable than any single-shot live screenshot attempt for this
  short-lived a target, even one triggered by "the window just appeared."

## Twenty-third round: two more directly-measured corrections, using the user's own worked method

Twenty-second round's re-verification (title 3px residual, content-Y bias-corrected) was itself
still not quite right by the user's own live-eyes check against the deployed build. Two further,
smaller corrections, both derived the same directly-measured way rather than re-guessed:

**Title still sat slightly high.** The twenty-second round's own re-measurement had used the whole
title string's ink band (all-caps-and-lowercase mixed) to compute a center, which skews high
relative to what a human eye reads as "the text," because ascender letters ('J', 'W', 'A', 'R')
pull a whole-band center up above where the more numerous x-height letters actually sit. Remeasured
using only the x-height letters specifically -- the round 'o' in "Workshop" (a consistent 8-column-
wide flat top/bottom span, frame-relative Y 30-37, easily distinguished from the taller ascender
columns around it) -- centered at Y 33.5 against the header's true center at Y 35, needing 1.5-2px
more. Bumped the existing empirical Y-correction from `+6` to `+8`.

**Content text's left inset had the sign backwards in effect, not just magnitude.** Despite the
field correctly reading `contentRect.X = 12` in the geometry-diag log (a `+4` insert on top of
`scaledPadding.Left`), live pixel measurement found the *rendered* ink starting at frame-relative
X ~21-22 -- to the **left** of the avatar's own left edge (X=24), not the small inset to its right
that the value implied. The same rendering bias --patch-notification-title-vcenter-fix's Y-fix
and the twenty-second round's content-Y-fix both already found, this time showing up on the X axis
too: content text renders ~5-6px left of its own coded X, not just high of its coded Y. Corrected
using the user's own directly-specified method (rather than assuming the vertical bias's magnitude
transfers unchanged to the horizontal case): measured D, the distance the ink currently sits left
of the avatar's edge (~2.5px), and added `2*D` (~5) to the existing `+4` insert -- once to reach
the avatar's edge, once more to land the same D as a genuine inset past it. `+4` became `+9`.

**Re-verified from scratch, genuinely fresh app launch (confirmed via the twenty-second round's own
lesson -- timed the "main window loaded" poll at 7s, not the ~1s that would mean a stale reused
instance), `ffmpeg` recording, same per-column/per-row pixel-profile method:**
- Title x-height center (the 'o' in "Workshop"): frame Y 33-39, center ~35.5, against the true
  center at Y 35 -- effectively exact.
- Content ink onset: frame X ~26.5, exactly matching the predicted `avatar_edge(24) + D(2.5)` from
  the correction's own math -- confirms the bias model, not a coincidence.

Annotated proof frame (green = true header center through the 'o's, yellow = avatar's left edge,
cyan = measured content ink onset ~2.5px right of it) sent to the user directly rather than only
reported as numbers, per the standing instruction from this whole investigation not to claim a fix
without a fresh, precise, re-shown measurement every time.

## Twenty-fourth round: reply/flag/delete/close/settings also invisible-until-fade; option 2 (plain overlay windows) hits a real, unresolved Wine z-order/paint blocker

With title/content text now genuinely fixed and confirmed, the user asked to check the reply/
flag/delete/close/settings icon row for the same bug. Confirmed directly: at a static "hold"
frame the icons are completely absent (solid background, nothing there); at a fade-transition
frame from the same recording, the reply/delete icons are clearly visible. Same mechanism as the
title/content text bug before its fix -- these are real `Control`s (reply/flag/delete) or
manually-drawn images (close/settings) painted through `this` form's own live `WM_PAINT` path,
which only actually composites during a real message-loop tick under Wine.

**Two candidate fixes discussed with the user:**
1. A genuinely layered overlay window using `UpdateLayeredWindow` (the same forced-blit mechanism
   `layeredWindow` itself already uses reliably) -- correct, but needs its own re-blit-on-every-
   interaction-state plumbing, more work.
2. A plain, non-layered, background-color-matched overlay window, positioned over the button
   area, since normal `WM_PAINT`-driven windows are NOT known to have this project's core bug
   (only `SetLayeredWindowAttributes`-driven ones are) -- simpler in principle.

Agreed to try option 2 first, with option 1 as the explicit fallback if it failed.

**Built:** `MailClient.Notifications.ButtonOverlay.dll`, a new companion assembly (same pattern as
`MailClient.Licensing.BouncyCastlePatch` -- see `il-patches/MailClient.Notifications.
ButtonOverlay/`) exposing a single `ButtonOverlayManager.Sync(...)`/`HideAll(...)` entry point,
deliberately keeping all the Rectangle math, `Controls.Find("tableLayoutPanel1", true)` lookup
(finds the reply/flag/delete panel by its designer-assigned `Name`, sidestepping the fact that
it's a private field FormGenericNotification's own code can't reach), and screen-coordinate
translation in ordinary C# rather than hand-assembled IL. `Sync` reparents the existing
`tableLayoutPanel1` (real controls, hover images, click handlers all untouched) onto one overlay,
and builds two new small `PictureBox`-based buttons replicating close/settings' existing hover-
image-swap behavior on a second overlay (close/settings were never real `Control`s in the
original code -- just `closeRect`/`settingsRect.Contains(location)` hit-testing in
`performMouseClick` -- so there was nothing to reparent for them).

Wired into `MailClient.dll` via a new standalone Cecil tool,
`notification-button-overlay-patcher` (same rationale as `license-oaep-patcher`: adding calls
into a newly-added sibling assembly doesn't fit `il-patcher`'s per-fix flag pattern). Deliberately
kept the actual patched IL a straight-line sequence of field loads, two `EventHandler` delegate
constructions, and one call -- no branches, no Rectangle/Controls.Find logic in IL at all, to
minimize the chance of another instance of this file's own hand-assembled-IL bugs. It didn't avoid
them entirely: two real bugs were caught and fixed before this worked --
`--dump-il`/decompile-verified before deploying, per this project's own standing discipline:
- The `ShowNotification` insertion originally used `InsertAfter` twice on the *same* fixed anchor
  instruction to emit two new instructions -- exactly IL-patching lesson 1's trap (each
  `InsertAfter(anchor, X)` call lands immediately after the anchor regardless of what's already
  there, so two calls in source order produce the *reverse* order in the final IL). Fixed by
  inserting both `Before` the anchor's own `.Next` instruction instead, in normal source order.
- A separate, more subtle version of the same class of bug nearly shipped in `doLayout`'s content-
  padding patch during the earlier round: not repeated here, but worth noting `--dump-il` (added
  this session specifically to make catching this kind of issue easier -- see the twenty-first
  round) earns its keep again.

**Deliberate simplifications, disclosed up front rather than discovered later:** both overlays are
always-visible while the notification itself is visible, not hover-gated the way the original
close/settings code was (mouse anywhere over the notification) -- avoided hooking a second
layered window's mouse-enter/leave into the real form's own tracking, which also drives an
unrelated reshow-on-hover behavior. Background color is a flat theme-color sample
(`NotificationWindowHeaderEnd`/`NotificationWindowBackgroundStart`), not a replicated gradient.

**Confirmed, after real debugging (a live "process silently vanishes with no crash report and no
diag-log growth" mystery that turned out to be two compounding false leads -- see below, not a
crash at all):**
- The overlay windows genuinely get created, correctly sized/positioned (`xwininfo` geometry
  matched the computed screen bounds exactly), and correctly hidden again when `Hide()` runs
  (`ButtonOverlayManager.HideAll`'s hook works).
- **Painting works** for a plain, unmodified `Show(owner)` overlay -- confirmed unambiguously with
  an intentionally garish debug `BackColor` (`Color.Lime`), which showed up reliably across dozens
  of consecutive video frames once this configuration was reached.
- **Z-order does not work.** The overlay paints correctly but sits *behind* the notification
  window, so in practice nothing is visible (the theme-matched real background color made this
  indistinguishable from "not rendering at all" until the lime debug test settled it). Every
  tested fix for this made painting stop working entirely again, every single time: `TopMost`,
  `Control.BringToFront()` called synchronously right after `Show()`, the same `BringToFront()`
  deferred to a later message-loop tick via `BeginInvoke` (in case the break was a race with the
  window's very first paint, not the mechanism itself), and the `ShowWithoutActivation` +
  `WS_EX_NOACTIVATE` combination (tried because it seemed like the least invasive option). Not "no
  visible effect" in any of these cases -- genuinely, reproducibly, back to zero lime pixels in
  the entire recording.

**Two real false leads hit while chasing this, both resolved, both worth recording:**
- **The user's own separately-reported observation -- a test notification can only be triggered
  once per app session -- explains several apparent "process crashed" observations that weren't
  crashes at all.** `--patch-test-monogram-avatar`'s tick handler calls
  `__monogramTestTimer.Stop()` unconditionally on its one successful fire and never restarts it;
  touching the trigger file again in the same session does nothing (silently: no diag-log growth,
  trigger file left unconsumed), which looks identical to "the app died before processing the
  trigger" unless the process list is checked directly. Confirmed via careful `pgrep` polling that
  several "crashed" observations during this round were actually this -- the app was alive the
  whole time, just not listening anymore. Always close (`close-listener` signal) and do a
  genuinely fresh launch (confirmed via the `~7-9s` startup timing, not the `~1s` that means a
  reused/reactivated instance -- see the twenty-third round) before re-triggering, or fix the
  timer to restart itself, before trusting a "trigger didn't fire" observation as a real bug.
- **A single-shot Python/Xlib screenshot at the exact geometric coordinates of a since-hidden
  overlay window looks identical to "the overlay never rendered"** -- both show whatever was
  genuinely behind it. Cross-checked with `xwininfo`'s `Map State` before trusting either result;
  this is the same class of trap as the twenty-third round's "single-shot capture can miss a
  short-lived window" lesson, generalized to "and a stale coordinate check after it's already
  hidden looks exactly like a paint failure."

**Left in a clean, honest, committed checkpoint** (per explicit instruction, so this is a
step this project can reliably return to, not a dead end to redo from scratch): both overlays
paint using a bare `Show(owner)` with the *real* theme-matched `BackColor` (debug lime removed),
Z-order left unaddressed. The icons are, in practice, not visibly usable in this state -- most of
each overlay's area is genuinely behind the notification window. Option 1 (`UpdateLayeredWindow`,
the fallback discussed with the user up front) has not been attempted yet.

## Twenty-fifth round: Z-order fixed for the overlay window itself via raw SetWindowPos, but child controls still don't paint -- option 2 now considered exhausted

User asked to try more Z-order angles specifically before falling back to option 1. One worked,
partially:

**`SetWindowPos(HWND_TOP, SWP_NOACTIVATE)` via raw P/Invoke, called once right after `Show(owner)`,
is the first Z-order fix that did not break painting.** Unlike `TopMost` (persistently toggles the
`WS_EX_TOPMOST` extended style) and `Control.BringToFront()` (a WinForms wrapper that may do more
than a single `SetWindowPos` call internally -- e.g. also touching focus), this is the single
narrowest possible native call: one-time, no persistent style change. Confirmed live with the
debug `Color.Lime` `BackColor`: the icon overlay window visibly moved to sit above/beside the
notification instead of behind it, still painting reliably. Real progress on the specific thing
asked for.

**But a second, distinct problem surfaced once color was switched back to the real theme color:
the overlay *window* now paints (a faint but visible rectangle, matching the theme color, with a
detectable edge against its surroundings), but its *child controls* -- the two icon `PictureBox`es,
and the reparented `tableLayoutPanel1`'s own reply/flag/delete buttons -- do not paint their own
content at all.** No icon graphics, no button appearance, just the parent's flat background color
showing through where they should be. This is a new instance of the same underlying class of Wine
paint gap this whole investigation keeps finding, one layer down: fixing the *parent* window's
paint+Z-order didn't fix its *children*'s paint.

**Two more attempts, both following the project's own established idioms, neither worked:**
- `overlay.Refresh()` immediately after `Show()`+`RaiseViaSetWindowPos()` -- no effect.
- A one-shot 250ms-delayed re-assertion (`RaiseViaSetWindowPos()` + `Refresh()` + explicitly
  `Refresh()`ing every child) via a `System.Windows.Forms.Timer`, matching this project's own
  periodic-reblit idiom (a paint forced immediately after a state change can race Wine's
  compositor before it catches up, per the fourteenth/sixteenth rounds) -- no effect either.
  (Hit a real, if minor, implementation snag along the way: a blind IDE-style find-and-replace of
  the bare word `Timer` to disambiguate `System.Windows.Forms.Timer` from `System.Threading.Timer`
  also mangled the *field name* `_delayedRepaintTimer` into `_delayedRepaintSystem.Windows.Forms.
  Timer`, since it matched the substring `Timer` too -- caught immediately by the compiler error,
  fixed by hand rather than blind-replacing again. Worth remembering: a global text substitution
  is not IL-instruction-level precise the way this file's other tools are -- it can hit substrings
  inside identifiers just as easily as inside a type-name reference.)

**Option 2 considered exhausted at this point, per the user's own explicit standard set at the
start of this attempt ("try 2, if that fails we'll look at 1").** Four independent Z-order
mechanisms tried across two rounds (`TopMost`, `BringToFront()` sync, `BringToFront()` deferred via
`BeginInvoke`, `WS_EX_NOACTIVATE`) either broke painting outright or (the one exception,
`SetWindowPos`+`SWP_NOACTIVATE`) fixed the parent's Z-order but left child-control painting broken
regardless. This isn't "one wrong guess away from working" -- it's the same fundamental gap
recurring at a new layer every time a different symptom gets fixed, strong evidence that a plain
`WM_PAINT`-based approach is not going to get all the way to a working, visible, correctly-stacked
overlay under this Wine/window-manager combination. Option 1 (`UpdateLayeredWindow`, matching
`layeredWindow`'s own proven-reliable forced-blit mechanism, which sidesteps the whole "does this
window's paint message get processed under Wine" question by never depending on it) is the
recommended next step if this is picked back up.

**State left in this checkpoint:** the `SetWindowPos`+`SWP_NOACTIVATE` Z-order fix and the delayed-
repaint timer are both still in place in `MailClient.Notifications.ButtonOverlay.dll` (real,
working improvements on the Z-order front specifically, even though the overall feature isn't
usable yet) -- not reverted, since they're net positive and don't regress anything. `MailClient.dll`
itself was not rebuilt/redeployed with this round's helper-assembly-only changes (only
`MailClient.Notifications.ButtonOverlay.dll` was touched and hot-swapped for each test) -- the
`notification-button-overlay-patcher`-produced `MailClient.dll` from the twenty-fourth round's
commit is still the current wired-in version and remains valid (the interface between the two
assemblies -- `ButtonOverlayManager.Sync`/`HideAll`'s signatures -- did not change this round).

## Twenty-sixth round: Option 1 -- close/settings icons baked into `backgroundBitmap`, confirmed working live

Per the user's explicit "yes try option 1", implemented pragmatically rather than literally: instead
of building a brand-new second `UpdateLayeredWindow`-backed window from scratch, extended the
*existing* `backgroundBitmap`/`layeredWindow` mechanism that already fixed the title/content text
(see the earlier rounds) to also draw the close/settings icons. This reuses the one blit path
already proven to reach the screen reliably under Wine, instead of introducing a second one.

New IL patch, `--patch-notification-icon-bitmap`, three parts:
1. **`__drawNotificationTextIntoBitmap`** (the method that already draws title/content into
   `backgroundBitmap`) extended to also `graphics.DrawImage(mouseOverClose ? closeImageOver :
   closeImage, closeRect)` and the same for settings, guarded by `closeImage != null` /
   `settingsImage != null` null checks (see crash below for why the guards are required).
2. **`OnMouseMove`**: both existing `Invalidate()` calls (on `mouseOverClose`/`mouseOverSettings`
   state change) now also call `updateLayeredBackground(refreshBitmap: true)`, so a hover-state
   change actually gets baked into the bitmap and reblitted, not just requested via `Invalidate()`
   (which alone was already established, for title/content, as insufficient under Wine without a
   real message-loop tick).
3. **`OnPaint`**: the `mouseOver` block's direct `e.Graphics.DrawImage(closeImage/closeImageOver,
   closeRect)` calls (and the settings equivalent) removed outright -- confirmed via `--dump-il`
   that only the `backgroundBitmap`-sourced sub-rectangle redraws remain. Leaving the direct draws
   in place alongside the new bitmap-sourced ones would have painted correctly the instant `OnPaint`
   itself is Wine-processed (which the whole rest of this investigation has shown isn't reliable)
   while doing nothing to fix the actual bug -- the bitmap-only redraw is the one already proven to
   reach the screen via `layeredWindow`'s own forced `UpdateLayeredWindow` call regardless of
   whether Wine ever processes this window's own `WM_PAINT`.

**Two real bugs hit and fixed before this was safe to deploy, both caught by this project's own
established discipline rather than by a live failure (well, one of them was, the first time):**
- **Live crash, first deploy attempt, zero diagnostic output beyond two `updateBackgroundBitmap`
  calls then silence, no `bug.*.txt` crash-report file.** Diagnosed (no debugger available under
  Wine for this class of native-boundary crash) as `Graphics.DrawImage(null, rect)` throwing inside
  GDI+'s own call stack: `closeImage`/`closeImageOver`/`settingsImage`/`settingsImageOver` are only
  populated by `recolorImages()`, called from `OnLoad()`, and `updateBackgroundBitmap()` -- unlike
  the *original* code's icon-drawing, which only ever ran from a live-mouse-hover context where the
  form was necessarily already loaded -- can now run on a freshly-constructed form *before*
  `OnLoad()` completes, a new risk this patch itself introduced. Fixed with the null guards in part
  1 above. Confirmed on redeploy: a fresh launch replaying the exact same trigger sequence that
  crashed before now runs clean (see below) -- the crash-reporter's own `bug.20260903053059.txt`
  from the earlier failed attempt was inspected and confirmed to contain `ArgumentNullException`
  inside `DrawImage`, matching the diagnosis exactly.
- **Self-caught stack-balance bug in the null-guard fix itself**, before ever building it: the
  branch-target instructions for "skip the draw, image is null" (`closeSkipInstr`/
  `settingsSkipInstr`) were initially created as `Instruction.Create(OpCodes.Ldarg_0)`. Since these
  execute on *both* the fall-through-after-a-successful-draw path and the branch-in-because-null
  path, and `Ldarg_0` has a real stack effect (pushes `this`, consumed by nothing), this would have
  corrupted the evaluation stack on every single call regardless of which path was taken -- caught
  by manually tracing the stack effect of the generated IL before ever attempting a build, not by a
  build error or a live failure. Fixed by using `OpCodes.Nop` instead (zero stack effect, a pure
  jump-target marker) -- exactly what a "do nothing, just land here" branch target needs.

Also hit, in the `OnPaint` block-removal search specifically: `body.SimplifyMacros()` (called up
front on this method, per IL-patching lesson 4, since the insertion is large enough to risk a
short-branch range issue) converts the compact `Ldarg_0` opcode to the general `Ldarg` opcode with a
`ThisParameter` operand -- a pattern search anchored on `OpCodes.Ldarg_0` specifically silently
never matched (same class of bug as the button-overlay patcher's `ShowNotification` helper, earlier
this investigation). Fixed by anchoring on the `Ldfld` field-access instruction instead (a value
`SimplifyMacros()` never touches). A related smaller bug in the same search: comparing
`instrs[i+1].Operand == mouseOverField` (C# reference equality between a `FieldReference` pulled off
an existing instruction and an independently-`Resolve()`d `FieldDefinition`) doesn't reliably match
even when they name the same field -- fixed by comparing `.Name` strings instead.

**Full verification before deploy:** decompiled `__drawNotificationTextIntoBitmap` (shows both null
guards exactly as intended), `OnMouseMove` (shows both `updateLayeredBackground(refreshBitmap:
true)` calls), and `--dump-il`'d `OnPaint` directly (confirms zero remaining `closeImage`/
`settingsImage`/`closeImageOver`/`settingsImageOver` field references -- only `backgroundBitmap`
sub-rectangle redraws) -- all against the exact final chained build about to be deployed, not an
earlier pre-null-guard build. Interpolation regression scan (13/13 unchanged) and `--dump-handlers`
(nesting order correct) both clean.

**Live-tested, confirmed working, on a genuinely fresh launch (not a `cxstart` reactivation --
timed at ~8s to main-window-loaded, matching this project's own established fresh-launch signature)
against bottle 8:**
- **The crash is fixed.** App survived the full notification lifecycle -- diag log shows
  `OnPaint`/`timer_OnTimer` firing continuously from first paint through fade-out (05:43:20 through
  05:43:31, over 4000 log lines), process still running afterward, no new `bug.*.txt` file.
- **Close and settings icons are visible from the earliest captured frame of the static hold
  period (~1.7s after trigger, well before any fade-out), not just glimpsed during fade** -- the
  exact bug this whole line of investigation was chasing. Confirmed via `ffmpeg` continuous
  screen recording (15fps, correct display resolution 1897x1171 -- an earlier capture attempt used
  a stale hardcoded 1920x1080 and failed outright with "Capture area ... outside the screen size")
  and frame extraction at 3fps; both the gear (settings) and X (close) icons are clearly visible,
  consistently, across every extracted frame during the hold period, cropped and upscaled 3x for
  direct visual inspection.
- **Not live-tested this round: the hover-state icon swap** (Part 2's actual purpose). No mouse
  automation tooling exists in this environment (`xdotool` confirmed absent) to move the pointer
  onto the icons programmatically, and the test-notification dev timer
  (`--patch-test-monogram-avatar`) only fires once per app session, so re-testing costs a full
  app restart each time -- not spent this round since the primary bug (visibility during the hold
  period, and the crash) is now solidly confirmed. Part 2's code is decompile-verified correct
  (both `Invalidate()` sites now also call `updateLayeredBackground(refreshBitmap: true)`) but a
  human moving the mouse over the icons live is the next real confirmation step if pursued.
- **Not tested at all this round: reply/flag/delete.** The test notification used
  (`--patch-test-monogram-avatar`'s `FormMailNotification`) does not have a reply/flag/delete
  panel in its captured frames -- consistent with the project's own earlier note that not every
  `FormGenericNotification` subclass has one (`Controls.Find("tableLayoutPanel1", ...)` comes back
  empty for some). Reply/flag/delete was always explicitly scoped as a second, more complex pass
  (real `ControlToolStripButton` instances backed by `MultiResImageList` resources, not simple
  `Image` fields) -- not started.

**State left at this checkpoint:** `il-patches/output-icon-final/` is the fully chained, verified,
deployed build (deployed to bottle 8, hash-confirmed). The now-unused `MailClient.Notifications.
ButtonOverlay` companion assembly and its patcher (option 2, twenty-third through twenty-fifth
rounds) remain in the repo, untouched, as historical/fallback reference -- not wired into this
build. `--patch-notification-icon-bitmap` is not yet folded into `releases/<version>/deploy.sh`'s
main pipeline (still applied as a manual extra stage on top of `output-final2-final`) pending the
reply/flag/delete second pass and a live hover-swap check.

## Twenty-seventh round: title text overlapping the now-always-visible icons, fixed; dev trigger made repeatable

Two small follow-ups requested immediately after the twenty-sixth round's close/settings fix went
live, both confirmed and deployed together.

**Title overlapping the icons.** Making the icons always-visible (twenty-sixth round) exposed a
gap the app already half-anticipated but didn't fully cover: `OnPaintTitle`'s own IL already
narrows the title's clip/ellipsis width to `settingsRect.Left - Padding.Horizontal` -- but only
inside `if (mouseOver) { ... }`, matching the icons' *old* hover-only visibility. With the icons now
unconditionally drawn into `backgroundBitmap` regardless of `mouseOver`, the non-hover width branch
(full header width, no icon allowance) let `EndEllipsis`-truncated text run under the icons
whenever the mouse wasn't over the notification -- which, since the icons are now always visible,
is most of the time. New patch, `--patch-notification-title-icon-clip`: found the `Ldfld
mouseOver; Brfalse <else>` pair in `OnPaintTitle`'s raw IL and replaced the `Brfalse` with a `Pop`
(identical stack effect -- pop 1, push 0 -- so no other instruction needs touching), forcing the
`if`'s then-branch (narrow to `settingsRect.Left`) to always execute. The else-branch's own
instructions are left in place as harmless dead code (confirmed nothing else targets them before
making the change, so no retargeting per IL-patching lesson 2 was needed). Decompile confirms the
intended shape exactly (`_ = mouseOver; bounds.Width = settingsRect.Left - Padding.Horizontal;`
unconditionally). Live-confirmed via fresh launch + video capture: title now reads "Jaguar Workshop
Auto Re…" (properly ellipsized) with a clean gap before the gear/X icons, both immediately after
the notification appears and later in its hold period -- `supporting/notification-title-clips-
before-icons-fixed.png`.

**Dev trigger firing only once per app session.** `--patch-test-monogram-avatar`'s
`__monogramTestTick` called `__monogramTestTimer.Stop()` the moment it fired -- the
`File.Exists`+`File.Delete` guard already makes a single `touch` fire exactly once on its own, so
this `Stop()` was never load-bearing for correctness, just an accidental one-shot limitation (this
project's own earlier finding: "the dev-test notification timer only fires once per app session,"
flagged by the user as a likely source of false "crash" readings in an earlier round). First
attempt at the fix edited `RunPatchTestMonogramAvatar`'s generator to drop the `Stop()` call from
the instructions it emits -- correct in isolation, but re-running that generator against a base
that had *already* been through an earlier application of the same patch (this project's
`output-final2-final` checkpoint, confirmed via decompile to already carry `__monogramTestTick`)
doesn't update the existing wiring, it duplicates it: two `__monogramTestTick` methods, two
`__monogramTestTimer` fields, `OnShown` prepended twice -- caught via decompile before deploying,
not live. Fixed properly with a second, narrower patch instead, `--patch-test-monogram-repeat`:
locates the *existing*, already-baked-in `__monogramTestTick` method and removes just its
`Ldarg_0; Ldfld __monogramTestTimer; Callvirt Stop()` instruction triple (three straight-line
instructions, confirmed no branch/handler references any of them before removing). Decompile
confirms exactly one tick method remains, with no `Stop()` call. Live-confirmed: triggered the test
notification twice in the same app session (`touch` the trigger file, wait for it to fully show and
fade, `touch` again) without any app restart -- diag log's `OnPaint` line count roughly doubled
(402 -> 804) and the second notification rendered identically to the first, title clip included.

Both changes are additive, narrow IL edits on top of the twenty-sixth round's build (`--patch-
notification-title-icon-clip` and `--patch-test-monogram-repeat`, applied in that order after
`--patch-notification-icon-bitmap`, before `--patch-notification-geometry-diag`/`--patch-close-
listener`/`--patch-diag`) -- `il-patches/output-final4/` is the current fully-verified, deployed
checkpoint. `--patch-test-monogram-repeat` is dev-only (same category as `--patch-test-monogram-
avatar` itself and `--patch-close-listener` -- strip before any release build); `--patch-
notification-title-icon-clip` is a real user-facing fix and belongs in the eventual release
pipeline alongside `--patch-notification-icon-bitmap`.
