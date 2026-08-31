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

## Not yet tried

- A live X11 pixmap dump (e.g. `xwd`/`import`) precisely synchronized with the broken window's
  timeframe, to inspect the actual server-side composited pixmap content directly, rather than
  inferring from Wine's own trace log of what it *believes* it did.
- Reading Wine's `window_surface`/layered-window implementation source directly for the specific
  code path used by a `WS_EX_LAYERED` top-level window combined with WinForms'
  `OptimizedDoubleBuffer`, rather than black-box trace observation.
- Testing under a different desktop environment/window manager (non-Cinnamon) to determine
  whether this reproduces identically elsewhere, or is specific to this Mint/Muffin combination.
- Testing against a different CrossOver/Wine build to bound whether it's version-specific.
- The user separately observed the notification's sender avatar/icon appearing to draw before the
  text in an earlier manual (non-recorded) repro. Not reproduced in the recorded session analyzed
  here (the box was uniformly blank in every sampled frame during the hold, no icon visible before
  the text+fade moment) — worth another look if a future session can catch it on video, since if
  real it would suggest the icon (a `DrawImage`/bitmap blit) and the text (`ExtTextOutW`) take
  different paths to the screen, and only one of them is affected.

## Status

**Unresolved.** All experimental changes (diagnostic instrumentation, all three fix attempts) have
been reverted — `emClient_win_8_x64` is back to its confirmed-working baseline (all 7 prior patch
stages intact, nothing extra deployed). No fix is currently applied or shipped for this issue.
