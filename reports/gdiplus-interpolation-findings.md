# GDI+ `InterpolationMode` rendering gap — consolidated findings

> **Supersedes** `reports/splash-icons-findings.md` and `reports/settings-panel-findings.md`. Those two investigations turned out to be two symptoms of one root cause; this report folds both in, adds a full-codebase scan for the same defect, and lays out a fix path. The original two files are left in place for their fuller narrative/trace excerpts but should be read as historical detail, not the current plan.

> **2026-08-28 correction #1, fresh bottle `emClient_win_7_x64`:** Stage 1 was originally patched to `HighQualityBilinear` (6), on the strength of row #11 below ("already relied on elsewhere without issue"). That was wrong. A live trace of the patched build (`reports/cxlog.txt`) shows Wine's `resample_bitmap_pixel` throws `Unimplemented interpolation 6` at the exact same splash-screen draw that used to throw `Unimplemented interpolation 7` — mode 6 is just as unimplemented as mode 7 for an actual scaled `DrawImage`. Row #11's "proof" doesn't hold up: `SettingImages.CreateImages` sets mode 6 but every `DrawImage` call in that method uses a `destRect` identical in size to the source rect, so it never triggers a real resample regardless of interpolation mode — it was never a live test of mode 6 at all.
>
> **2026-08-28 correction #2:** The obvious next guess — `InterpolationMode.High` (2), on the strength of a live trace showing a genuinely-scaled avatar draw complete with no fixme — was *also* wrong, and for a subtle reason: a second trace of that patch showed the exact same splash draw fail again, but logged as `Unimplemented interpolation 7`, not `2`, even though the code sets `graphics.InterpolationMode = InterpolationMode.High` and `GdipSetInterpolationMode` logs receiving `2` verbatim. Trace-based empirical testing turned out to be unreliable here — the earlier "success" was a false signal, not evidence mode 2 works. To settle it for real, `/opt/cxoffice/lib/wine/i386-windows/gdiplus.dll` was disassembled directly (`resample_bitmap_pixel`, entry at file-relative `0x1005d820`): its dispatch logic is `cmp $3,mode; je <real code>` / `cmp $5,mode; je <real code>` / **anything else falls through to the "Unimplemented interpolation %i" fixme**. Only `Bilinear` (3) and `NearestNeighbor` (5) have real implementations; `Default`(0), `Low`(1, though it aliases to 3 internally so is *effectively* safe), `High`(2), `Bicubic`(4), `HighQualityBilinear`(6), and `HighQualityBicubic`(7) all hit the stub. `High`(2) specifically gets normalized to `HighQualityBicubic`(7) internally before reaching this function (matching real GDI+'s documented legacy-alias behavior) — which is why the fixme reported "7" for code that set "2".
>
> Stage 1 has been re-patched to `InterpolationMode.Bilinear` (3) — the only value with disassembly-confirmed ground truth, not trace-inferred guesswork — on both confirmed sites. The "Fix path" and inventory table below are left as originally written for history, but treat **mode 3, not 6 or 2,** as the actual target for any future stage. Rows #12–13 (`InterpolationMode.High` = 2) are now confirmed **broken**, not merely "no evidence" — update the Hold reasoning accordingly if revisiting those sites. If disassembly is available next time, prefer it over trace-based inference from the start; it's cheaper and more reliable than it looks.
>
> **2026-08-28 correction #3 — the Settings panel was never this bug.** With the splash screen genuinely fixed (mode 3, confirmed clean by both disassembly and a fixme-free trace), the Settings dialog's left category panel is *still* completely blank. The link between the two bugs, asserted in the original consolidated report below, was always a hypothesis (`reports/settings-panel-findings.md` says so explicitly — it was never live-trace-confirmed) and it was wrong. The actual cause is a Wine clip-region bug in `BeginPaint`/`WM_NCPAINT` handling, unrelated to GDI+ image resampling and not fixable by patching MailClient. Full writeup: `reports/settings-panel-clip-region-findings.md`.

## Root cause

Wine's `gdiplus` does not implement `InterpolationMode.HighQualityBicubic` (enum value `7`). When app code sets `Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic` and then draws a scaled bitmap, CrossOver logs:

```
fixme:gdiplus:resample_bitmap_pixel Unimplemented interpolation 7
```

and continues without throwing — it does **not** crash, it silently produces bad/incomplete pixel data for that draw call. High-frequency detail (icon glyphs, small artwork) degrades to blank or garbled; large low-frequency areas (background gradients) can look fine, which is why the symptom reads as "some icons missing" rather than "the whole image is broken."

Confirmed live via traced launch:
```
CX_DEBUGMSG="+gdiplus,+font,+seh" /opt/cxoffice/bin/wine --bottle "emClient_win_10_x64" --cx-app MailClient.exe
```
(Note: `CX_DEBUGMSG=help` does **not** enumerate channel names on this CrossOver build — it only prints the generic `err|warn|fixme|trace` class syntax. Channel names had to be confirmed by grepping Wine's own libraries, e.g. `strings /opt/cxoffice/lib/wine/x86_64-unix/win32u.so` for `font`/`win32u` — "gdi" was folded into `win32u` in this Wine version. Keep this in mind for any future CX_DEBUGMSG-based investigation.)

## Full inventory (whole-app scan)

The two originally-reported bugs (splash screen, Settings panel) both trace back to code using `InterpolationMode.HighQualityBicubic`. Rather than assume those were the only two call sites, I extended `~/tools/il-patcher` (previously an empty stub) into a Mono.Cecil scanner that walks every method body in every DLL under `original/` and reports every `Graphics.InterpolationMode = ...` set-site by its actual numeric value — this can't be found with `strings`, since the C# compiler emits a numeric IL literal (`ldc.i4.7`), not a text reference to the enum name.

**13 set-sites total, across 3 assemblies:**

| # | Assembly | Method | Mode | Status |
|---|---|---|---|---|
| 1 | MailClient.Common.UI.dll | `CommonPaintUtils.ResizeImage` (lambda) | **Bicubic (7)** | ✅ Confirmed by trace — feeds Settings panel icons |
| 2 | MailClient.dll | `UI.Forms.FormSplashScreen.OnPaintBackground` | **Bicubic (7)** | ✅ Confirmed by trace — the reported splash bug |
| 3 | MailClient.Common.UI.dll | `Controls.PictureBoxEx.OnPaint` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed by trace |
| 4 | MailClient.Common.UI.dll | `Controls.ControlDataGrid.ControlDataGrid.DrawNoItems` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed — notable: inside the exact grid class behind the Settings panel bug |
| 5 | MailClient.dll | `UI.Forms.formAbout.OnPaintBackground` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed — About dialog banner, same code shape as splash |
| 6 | MailClient.dll | `UI.Forms.formDataAccounts.panel_Details_Paint` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed |
| 7 | MailClient.dll | `UI.Controls.ControlItemCardsWithImage.DrawItemForeground` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed |
| 8 | MailClient.dll | `Avatar.DesktopAvatarManager.TryResizeAndSaveBitmap` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed — contact avatar resize |
| 9 | MailClient.dll | `Avatar.ImageHandler.TryTranscodeAvatarAsPngOfOptimalSize` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed |
| 10 | QRCoder.dll (vendored) | `QRCode.GetGraphic` | **Bicubic (7)** | ⚠️ Same pattern, unconfirmed — feeds the Settings QR-export feature |
| 11 | MailClient.dll | `UI.SettingImages.CreateImages` | HighQualityBilinear (6) | ✅ Already safe — proof mode 6 is already relied on elsewhere in this codebase |
| 12 | MailClient.dll | `UI.UIAvatar.GetImageSingleRes` | High (2) | ❓ Different value, not trace-confirmed either way |
| 13 | QRCoder.dll (vendored) | `QRCode.ArtQRCode.Resize` | High (2) | ❓ Different value, not trace-confirmed either way |

Rows 3–10 (8 sites) are mechanically identical to the two confirmed ones — same `ldc.i4.7` → `set_InterpolationMode` shape — just not individually reproduced against a live trace, since none of them were reported as bugs yet. Rows 12–13 use a different enum value (`High` = 2) that real GDI+ treats as a bicubic-family alias, so it's plausible but unproven that Wine hits the same gap there.

## Bug detail (from the superseded reports)

**Splash screen icons** — `FormSplashScreen.OnPaintBackground` draws `BrandingUtils.SplashScreen`, a plain embedded raster bitmap in `MailClient.dll`'s own `Branding` resx resource, via `Graphics.DrawImage` with `InterpolationMode.HighQualityBicubic`. The original CLAUDE.md lead (SVG rendering via `Svg.dll`/`IconsLibrary.dll`) is ruled out: `IconsLibrary.dll` has no managed resources at all — `objdump -p` shows its `.rsrc` is a native Win32 table of 184 `RT_ICON` entries (classic `.ico` resources, no SVG), and the spinner (`ControlWaiting`) is hand-drawn with `Graphics.DrawArc`, no image involved.

**Settings panel empty** — `formSettings` (`MailClient.dll`, not the `.Settings`/`.Settings.Client` DLLs, which are pure data/schema with no UI code) populates its left nav via a static, compile-time `Dictionary<string, PanelDefinition>` plus 45 designer-added `SwitchPanel` children of a `ControlPanelSwitcher` — not reflection, not a runtime resource load. The left-panel control itself, `dataGridCategory`, is a `ControlDataGrid` (a large fully custom owner-drawn/virtualized grid, also used for the mail list/contacts elsewhere in the app). Every row/group icon it needs is resolved through `ImageList.GetSuitableImageForSize(...)` → `CommonPaintUtils.ResizeImage`, i.e. call site #1 above — the same helper implicated in the splash bug. Row #4 above (`ControlDataGrid.DrawNoItems`) is a second, independent Bicubic(7) call site inside this same grid class, discovered only by the full scan.

**Not yet confirmed live:** I couldn't drive the Settings dialog open in this sandbox — no `xdotool`/`ydotool`/`xte` input-automation tooling is present (and there's no package-manager access to install one), and screenshot capture (`gnome-screenshot`, `PIL.ImageGrab`) returns a fully black frame for this Wine build's window surface regardless of what's actually drawn (the CEF renderer separately logs `Exiting GPU process due to errors during initialization` — unrelated to these bugs, but it explains why screen-scraping doesn't work here either). A human driving the UI with the same `CX_DEBUGMSG="+gdiplus,+font,+seh,+heap"` trace, opening Settings, would confirm rows #1 and #4 fire at that moment.

## Fix path

The mechanical fix is identical at every Bicubic(7) site: change the interpolation mode to one Wine actually implements. `HighQualityBilinear` (6) is the safest choice — it's the closest visual match, and row #11 shows this codebase already relies on it elsewhere without issue.

**This does not need ten separate source edits.** All 10 Bicubic(7) sites reduce to the same one-instruction IL change (`ldc.i4.7` → `ldc.i4.6` immediately before `Graphics::set_InterpolationMode`), so the right tool is a small Cecil patch script (a natural extension of the scanner already written into `~/tools/il-patcher`) that rewrites all matching sites in a DLL and saves it back out — not a decompile→hand-edit→recompile cycle per call site. This sidesteps the concern flagged in the original splash report, that `MailClient.dll` (28MB) is unlikely to round-trip cleanly through `ilspycmd -p` + rebuild — the IL patch never needs it to.

That said, the 13 sites don't all carry the same evidence or the same risk, so I'd still stage the work rather than flip all 13 in one commit:

### Stage 1 — Patch the two confirmed sites
Rows #1 and #2. These have direct trace evidence tied to the two bugs the user actually reported. Patch, rebuild the bottle, re-test the splash screen and Settings panel against the user's reference screenshots. Highest confidence, highest user-visible payoff, smallest blast radius (one call site each, both already fully understood).

### Stage 2 — Patch the remaining same-pattern in-house sites
Rows #3–9 (`MailClient.Common.UI.dll` and `MailClient.dll` only). Mechanically identical fix, same tool, same commit shape as Stage 1 — there's no reason to run the patcher three separate times for these. What makes this its own stage is *verification*: none of these were reported as bugs, so after patching, each surface (a generic `PictureBoxEx` consumer, `ControlDataGrid.DrawNoItems`'s empty-state view, the About dialog, `formDataAccounts`'s details panel, card-list items, avatar resize/save, avatar-to-PNG transcode) needs an independent visual spot-check rather than a single before/after comparison. Do this once Stage 1 is confirmed good, so a problem here can't be confused with a regression in the two known bugs.

### Stage 3 — Vendored `QRCoder.dll` (row #10)
Same pattern, same fix, but it's a third-party binary rather than in-house code, and it only affects one minor, rarely-used feature (Settings → QR export). Worth a separate go/no-go: patch it the same mechanical way, or leave it and accept that one edge case for now. I'd treat this as a smaller, optional follow-up rather than bundling it into Stage 1/2.

### Hold — do not patch without new evidence
Rows #12–13 (`InterpolationMode.High`, value 2). No trace evidence either way that Wine's `gdiplus` fails on this value — changing them would be a speculative behavior change with nothing to justify it yet. Leave as-is; if a future trace on `UIAvatar.GetImageSingleRes` or `QRCoder.ArtQRCode.Resize` shows a `resample_bitmap_pixel` (or equivalent) fixme, promote to a stage of its own then.

### Verification tooling
Re-run the scanner (`dotnet ~/tools/il-patcher/bin/Release/net10.0/il-patcher.dll <dir-of-patched-dlls>`) after each stage — it should report the patched sites as `HighQualityBilinear` and nothing left at `Bicubic(7)` for whichever DLLs were touched in that stage. This gives a cheap, mechanical pass/fail check independent of visual inspection, which matters here given screenshot capture is unreliable in this sandbox.
