# Splash screen tip label — tofu boxes before the tip text

> Found during Windows 8 bottle testing, revisiting the original "broken icons on the splash
> screen" report that hadn't been visually confirmed under Windows 7 earlier in this project.
> `supporting/splash-broken.png`.

## Symptom

The splash screen's rotating "tip" line (e.g. "Automatic replies - set up Out of office
responses", "Schedule and start Online meetings", "Send later - send emails at a specific time")
shows two tofu boxes (`□□`) immediately before the text on every launch.

## Root cause — CONFIRMED, and confirmed a genuine Wine gap (unlike the license button bug)

`FormSplashScreen.labelTip`'s baseline `Text` resource
(`MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text`, extracted directly via
`ilspycmd --resource`) is a single, correctly-encoded emoji: **U+1F4A1 (💡, light bulb)**. The
code then appends a randomly-chosen tip string after it:

```csharp
LabelEx labelEx = labelTip;
labelEx.Text = labelEx.Text + " " + keyValuePair.Value;
```

Unlike the License dialog's "Get a license" button (`reports/license-icon-findings.md`), this is
**not** corrupted resource data — U+1F4A1 is a real, properly-encoded character that real Windows
renders fine via its built-in Segoe UI Emoji font-linking. A UTF-16 surrogate pair with no
available glyph typically renders as one tofu box per surrogate half, which is exactly the two
boxes observed. This is a genuine Wine emoji-rendering gap, squarely in this project's
"CrossOver stabilization" scope — not an upstream eM Client bug.

Confirmed the same 4-byte UTF-8 sequence (`F0 9F 92 A1`) also appears once more elsewhere in
`MailClient.dll`, inside `SplashScreenHints`' `EmoticonLookup` XML table (`*IDEA*` → 💡, a
legitimate, unrelated compose-window emoticon-shortcut feature) — not part of this bug, and
irrelevant to the fix since it scopes its byte search to the `FormSplashScreen.resources`
container specifically.

## Investigation: a proper font-linking fix was attempted first

Rather than immediately reaching for a narrow string patch, first investigated whether Wine
could be configured to render the emoji correctly everywhere in the app (this splash tip, and
any other emoji encountered elsewhere, e.g. in received email bodies) — a much higher-leverage
fix than patching one label if it worked.

**Fonts**: the bottle ships none of Segoe UI, Segoe UI Emoji, or Tahoma — only the classic Wine
"core fonts" (Arial, Times, Verdana, etc.). The user sourced a full, genuinely-licensed set of
Segoe UI (all weights), Segoe UI Emoji, Segoe UI Symbol, Segoe MDL2 Assets/Fluent Icons, Segoe
Print/Script, Segoe UI Historic/Variable, Tahoma, and Calibri font files (30 files total,
licensed via the user's own Windows license) and vendored them into the repo at `fonts/`. Safe
to keep committed since the user holds a legitimate license to use them for this testing.

**Registry configuration attempted**: installed all 30 fonts into a bottle's `Fonts` folder,
registered them in `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts`, and added
`FontLink\SystemLink` fallback entries (Tahoma and Segoe UI → Segoe UI Symbol, Segoe UI Emoji) —
the same registry mechanism real Windows uses to resolve emoji/symbol glyphs inside normal UI
text without the app having to change fonts mid-string.

**A real bug found along the way**: `wine regedit /S <file.reg>` has a multi-string (`REG_MULTI_SZ`)
import bug in this CrossOver/Wine build. Confirmed via `CX_DEBUGMSG=+font` trace — both `hex(7)`
(raw byte) and `str(7)` (Wine's own quoted-text-with-`\0`-escapes) `.reg` syntax get mangled on
import: the two intended fallback-font strings were split into one bogus single-character
"string" per UTF-16 code unit (trace showed `load_system_links L"Segoe UI": L"s"`, then `L"e"`,
then `L"g"`, ... spelling out the value one character at a time instead of loading it as two
separate entries). Wine's own pre-existing default `SystemLink` entries (for CJK font fallback,
never round-tripped through `regedit` import) don't show this corruption — the bug is
specifically in the import parser, not in how the value is stored or consulted afterward.

**Worked around** by writing the registry value through the actual Win32 registry API instead of
`regedit`'s text-file importer: a tiny throwaway `net8.0` console app, published as a `win-x86`
executable and run inside the bottle with its own installed .NET runtime
(`Microsoft.Win32.Registry.SetValue(..., string[], RegistryValueKind.MultiString)`), which
writes correctly (confirmed by reading the value back, and independently by re-tracing: after
this fix, `load_system_links` correctly logged the two fallback strings as separate entries).

**Still didn't fix the rendering.** Confirmed via trace that Wine correctly *loads* the
`SystemLink` configuration into its font-linking data structures — but the splash tip still
showed the same two tofu boxes after relaunching. `labelTip`'s paint path
(`LabelEx.OnPaint` → `TextRenderer.DrawText`, confirmed by decompile) uses the GDI text-rendering
API that `SystemLink` is meant to support, so this isn't a GDI-vs-GDI+ mismatch either. The gap
is deeper than registry configuration: somewhere in Wine's actual glyph-shaping/`ExtTextOut`
implementation, the loaded fallback data isn't being consulted at the point of drawing a specific
missing glyph — a genuine Wine internals question (Uniscribe/`usp10`, or the GDI glyph-lookup
code itself), not something fixable from the registry or app side. Not pursued further; this is
a real, separate Wine limitation worth revisiting if it resurfaces elsewhere, but out of scope
for a single-label cosmetic fix.

**The vendored fonts and registry entries were left in place** (harmless, and Segoe UI/Tahoma
being genuinely installed instead of Wine's default substitutes may still improve general text
fidelity elsewhere in the app, independent of this specific bug) but are **not** what fixes this
symptom — see below.

## Fix (what actually resolved the symptom)

Same pattern as the license button fix: byte-level replace within the embedded resource blob,
`F0 9F 92 A1` (💡, 4 UTF-8 bytes) → `C2 A0 C2 A0` (two U+00A0 non-breaking spaces, also 4 bytes)
— `--patch-splash-tip-icon` in `~/tools/il-patcher`. Byte-length-preserving for the same reason
as before: keeps the `.resources` container's data-section offset table untouched, so no
full binary-format rewrite is needed. Verified: patched `MailClient.dll` is byte-for-byte
identical in size to the input, `cmp -l` shows exactly 4 differing bytes in the whole 28MB file.

Chose non-breaking spaces (matching the license button precedent) over deleting the character
outright, for the same length-preservation reason, and over trying to find "the right" visible
replacement glyph — there's no strong signal for what, if anything, should visually replace a
tip-line icon that Wine simply can't render right now.

## Verification

Deployed to `emClient_win_8_x64` and relaunched multiple times (the tip text is randomly chosen
per launch) — confirmed clean rendering with no tofu boxes across multiple different tip strings
("You can add tasks directly to the Agenda in the sidebar...", etc.), via screenshot capture
timed to catch the splash window (borderless/untitled, so not reliably found by window-manager
title matching — rapid-interval `gnome-screenshot` polling right after launch instead).

## Status

Patched (Stage 7) and confirmed working via screenshot. Also fixes the original, never-actually-
visually-confirmed "bug 1" from earlier in this project (splash screen rendering) — that earlier
fix (Stage 1, `InterpolationMode.Bilinear`) addressed the banner *image* quality correctly, but
the tofu-box symptom the user was actually seeing on the splash screen was this separate label
bug, not resolved until now.
