# Settings dialog: left navigation panel empty

> **Superseded by `reports/gdiplus-interpolation-findings.md`**, which folds this investigation together with the splash screen bug (same root cause) and a full-codebase scan for other affected call sites. Kept here for the fuller trace excerpts. Treat the consolidated report as the current plan.

## Responsible assembly / method

- **Assembly:** `MailClient.dll` (not `MailClient.Settings.dll` / `MailClient.Settings.Client.dll` — those two decompiled cleanly and turned out to be pure data-layer: setting *values*, enums, and XSD schemas for the settings store, with no UI code at all. The filenames were a reasonable guess in CLAUDE.md but the UI lives in the main app assembly.)
- **Form:** `MailClient.UI.Forms.formSettings`
- **Left-panel control:** `dataGridCategory`, a `MailClient.Common.UI.Controls.ControlDataGrid.ControlDataGrid` — eM Client's own large, fully custom owner-drawn/virtualized grid control (also used for the mail list, contacts, etc; the decompiled class alone spans 1000+ compiler-generated display classes, so it wasn't fully decompiled — see "Not yet confirmed" below).
- **Category source control:** `controlPanelSwitcher`, a `MailClient.Common.UI.Controls.ControlPanelSwitcher.ControlPanelSwitcher`, whose children are `SwitchPanel` instances (`tabGeneral`, `tabRead`, `tabThemes`, … one per settings page, 45 total).

## How the panel populates itself

**Not reflection, not a runtime resource load.** It's a static, compile-time list:

1. `InitializeComponent()` adds every `SwitchPanel` (`tabGeneral`, `tabProfiles`, `tabLanguage`, …) to `controlPanelSwitcher.Controls` unconditionally — hardcoded designer calls, e.g. `this.controlPanelSwitcher.Controls.Add(this.tabGeneral);` for all 45 tabs.
2. A private field `panels` (`Dictionary<string, PanelDefinition>`) maps each tab name to a `PanelDefinition` record `(Type, Func<Control> Create, DockStyle, SearchProvider, ImageList)` — also a hardcoded literal dictionary of all 45 settings pages (`ControlSettingsGeneral`, `ControlSettingsThemes`, …), each pointing at its `MultiResImageResources.*` icon.
3. On `formSettings_Load`, `loadCategories()` runs:
   - `localizeCategoryGroups()` sets each `SwitchPanel.Tag` to its category name (Mail/General/Calendar/Contacts/…).
   - `refreshCategoryImages()` builds the group-header icon map.
   - `ReloadCategories()` walks `controlPanelSwitcher.Panels` (the 45 `SwitchPanel`s from step 1, minus `tabIM`/`tabWeather` if the Chat/Weather features are disabled), adds each to a `BindableList<Control> categories`, and calls `dataGridCategory.SetDataSource(categories)`, grouped by `Tag`.
   - Per-row and per-group icons are supplied lazily by the grid via two owner-draw callbacks, `DataGridCategory_CellNeedImage` and `DataGridCategory_NeedGroupImage`, which call `PanelDefinition.ImageList.GetSuitableImageForSize(...)` / `categoryImageMapping[...].GetSuitableImageForSize(...)` to get a correctly-sized bitmap for the current DPI scale.

Feature flags (`ApplicationFeatures.TranslationsSupport`/`Classification`/`AI`, `GetEncryptionSigningEnabled`) remove a handful of panels before load, but none of that logic is CrossOver-sensitive (it's just app config), and it can only ever remove items, not empty the whole list — so a config-flag explanation doesn't fit "left panel doesn't populate."

## Leading hypothesis: same GDI+ gap as the splash screen

`ImageList.GetSuitableImageForSizeWithScale` (`MailClient.Common.UI.dll`) — the method behind every icon `formSettings` requests in step 3 above — resizes bitmaps via `CommonPaintUtils.ResizeImage`, which does:

```csharp
using (Graphics graphics = Graphics.FromImage(bitmap))
{
    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
    graphics.SmoothingMode = SmoothingMode.HighQuality;
    graphics.PixelOffsetMode = PixelOffsetMode.None;
    using ImageAttributes imageAttributes = new ImageAttributes();
    imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
    graphics.DrawImage(original, destRect, 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, imageAttributes);
}
```

This is the **exact same `InterpolationMode.HighQualityBicubic` call** identified as unimplemented in Wine's `gdiplus` (`fixme:gdiplus:resample_bitmap_pixel Unimplemented interpolation 7`) while investigating the splash screen (see `reports/splash-icons-findings.md`). Every category row icon and every group header icon in the Settings left panel goes through this same code path on first paint.

Working theory: the group/row icon resample silently produces bad bitmap data under CrossOver (as it does for the splash banner), and `ControlDataGrid` — being a heavy, fully custom-drawn/virtualized grid rather than a native `ListView`/`TreeView` — depends on successfully-sized icons somewhere in its row/group measurement or paint pipeline. If it doesn't defensively handle a malformed/zero-content bitmap there, rows or groups can end up with zero measured height or an aborted owner-draw pass, which would present exactly as "the left panel doesn't populate" rather than a visible icon glitch.

## Not yet confirmed (blocked in this environment)

I could not capture a live trace of the Settings dialog actually being opened: this sandbox has no input-automation tooling (`xdotool`/`ydotool`/`xte` are all absent and there's no package-manager access to install one), and `gnome-screenshot`/`PIL.ImageGrab` both return a fully black frame for this Wine build's window surface (the CEF renderer also logs `Exiting GPU process due to errors during initialization`, separately from these two bugs), so I couldn't even visually verify by screen-scraping. I did capture a full traced app-startup run (through to the main Inbox window) with `+gdiplus,+font,+seh`, but the app never opened Settings during that window, so no `ControlDataGrid`-specific fixme/err was observed one way or the other.

**To confirm:** with a human driving the UI, run
```
CX_DEBUGMSG="+gdiplus,+font,+seh,+heap" /opt/cxoffice/bin/wine --bottle "emClient_win_10_x64" --cx-app MailClient.exe
```
open Settings, and grep the resulting log for `resample_bitmap_pixel`, `dispatch_exception`, and anything under `ControlDataGrid`/`ImageList` timing (the log has no managed stack traces, but a `resample_bitmap_pixel` fixme landing right as the Settings window opens — analogous to what was seen at splash time — would confirm the shared root cause).

## Proposed fix

Same fix as the splash screen, applied to the shared helper this time so it covers every caller (Settings icons, and anywhere else in the app using `ImageList.GetSuitableImageForSize*`/`ResizeImage`):

- In `CommonPaintUtils.ResizeImage` (`MailClient.Common.UI.dll`), change `graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;` to `InterpolationMode.HighQualityBilinear`.
- `MailClient.Common.UI.dll` (2MB) is far more likely to round-trip cleanly through ilspycmd decompile+rebuild than the 28MB `MailClient.dll`; try that route first before reaching for an IL patch.
- If the live trace above shows this *isn't* what's happening (no `resample_bitmap_pixel` fixme near Settings-open), the next things to check are `ControlDataGrid`'s virtualized row-height/measurement code and `ControlPanelSwitcher`'s panel-activation logic — both too large to fully decompile in this pass (`ControlDataGrid` alone has 1000+ compiler-generated display classes) and worth a dedicated follow-up if the icon-resample fix doesn't resolve it.
