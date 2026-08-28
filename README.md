# eM Client CrossOver Stabilization

IL-level binary patches (applied via [Mono.Cecil](https://github.com/jbevain/cecil), no source
access needed) that fix real bugs in [eM Client](https://www.emclient.com/) 10.4.5674 when it
runs under [CrossOver](https://www.codeweavers.com/crossover)/Wine on Linux — not a native Linux
port, no source changes to eM Client itself, just targeted binary patches for the specific things
Wine gets wrong.

## Quick start

Already have eM Client installed in a CrossOver bottle? Patch it:

```bash
./releases/10.4.5674/deploy.sh
```

That's it. It finds your bottle(s), checks the installed version matches, backs up the current
files, rebuilds and re-verifies every patch fresh against your exact install, deploys, and rolls
back cleanly if anything goes wrong. Safe to run more than once — it detects an already-patched
install and does nothing.

```bash
./releases/10.4.5674/deploy.sh --list          # just show what bottles/versions it finds
./releases/10.4.5674/deploy.sh --bottle NAME   # target a specific bottle non-interactively
./releases/10.4.5674/deploy.sh --help          # full usage
```

Needs the [.NET SDK](https://dotnet.microsoft.com/download) and `python3` — the script checks
for both and tells you how to install whichever is missing.

## What's fixed

| Bug | Root cause | Report |
|---|---|---|
| Splash screen banner rendered wrong | Wine's `gdiplus` doesn't implement `InterpolationMode.HighQualityBicubic` | [gdiplus-interpolation-findings.md](reports/gdiplus-interpolation-findings.md) |
| Settings dialog's left category panel was completely blank | `formSettings`'s `Load` event never fires under Wine | [settings-panel-clip-region-findings.md](reports/settings-panel-clip-region-findings.md) |
| Settings crashed when clicking a category | Wine throws the wrong exception type from a Shell COM API eM Client already handles for the *expected* type | [default-mail-client-notimplemented-findings.md](reports/default-mail-client-notimplemented-findings.md) |
| License Activation silently failed (spinner, then nothing) | RSA-OAEP decrypt fails inside Wine's `bcrypt.dll`/GnuTLS backend | [license-activation-oaep-findings.md](reports/license-activation-oaep-findings.md) |
| License dialog's "Get a license" button showed two tofu boxes | Corrupted control characters baked into eM Client's own resource data (not a Wine bug) | [license-icon-findings.md](reports/license-icon-findings.md) |
| Splash screen tip line showed two tofu boxes | A real emoji character Wine has no glyph for (a genuine Wine gap, investigated at length) | [splash-tip-icon-findings.md](reports/splash-tip-icon-findings.md) |

All six confirmed fixed and working, tested across Windows 7/8/10/11 CrossOver bottles.

## How it's built

```
original/<os-version>/   Untouched eM Client install output, one subfolder per Windows version
                          tested against (the installer ships identical bytes for every version
                          checked so far). Not tracked in git — drop a fresh install here.

il-patches/               The Cecil-based patcher tools (source tracked, binaries built fresh
                           each run) plus every patch's own doc comments explaining what it does
                           and why.

releases/<version>/       deploy.sh — the thing you actually run (see Quick start). Versioned so
                           a new eM Client release gets its own folder rather than editing this
                           one in place.

reports/                  One findings doc per bug: root cause, the fix, and — importantly —
                           any wrong turns taken first and why, so they don't get re-explored.

supporting/                Before/after screenshots for the visual bugs.

fonts/                    Genuine Microsoft fonts (Segoe UI, Tahoma, Calibri) the app expects
                           but Wine doesn't ship — see "A note on fonts/" below.

CLAUDE.md                 The full technical deep-dive: exact patch pipeline commands, the
                           investigation method that found each bug, and hard-won lessons about
                           doing this kind of IL patching correctly. Written for AI-assisted
                           continuation (this project was built working with Claude Code) but
                           equally useful as human documentation of exactly how everything works.
```

Every patch is a small, targeted edit — flip a constant, add a missing exception handler, redirect
one call to a different implementation, fix a couple of bytes in an embedded resource — never a
full reimplementation. `CLAUDE.md` documents the exact IL-level mechanics and the mistakes made
(and fixed) along the way, if you want to go deeper than the summary table above.

## Extending this for a new eM Client version

1. Drop the new version's install output into `original/<new-version>/`.
2. Copy `releases/10.4.5674/` to `releases/<new-version>/`.
3. Retest each patch by hand against the new build — don't assume it still applies; see
   `CLAUDE.md`'s "Investigation method" for how each bug was originally found and confirmed.
4. Fix up whatever broke in the new folder's `deploy.sh`. Each version's script stays a frozen,
   working reference for that exact build.

## A note on `fonts/`

The font files vendored in this repo (Segoe UI, Segoe UI Emoji, Tahoma, Calibri) are genuine
Microsoft fonts, not open-licensed — they're here because whoever added them holds a valid
Windows font license, not because they're freely redistributable. They're used to work around
Wine's default font substitutes (see `reports/splash-tip-icon-findings.md` for what that
investigation did and didn't fix) and are installed into a bottle only on explicit opt-in
(`deploy.sh --install-fonts`, which asks first). If you're forking or reusing this repo, make
your own call on whether you're entitled to redistribute them — don't assume this repo having
them means you are.

## License

The patching tools and scripts in this repo (`il-patches/`, `releases/`) are original work with
no license file attached yet — ask if you want to reuse them and it'll get sorted out. eM Client
itself is proprietary and not included here (`original/` is gitignored); this repo only ever
ships small binary *diffs* against an install you provide yourself. See "A note on `fonts/`"
above for the vendored font files specifically.
