# Settings dialog left panel blank — actual root cause: Wine clip-region bug, not GDI+ interpolation

> **Supersedes the Settings-panel portion of `reports/settings-panel-findings.md` and
> `reports/gdiplus-interpolation-findings.md`.** Both of those treated the blank Settings
> panel as the same bug as the splash-screen icon corruption (shared `InterpolationMode`
> root cause). That link was never actually confirmed live — the original investigation
> couldn't drive the Settings dialog open — and it turned out to be wrong. With the
> splash-screen `InterpolationMode` bug now genuinely fixed (`InterpolationMode.Bilinear`,
> confirmed by disassembly and a fixme-free trace), the Settings panel is still blank. The
> real cause, found below, is unrelated to GDI+ image resampling entirely and cannot be
> fixed by patching `MailClient`'s IL.

## Root cause

Wine's `BeginPaint`/`WM_NCPAINT` handling for `dataGridCategory` (a borderless owner-drawn
`ControlDataGrid`, HWND `0x102fc` in the trace below) computes an empty clip region for the
control's *client-area* paint, because it reuses the same region object across two
conceptually distinct steps that should not share state:

1. **Non-client region step** (`WM_NCPAINT`, sent internally as part of dispatching
   `WM_PAINT`): Wine computes `window_region DIFF client_region` to find the non-client
   (border/frame) area that needs repainting. For this control, window rect and client rect
   are identical (`(505,204)-(735,772)`, a plain borderless child — no `WS_BORDER`/caption),
   so `A DIFF A = ∅`. This is *correct* — there's genuinely no non-client area to paint —
   and Wine stores that empty result in region object `0xec4cd570`.
2. **Client region step** (immediately after, setting up the DC clip for the actual
   `WM_PAINT`/`OnPaint` call): Wine then does `RGN_AND` (intersect) using the *same* region
   object `0xec4cd570` — still holding the empty result from step 1 — as one of the inputs,
   instead of a fresh region derived from the client rect. `∅ AND anything = ∅`, so the
   client-area clip region collapses to `(0,0)-(0,0)`.

The control's `OnPaint` handler still runs, still issues real drawing calls (text, group
headers, icons), but every one of them is clipped to an empty rectangle by the OS, so
nothing is actually written to the screen. The panel background itself is also empty
because `ControlDataGrid.OnPaintBackground` is a near-no-op (see below), so the result is a
fully blank area — no icons, no group headers, no row text — matching exactly what's seen
in `supporting/settings-broken.png` and `supporting/settings-still-broken.png`.

This is a genuine Wine/CrossOver window-management bug, not a MailClient defect. The C#
code that builds and populates the category list (`formSettings.loadCategories()` /
`ReloadCategories()`) was checked in full and is completely platform-neutral — no
OS-version checks, no silently-empty data source, nothing that would behave differently
under Wine.

## Trace evidence

Captured with `CX_DEBUGMSG="+region,+clipping,+bitblt,+driver,+message,+font,+seh"` (see
`reports/cxlog-*.txt.bak` for the raw capture this was extracted from — not committed,
regenerate by rerunning under trace per `CLAUDE.md`).

The Settings window (`0x201f2`, 912×646 at (504,127) — matches
`supporting/settings-working.png`'s proportions) is created, and its left-panel grid gets
its own child HWND `0x102fc`, correctly sized 230×568 at screen position (505,204)-(735,772)
— confirmed via its `WM_NCCALCSIZE` dump. So the control is *not* zero-sized; this is not a
DPI/layout bug.

`0x102fc` receives exactly one `WM_PAINT`. Sequence (line numbers refer to the capture this
was extracted from):

```
WM_PAINT dispatched
  NtGdiGetRgnBox 0x75040139 (505,204)-(735,772)         # update region: correct, full-size
  WM_NCPAINT sent from self
    NtGdiCreateRectRgn 505,204-735,772  -> 0x76040139     # window rect
    NtGdiCombineRgn 0x130400e2,0x76040139 -> 0x130400e2 mode=4   # RGN_DIFF
      src1: (505,204)-(735,772) 1 rects   [window]
      src2: (505,204)-(735,772) 1 rects   [client -- identical, no border]
      dest (0xec4cd570): (0,0)-(0,0) 0 rects              # correctly empty: no NC area
    set_visible_region ... 0x130400e2 ...
    NtGdiGetAppClipBox => (0,0)-(0,0)                      # WM_NCPAINT's own DC: fine, nothing to draw
  WM_NCPAINT returned
  NtGdiCombineRgn 0x77040139,0x5c04010f -> 0x77040139 mode=1     # RGN_AND
    src1 (0xec4cd570): (0,0)-(0,0) 0 rects    <-- SAME region object, still empty from step above
    src2: (505,204)-(735,772) 1 rects
    dest (0xec4cd570): (0,0)-(0,0) 0 rects    <-- empty AND anything = empty
  set_visible_region ... 0x77040139 ...
  NtGdiGetAppClipBox 0x15010040 => (0,0)-(0,0)             # THIS is now the client paint's clip -- wrong!
WM_PAINT returned
```

No `NtGdiExtTextOut`/`TextOut` and no `NtGdiStretchBlt` calls occur anywhere inside this
`WM_PAINT` — consistent with every draw call being silently clipped away rather than any
draw call failing outright (which is why there's no fixme, no exception, nothing to catch —
just silence).

Contrast: the same trace shows other, non-owner-drawn controls (e.g. the right-hand
settings-page content, which does render — see `supporting/settings-still-broken.png`)
painting successfully, so this isn't universal — it's specific to whatever code path this
particular control's `WM_PAINT`/`WM_NCPAINT` combination triggers in Wine. `ControlDataGrid`
does **not** set `ControlStyles.AllPaintingInWmPaint`; it only sets `UserPaint`,
`ResizeRedraw`, `Selectable`, `StandardClick|StandardDoubleClick` (see
`Controls.ControlDataGrid.ControlDataGrid`'s constructor). On real Windows this doesn't
matter (`BeginPaint`'s region math is correct there), but it likely changes which internal
.NET WM_PAINT/WM_ERASEBKGND code path gets taken, which may be why this control hits the
buggy Wine region-reuse path when apparently-similar controls elsewhere in the app don't.

## Why this can't be fixed by patching MailClient

The bug is in Wine's region/clipping bookkeeping between `WM_NCPAINT` and the client-area
paint setup inside `BeginPaint` — code that lives in Wine's `win32u.dll`
(`/opt/cxoffice/lib/wine/i386-windows/win32u.dll` and its unix-side `win32u.so`), not in
`MailClient.dll`/`MailClient.Common.UI.dll`. No amount of IL-level editing of the app
changes what Wine's window manager does with clip regions during paint dispatch.

`~/.cxoffice/cxfixes.xml` (CrossOver's built-in per-app compatibility-fix database) was
checked for an existing eM Client entry that might already work around this — there isn't
one.

## Update: the clip-region bug is real but was a red herring for this symptom

`ControlDataGrid.initialize()` was patched to add `SetStyle(ControlStyles.AllPaintingInWmPaint, true)`
(inserted via a new `--patch-allpaintinginwmpaint` mode in `~/tools/il-patcher`, real IL
instruction insertion rather than an operand rewrite — see `il-patches/il-patcher-Program.cs`).
A re-trace with the same channels confirmed this **did** fix the clip-region bug: both
`WM_PAINT` events for `dataGridCategory` now show the correct non-empty client clip
(`(0,0)-(230,568)`, previously `(0,0)-(0,0)`) and a real `StretchBlt` of the fully-rendered
buffer to the screen. The mechanism worked exactly as hypothesized.

**The panel was still blank anyway.** That ruled out the clip-region bug as *the* cause of
this particular symptom — it's a real, separately-triggerable Wine bug, but not the reason
Settings never shows content. Something else was still wrong.

## The actual cause: a stale paint, never superseded

Rather than keep guessing from GDI trace noise, the app was instrumented directly: a new
`--patch-diag` mode in `~/tools/il-patcher` inserts logging into `ControlDataGrid.doPaint()`,
`drawCellsWithGrouping()`, and `handleVisibleItemsChange()` that writes plain text to
`Z:\tmp\claude-diag.log` (== `/tmp/claude-diag.log` on the Linux side — readable directly, no
`CX_DEBUGMSG` trace needed).

This showed, unambiguously: **`dataGridCategory` (identified by `Control.Name`) paints exactly
once during a whole Settings session, and at that one paint `columns.Count == 0`.**
`ControlDataGrid.doPaint()` has an early-exit branch:

```csharp
else if (columns.Count == 0 && !ownerDraw) { g.FillRectangle(brushBackColor, ...); }
```

which fires — filling the background and returning, never calling `drawCells()` — because this
paint happens *before* `formSettings.loadCategories()` has added its three columns
(`dataGridCategory.Columns.Add(...)` ×3). No further paint ever arrives afterward to redraw it
correctly once the columns (and the ~40 category rows, grouped) actually exist. This is neither
the interpolation bug nor the clip-region bug — it's a single stale paint slipping through
during setup that nothing ever supersedes. (`refreshingCachedItemList`, the flag guarding
`drawCells()`'s other branch, was also checked and ruled out: ENTER/EXIT logging around
`handleVisibleItemsChange()` showed it always correctly resets, 54/54 across a session — no
exception is leaking there.)

## Fix

`MailClient.dll`, `UI.Forms.formSettings::formSettings_Load` — insert
`dataGridCategory.Refresh();` immediately after the existing `dataGridCategory.EndUpdate();`
call, via a new `--patch-settings-refresh` mode in `~/tools/il-patcher`. This forces one more,
unconditional repaint at the exact point where `loadCategories()` has already finished and the
grid is guaranteed fully populated — superseding whatever premature paint happened earlier,
regardless of why it happened. Not yet visually confirmed working.
