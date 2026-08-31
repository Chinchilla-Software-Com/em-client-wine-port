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

**Unresolved.** All experimental changes (diagnostic instrumentation, all three fix attempts, the
real app instance used for the final `xprop` check) have been reverted/closed out —
`emClient_win_8_x64` is back to its confirmed-working baseline (all 7 prior patch stages intact,
nothing extra deployed). No fix is currently applied or shipped for this issue. Two specific,
well-evidenced hypotheses (Wine's opacity-property delete-vs-change split; Muffin's X11-Sync-
extension frame-freeze) have been directly tested and ruled out this round, narrowing the
remaining search space to Muffin's texture/damage-tracking code specifically, or to something
about the real app's process complexity that a minimal repro hasn't yet replicated.
