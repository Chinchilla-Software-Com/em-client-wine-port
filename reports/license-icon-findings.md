# License dialog "Get a license" button — tofu boxes instead of an icon

> Found during testing around the RSA-OAEP license Activation bug (same dialog,
> `supporting/register-icons-broken.png`). Initially assumed to be in the same family as the
> font-substitution fixmes noted elsewhere in this investigation (Wine not resolving an
> icon-font glyph) — turned out **not** to be a Wine/CrossOver issue at all.

## Symptom

The "Get a license" button in the License dialog shows two tofu boxes (`□□`) immediately after
the label text, with no space: "Get a license□□". The other two buttons on the same dialog
(Activate, Close) show no such artifact.

## Root cause — CONFIRMED, and confirmed NOT a Wine bug

Extracted the button's `Text` resource directly (`ilspycmd --resource
"MailClient.UI.Forms.formLicense.resources/buttonGetLicense.Text" original/MailClient.dll`):
the literal string is `"&Get a license"` — two Unicode **C1 control characters**
(U+0083, "BREAK PERMITTED HERE") baked directly into `MailClient.dll`'s own embedded resource
data. This isn't something CrossOver or Wine could introduce; it's shipped as-is in the original
DLL.

Three checks, each ruling out a Wine-side explanation:

1. **Not a font-substitution gap.** `buttonGetLicense` is a `MailClient.Common.UI.Controls.
   ControlButton` (custom control). Its paint code
   (`ControlToolStripButton.OnPaint` → `TextRendererEx.DrawText(e.Graphics, text, Font,
   textRect, foreColor, flagsToUse)`) draws the raw `Text` string with plain GDI text
   rendering — no icon-glyph special-casing, no custom font substitution table, anywhere in the
   class hierarchy. Whatever renders for U+0083 is whatever GDI/Uniscribe renders for it, on any
   platform. C1 control characters (U+0080–U+009F) have no glyph in virtually any font, so this
   would very likely also show as tofu on real Windows, not just under Wine.
2. **Isolated, not a repeated convention.** Searched the whole `MailClient.dll` for the exact
   byte pattern (UTF-8 `C2 83 C2 83`, i.e. two U+0083 in a row): occurs **exactly once**, right
   after "Get a license", nowhere else. If this were a deliberate "icon suffix" convention (some
   apps append a private-range character to a label to trigger a custom-drawn icon), other
   buttons using the same pattern would be expected — none exist. A broader scan for any
   printable text immediately followed by a UTF-8-encoded C1 control character (not necessarily
   the same codepoint) found only random-looking matches inside unrelated binary/numeric data —
   no other genuine label text anywhere in `MailClient.dll` or `MailClient.Common.UI.dll` is
   followed by a C1 control character.
3. **No clean reference to compare against.** English is the neutral/default culture, baked
   directly into `MailClient.dll` itself — there's no separate `en` satellite resource DLL the
   way there is for `de`, `fr`, `ja`, etc., so there's no "known good" copy of this exact string
   anywhere in the shipped app to diff against.

Byte-level provenance: the UTF-8 bytes `C2 83 C2 83` are exactly what results from taking two raw
bytes each valued `0x83` and promoting each straight from byte-value to Unicode codepoint
(Latin-1-style promotion, as opposed to Windows-1252, which remaps `0x83` to a printable
character) before UTF-8-encoding. This is a textbook double-encoding/mojibake signature — the
kind of thing a localization/resx-compilation tool produces when a byte sequence gets
reinterpreted under the wrong 8-bit codepage at some point in a build pipeline. It points to a
one-off encoding bug in whatever produced this specific string, with no discoverable "correct"
original value: no locale to compare against, no repeated pattern elsewhere in the app to infer
intent from, and no code path that treats these characters as anything other than literal text
to render.

**Conclusion: this is an upstream eM Client resource-authoring bug, not a CrossOver/Wine
compatibility issue.** Fixed anyway since the fix is cheap, safe, and clearly correct regardless
of platform.

## Fix

`MailClient.dll`, embedded resource `MailClient.UI.Forms.formLicense.resources`, entry
`buttonGetLicense.Text`: raw byte-level replace of `C2 83 C2 83` → `C2 A0 C2 A0` (two U+0083 →
two U+00A0, non-breaking space) via a new `--patch-license-icon` mode in `~/tools/il-patcher`.

Chosen specifically as a byte-length-preserving edit (2 UTF-8 bytes → 2 UTF-8 bytes per
character, 4 bytes total either way) rather than deleting the two characters outright. The
`.resources` container format stores absolute byte offsets into a shared data section for every
resource entry; shrinking one entry's byte length would require correctly recomputing and
rewriting every subsequent entry's offset, which needs full binary-format parsing. Swapping to a
same-length replacement character sidesteps that entirely — verified by confirming the patched
`MailClient.dll` is byte-for-byte identical in size to the input, and a `cmp -l` diff between the
two shows exactly 2 changed bytes in the whole 28MB file (the two `0x83` → `0xA0` byte values;
the leading `0xC2` byte of each 2-byte UTF-8 sequence is unchanged since both source and
replacement characters share it).

Non-breaking space was chosen over a visible replacement character (e.g. an actual external-link
arrow) because no evidence survived pointing to what, if anything, was originally intended there
— see "Root cause" above. The button now reads identically to how "Activate" and "Close" render:
plain text, no trailing icon.

## Verification

- File size: `output-stage5/MailClient.dll` and `output-final/MailClient.dll` are exactly the
  same byte count (28,670,976 bytes).
- `cmp -l` between the two: exactly 2 differing bytes, at the expected offsets.
- Resource content re-extracted and decoded post-patch: `"&Get a license  "`, as
  intended.
- Decompile check: `formLicense` and the rest of `MailClient.dll` (including the Stage 5 OAEP
  fix, still present) decompile cleanly with no other changes.
- No exception handlers touched, no IL instructions touched — this is a pure resource-data edit,
  so none of the three documented IL-patching pitfalls in `CLAUDE.md` apply.

## Status

Patched, deployed, and confirmed working. Tested in a fresh, never-activated secondary bottle
(`emClient_win_7_x64_2` — used to avoid disturbing the main bottle's already-completed license
Activation state) rather than the main bottle, even though the main bottle already had the same
build deployed. User confirmed: "That's fixed the icon issue on button... There's no icons
visible... just text... but that's nice and clean" — the button now renders as plain text with
no tofu boxes, matching Activate/Close.
