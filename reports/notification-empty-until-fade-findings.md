# New-mail notification toast shows empty until it starts to fade out — UNRESOLVED

> Extensively investigated across multiple rounds (direct app instrumentation, two independent
> fix attempts, three separate trace configurations, a desktop-compositor test, and pixel-level
> screenshot analysis). Every mechanism checked is confirmed working correctly. No root cause
> found. Documented in full so this doesn't get re-investigated from scratch, and so the next
> session has a clear list of what's already ruled out.

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
first time. That's a real gap between "GDI did the work" (confirmed, down to individual glyphs)
and "the screen shows it" — narrowed considerably from where the investigation started, but without
a specific mechanism identified.

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

## Status

**Unresolved.** All experimental changes (diagnostic instrumentation, both fix attempts) have been
reverted — `emClient_win_8_x64` is back to its confirmed-working baseline (all 7 prior patch
stages intact, nothing extra deployed). No fix is currently applied or shipped for this issue.
