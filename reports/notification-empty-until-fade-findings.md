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
easy to mis-narrate. Not yet used in a live correlated test as of this edit -- planned next.

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
- The user separately observed the notification's sender avatar/icon appearing to draw before the
  text in an earlier manual (non-recorded) repro on the real app. Not reproduced in the recorded
  session analyzed here, nor in the repro-app iteration that specifically added an avatar (sync or
  cross-thread-async) — worth another look on the real app if a future session can catch it on
  video, since if real it would suggest the icon (a `DrawImage`/bitmap blit) and the text
  (`ExtTextOutW`) take different paths to the screen, and only one of them is affected.

## Status

**Unresolved, but substantially narrowed as of this session (2026-09-01).** All experimental
changes across all three rounds this session (diagnostic instrumentation, all six fix/isolation
attempts including `--patch-notification-no-fading`, the real app instance used for the final
`xprop` check and the two focus/no-fading re-tests) have been reverted/closed out —
`emClient_win_8_x64` is back on its confirmed-working stage-7 baseline (`md5sum`-verified against
`il-patches/backup/MailClient.dll.stage7-confirmed-working`), and the app is currently closed
(quit gracefully, not force-killed). `NotificationsHideAfterTimeout` was toggled off via Settings
mid-investigation and back on again by the user before this checkpoint -- worth a quick glance at
Settings -> Notifications next session just to confirm it's still on before assuming baseline
behavior.

**Current best understanding, superseding the "wall-clock/compositor timeout" theory from earlier
in this session:** content becomes visible when `Hide()` actually runs (state -> Disappearing),
triggered either by the auto-hide timer naturally elapsing or by a user click -- not by a fixed
elapsed-time threshold as such. The fourth round's burst/silent-wait tests, which all released
around the same ~6s mark regardless of what the app was doing, are now best explained as all
indirectly reaching that same natural `Hide()` trigger rather than each release being caused by
its own mechanism -- exactly how remains unresolved (see the fifth-round section above for the
specific unreconciled timing detail in the silent-wait test). Ruled out this session: Wine's
opacity-property delete-vs-change split; Muffin's X11-Sync-extension frame-freeze; disabling the
fade animation entirely (makes it worse -- permanently invisible, confirmed not a focus-suppression
artifact); and the idea that app-side repaint/opacity/message-pump activity during the blank window
has any causal effect by itself.

**Freshest, most actionable lead (not yet investigated) -- resume here next:** clicking a
no-text notification (auto-hide off) hangs the app if clicked *after* ~6 seconds have elapsed,
but not if clicked within that window (reproduced 2/2). This is cleanly reproducible without any
patch (just the existing Settings toggle + click timing) and points directly at a likely race
between the click handler and whatever fires at the `timeToStay` mark. Planned next step, agreed
with the user before this checkpoint: instrument `Hide()`, `timer_OnTimer`, and the notification's
click handler with `--patch-diag`-style logging to see exactly what collides, rather than
continuing to infer from screen recordings alone.
