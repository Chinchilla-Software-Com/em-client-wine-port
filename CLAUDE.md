# MailClient — CrossOver stabilization (not a native Linux port)

- original/**      — untouched, read-only
- decompiled/**     — ilspycmd -p output. If it rebuilds clean, edit here, swap the DLL in.
- il-patches/**      — Cecil scripts (~/tools/il-patcher) for assemblies that don't
                        round-trip cleanly from decompiled/
- reports/crossover-backlog.json — one entry per CONFIRMED freeze point
                                    (cxlog + VS thread stacks), not a static API scan
- reports/cxlog.txt — CX_DEBUGMSG trace, most recent run

Bottle name: emClient_win_7_x64. Exe: MailClient.exe.

Test loop: run under CrossOver; check the backlog's target fixme/err disappears
with no new freeze.

Before any edit: git commit a checkpoint.

## Current investigation: GDI+ InterpolationMode rendering gap

Root cause found and documented — search reports/*.md for "InterpolationMode" for
the full findings report. Wine's gdiplus does not implement
InterpolationMode.HighQualityBicubic (7); affected draws silently produce
blank/garbled output rather than throwing, which is why this reads as "some icons
missing" rather than a crash.

A full-codebase Cecil scan (in ~/tools/il-patcher, extended from an empty stub —
reuse it, don't rebuild it) found 13 call sites across 3 assemblies using this mode.

Status:
- Stage 1 (2 confirmed sites: CommonPaintUtils.ResizeImage, FormSplashScreen.OnPaintBackground)
  — in progress
- Stage 2 (7 same-pattern, unconfirmed-by-trace in-house sites) — not started
- Stage 3 (1 site in vendored QRCoder.dll) — not started, optional
- Hold (2 sites using InterpolationMode.High, value 2) — no trace evidence either
  way; leave as-is unless new evidence appears

Fix mechanism: single-instruction IL patch (ldc.i4.7 → ldc.i4.6 immediately before
Graphics::set_InterpolationMode) via the Cecil patcher — not a decompile/recompile
per site.
