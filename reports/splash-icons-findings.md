# Splash screen: two icons fail to render

> **Superseded by `reports/gdiplus-interpolation-findings.md`**, which folds this investigation together with the Settings panel bug (same root cause) and a full-codebase scan for other affected call sites. Kept here for the fuller trace excerpts. Treat the consolidated report as the current plan.

## Responsible assembly / method

- **Assembly:** `MailClient.dll` (the 28MB main app assembly — not IconsLibrary.dll or Svg.dll)
- **Class:** `MailClient.UI.Forms.FormSplashScreen`
- **Method:** `FormSplashScreen.OnPaintBackground(PaintEventArgs e)`
- **Image source:** `MailClient.BrandingUtils.SplashScreen` → `MailClient.Branding.eM_Client.Branding.splashscreen` / `.splashscreen_dark` (light/dark variants), which are plain **embedded raster bitmaps** compiled into `MailClient.dll`'s own resx-generated `Branding` resource class (`MailClient.Branding.eM_Client.Branding.resources`), decoded as ordinary `System.Drawing.Bitmap` objects.

The rotating tip text under the spinner (source of the "Custom theme - create your own theme" string used as the initial lead) comes from a *different* resx blob, `MailClient.Resources.UI.Form.SplashScreenHints.resources` (keys `CustomTheme` / `__CustomThemeLink`), read in the `FormSplashScreen` constructor. It's just one of several randomly-picked hint strings shown at the bottom of the splash — confirms this string is genuinely splash-screen code, not a red herring, and pins the control down to `FormSplashScreen`.

## SVG hypothesis: ruled out

The working assumption in CLAUDE.md — that the missing icons are SVG assets rasterized via `Svg.dll`/`IconsLibrary.dll` — does **not** hold for this form:

- `IconsLibrary.dll` (2.7MB) has essentially no managed code (`ilspycmd -l c` shows only `<Module>`/`ThisAssembly`) and **no embedded .NET resources at all** (`--list-resources` returns 0 entries). `objdump -p` shows its `.rsrc` section is a native Win32 resource table of **184 `RT_ICON` entries** (classic multi-resolution `.ico` icon-pack resources), not SVG XML. Confirmed no `<svg` markup anywhere in the file via `strings`.
- `Svg.dll` does contain real `Svg.SvgDocument`/`SvgVisualElement`/etc. classes, so SVG rendering genuinely exists somewhere in the app — just not in `FormSplashScreen`.
- `FormSplashScreen`'s own controls draw everything via plain GDI+ primitives: the spinner (`MailClient.Common.UI.Controls.ControlWaiting`) is hand-drawn with `Graphics.DrawArc`/`FillRectangle` (no image at all), and the banner is a single `Graphics.DrawImage()` call against the embedded bitmap.

**Update CLAUDE.md's lead:** IconsLibrary.dll/Svg.dll are not implicated in this bug. The splash banner is a static bitmap resource scaled with GDI+ interpolation — see below.

## Confirmed Wine/CrossOver gap (from traced run)

Ran under CrossOver with:
```
CX_DEBUGMSG="+gdiplus,+font,+seh" /opt/cxoffice/bin/wine --bottle "emClient_win_10_x64" --cx-app MailClient.exe
```
(`CX_DEBUGMSG=help` does **not** enumerate channel names in this CrossOver build — it only prints the generic `WINEDEBUG` class syntax (`err|warn|fixme|trace`) and otherwise gets silently appended to CX's baseline channel set. Channel names had to be confirmed by grepping the Wine unix-side libraries instead: `strings /opt/cxoffice/lib/wine/x86_64-unix/win32u.so` shows `font`/`win32u` (the "gdi" channel was folded into `win32u` in this Wine version); `gdiplus` lives in its own module and channel as expected. Worth recording this for future CX_DEBUGMSG-based investigations.)

During startup (before the main window appeared), the trace shows exactly one relevant fixme, arriving right after a 0.5-scale transform is set up for a bitmap blit:

```
4757.079:07b8:07fc:trace:gdiplus:GdipBitmapUnlockBits (00EE79B0,005BD858)
4757.079:07b8:07fc:trace:gdiplus:GdipSetMatrixElements (005BD8B0, 0.50, 0.00, 0.00, 0.50, 1.00, 1.00)
4757.079:07b8:07fc:trace:gdiplus:GdipInvertMatrix (005BD8B0(0.50,0.00,0.00,0.50,1.00,1.00))
4757.079:07b8:07fc:fixme:gdiplus:resample_bitmap_pixel Unimplemented interpolation 7
```

`7` is `System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic` — and `FormSplashScreen.InitUI`/`OnPaintBackground` sets exactly that mode before drawing the banner:

```csharp
protected override void OnPaintBackground(PaintEventArgs e)
{
    ...
    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
    Rectangle rectangle = new Rectangle(new Point(0, 0), image.Size);
    Rectangle dst = new Rectangle(borderSize, borderSize, base.Width - 2 * borderSize, base.Height - 2 * borderSize);
    e.Graphics.DrawImage(image, rectangle.ResizeToFit(dst).Center(dst), rectangle, GraphicsUnit.Pixel);
}
```

Wine's `gdiplus` (`resample_bitmap_pixel`) has no implementation for interpolation mode 7. When it hits this path it does not throw or crash — the trace shows the app continues straight into the main window — but the resampled output is wrong/incomplete for that draw call. High-frequency detail in the source bitmap (small icon glyphs baked into the banner artwork) is exactly the kind of content that degrades to blank/garbled under a failed high-quality resample, while large low-frequency areas (background gradient) can look visually fine — which matches "two icons fail to render" while the rest of the banner looks acceptable.

## Proposed fix

Avoid the unimplemented Wine/GDI+ code path entirely by not requesting `HighQualityBicubic` on this draw:

- In `FormSplashScreen.OnPaintBackground` (and anywhere else in the same class using it), change:
  ```csharp
  e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
  ```
  to `InterpolationMode.HighQualityBilinear` (mode 6) or plain `Bicubic` (mode 4). No other Wine `gdiplus` fixmes for interpolation modes were observed in this trace, so either should sidestep the gap; `HighQualityBilinear` is the closer visual match and the safer first try.
- Per the project workflow: try editing `decompiled/MailClient.dll`'s `FormSplashScreen.cs` and rebuilding first. If `MailClient.dll` doesn't round-trip cleanly through ilspycmd/rebuild (likely, given its size and heavy WinForms-designer content), fall back to an IL patch (`il-patches/`) that locates the `ldc.i4.7` / `callvirt set_InterpolationMode` call site inside `FormSplashScreen::OnPaintBackground` and rewrites the operand to `6`.
- Two secondary, unrelated fixmes worth a follow-up look but not blocking this bug: `fixme:gdiplus:GdipGetLineSpacing ignoring style` and `fixme:font:find_matching_face Untranslated charset 255` / `get_nearest_charset ... NotoKufiArabic-Regular.ttf` — font-substitution noise from Wine's font matcher scanning system fonts, unrelated to the icon glyphs.

## Verification note

Screenshot capture (`gnome-screenshot`) of the live app in this sandbox returned a fully black window surface for both the splash and the main window — this looks like a capture-tool limitation with this Wine build's rendering surface (the CEF renderer process also logged `Exiting GPU process due to errors during initialization`, unrelated to this bug), not a reproduction of the reported icon glitch. Visual confirmation of the fix should be done against the user's real screenshots.
