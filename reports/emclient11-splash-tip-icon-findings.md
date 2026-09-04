# eM Client 11 (beta) splash screen tip label: tofu boxes (same bug as v10)

**Status: fixed and deployed as release/11.0.196-2.** Confirmed working live (visually
verified) — the splash screen's tip label no longer shows tofu boxes before the tip text.

Separate product line from the rest of this repo's fixes — see CLAUDE.md's "eM Client 11
(beta) — separate release line" section and `reports/emclient11-pbkdf2-startup-crash-findings.md`
(release 11.0.196-1) for how this release line is kept apart from `releases/10.4.5674/`.

## Symptom

Same as the already-fixed eM Client 10.4.5674 bug (`reports/splash-tip-icon-findings.md`): the
splash screen's rotating "tip" label (`FormSplashScreen.labelTip`) showed two tofu boxes before
the tip text, on every launch.

## Root cause — confirmed identical to v10, not just similar-looking

Rather than assume the symptom implies the same root cause, checked directly: extracted this
build's own `MailClient.UI.Forms.FormSplashScreen.resources/labelTip.Text` baseline resource via
`ilspycmd --resource`. It contains exactly 4 bytes: `F0 9F 92 A1` — the UTF-8 encoding of
U+1F4A1 (light bulb emoji), byte-for-byte the same value the v10 fix targets, nothing else in
the resource. Same resource path, same assembly (`MailClient.dll`), same control
(`MailClient.Common.UI.Controls.LabelEx`, resource-populated via
`componentResourceManager.ApplyResources(this.labelTip, "labelTip")`), same mechanism as
documented in `reports/splash-tip-icon-findings.md`: a genuine emoji character Wine has no glyph
for, not corrupted bytes (contrast with the license-icon bug, which *was* corrupted bytes — see
that report's own "Wine gap vs. app bug" distinction for why this matters).

A full-assembly byte scan found the same 4-byte pattern in exactly 2 places in this build too
(matching v10's count): once in `FormSplashScreen.resources` (this bug) and once in
`MailClient.Resources.UI.Form.SplashScreenHints.resources` (the unrelated `*IDEA*` → 💡 emoticon
shortcut feature — irrelevant here, and the patch's own resource-scoped search never touches it).

## Fix

Reused the existing `--patch-splash-tip-icon` il-patcher flag **completely unmodified** — no new
patch code needed. Same byte-level, length-preserving replacement as v10:
`F0 9F 92 A1` → `C2 A0 C2 A0` (two U+00A0 non-breaking spaces, also 4 bytes, keeps the
`.resources` container's offset table untouched).

Verified before wiring into `releases/11.0.196-beta/deploy.sh`:
- Direct dry run against this build's own `MailClient.dll` (with its sibling assemblies present
  for Cecil's metadata resolution — this build's `MailClient.dll` alone isn't enough, same
  "needs the full app directory" requirement as every other patch in this release line).
- Output file size byte-identical to input (31095296 bytes both ways) — confirms the `.resources`
  offset table wasn't disturbed.
- Re-extracted `labelTip.Text` from the patched output: reads `C2 A0 C2 A0`, exactly as intended.
- Deployed as Stage 2 of `releases/11.0.196-beta/deploy.sh`, run live against the real bottle
  (resuming correctly from revision 1 straight to Stage 2, then a second run confirming the
  "already at release 11.0.196-2" short-circuit). **Visually confirmed by the user** after
  launch — no screenshot capture was practical (the splash screen is too brief to reliably catch
  mid-launch by polling), but direct visual inspection confirmed the fix.

## Takeaway

When a bug looks identical to one already fixed in a sibling product version, **verify the root
cause is actually identical before assuming the fix transfers** — don't just copy the patch over
on the strength of a matching symptom. Here it genuinely was identical (same resource path, same
exact bytes, same control, same mechanism), confirmed by extracting and comparing the actual
resource bytes before reusing the flag — which is also why no new patch code was needed at all,
just wiring the existing flag into this release line's own Stage 2.
