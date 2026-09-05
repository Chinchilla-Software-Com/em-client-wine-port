# eM Client 11 (beta) crash while typing in a compose window: missing ICU DLLs

**Status: fixed, confirmed working live.** Fixed by `releases/11.0.196-beta/install-msix.sh`
(an install-time step, not a `deploy.sh` IL patch — see "Why this isn't a deploy.sh patch" below).

Separate product line from the rest of this repo's fixes — see CLAUDE.md's "eM Client 11
(beta) — separate release line" section.

## Symptom

The app crashed repeatedly while drafting an email. Four `bug.<timestamp>.txt` crash reports were
generated in quick succession, all identical.

## Root cause

Every crash report showed the same exception:

```
System.DllNotFoundException: Unable to load DLL 'icuuc.dll' or one of its dependencies: Module not found. (0x8007007E)
  at MailClient.Utils.Text.WordBreak+Interop.ubrk_open(...)
  at MailClient.Utils.Text.WordBreak+<EnumerateWordsIcuCore>d__2.MoveNext()
  at MailClient.UI.SpellChecker.DocumentSpellChecker.WebBrowser_SpellCheckNeeded(...)
  at MailClient.Common.UI.Controls.CefWebBrowserEx.CefWebBrowserEx.OnSpellCheckNeeded(...)
  at MailClient.Common.UI.Controls.CefWebBrowserEx.CefSpellCheckHandlerEx.OnRequestTextCheck(...)
  ...
```

eM Client's spell-checker (`MailClient.Utils.Text.WordBreak`) calls ICU's BreakIterator C API
directly:

```csharp
[DllImport("icuuc.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
public static extern UBreakIteratorSafeHandle ubrk_open(UBreakIteratorType type, string locale, nint text, int length, out UErrorCode status);
// + ubrk_next, ubrk_getRuleStatus, ubrk_close, same DLL -- confirmed via full-assembly decompile
// that these 4 functions in 1 DLL are the ONLY native interop this assembly does at all.
```

`icuuc.dll` is a literal, unversioned filename — the classic ICU4C convention, used here because
real Windows 10 (1703+) ships its own built-in `icu.dll` plus compatible `icuuc.dll`/`icuin.dll`
shim DLLs system-wide. **Wine does not implement or ship any of these** — confirmed absent from
Wine's own DLL directory and from a clean bottle's `system32`. Spell-check fires on essentially
every text-check request while typing, so this crashed reliably and repeatedly the moment text
was typed in a compose window.

This is not a Wine gap in the traditional "implements the API differently" sense this project
usually deals with — it's a genuinely absent OS component. Confirmed it isn't a beta-build MSIX
packaging omission either: extracted and checked both the x86 *and* x64 `.msix` packages from
the bundle, neither ships any ICU DLLs at all (only `icudtl.dat`, which is CEF/Chromium's own
bundled data file for the browser engine's internal use — a completely different consumer than
eM Client's own spell-checker interop).

## A dead-end tried first: a random "icuuc.dll" found online

A quick web search suggested dropping a copy of `icuuc.dll` into the app's install directory,
since "modern Windows ships this natively." A copy was sourced and tried, and the crash was
unchanged. Inspection via `objdump -p` explained why: **every one of its 542 exports was a
Forwarder RVA to a separate `icu.dll`** — this file contained no actual ICU implementation, it
was purely a compatibility shim redirecting calls to Windows' own built-in `icu.dll`. Since that
target (`icu.dll`) doesn't exist in Wine either, the shim just moves the missing-dependency
problem one level, rather than solving it. Confirmed via the same `objdump -p` check that Wine
provides no `icu.dll` anywhere (its own DLL directory, or the bottle's `system32`).

## Fix: a self-contained ICU4C build with unversioned symbols

Found via a community Unix Stack Exchange answer
(https://unix.stackexchange.com/a/805541), itself citing an official WineHQ bug report
(https://bugs.winehq.org/show_bug.cgi?id=53354, "Wine should provide icu.dll") as the source of
the technique. The fix is a build of ICU4C from
**https://github.com/FaithLife-Community/icu** — confirmed via GitHub's API to be a direct fork
of the official `unicode-org/icu` project, not an unrelated binary — compiled with unversioned
(Microsoft-compatible) symbol names, so `ubrk_open` etc. export as plain names instead of ICU's
default versioned form (e.g. `ubrk_open_75`), matching exactly what eM Client's hardcoded
`DllImport` expects.

Downloaded and verified before use (release `72.1-custom+4`,
`https://github.com/FaithLife-Community/icu/releases/download/72.1-custom%2B4/icu-win.tar.gz`,
SHA-256 `2813ea1205fcf9d9a7930339dae181dc3e299b2391f2f4879c5e542be43f3a7f`):
- `icuuc.dll`/`icuin.dll` in *this* build are themselves thin forwarders to `icu.dll` too (same
  `objdump -p` check as above) — but unlike the broken one found online, this package ships its
  own real `icu.dll` right alongside them (~4-5MB, 2108 genuine exports, `ubrk_open` confirmed as
  a real non-forwarded export), so the forwarding chain actually resolves. All three files (plus
  the accompanying `globalization/ICU/*` locale/timezone data) must be installed together.
- License: ICU License (Unicode License v3) — permissive, freely redistributable open source,
  unrelated to this repo's `fonts/` proprietary-font licensing caveat.

### Installation (matches the Stack Exchange answer's own script, adapted for a CrossOver bottle)

1. Copy `system32/{icu,icuin,icuuc}.dll` and `syswow64/{icu,icuin,icuuc}.dll` into the bottle's
   `drive_c/windows/system32/` and `drive_c/windows/syswow64/` respectively (a win64 bottle keeps
   64-bit DLLs in `system32` and 32-bit ones in `syswow64` — eM Client itself is x86-only, so only
   the `syswow64` set is strictly load-bearing here, but the 64-bit set is installed too for
   completeness, matching the verified reference script).
2. Copy `globalization/ICU/*` (locale/timezone data, including a *second*, unrelated `icudtl.dat`
   — Windows' own system-wide globalization data, not to be confused with eM Client's own
   CEF-bundled `icudtl.dat` sitting directly in the app's install directory) into
   `drive_c/windows/globalization/ICU/`.
3. Set `HKCU\Software\Wine\DllOverrides` to `native` for `icu`, `icuin`, and `icuuc` (via
   `wine reg add ... /f`) — forces Wine to actually use the just-installed files rather than
   anything it might otherwise try to resolve internally.

### Why this isn't a `deploy.sh` patch

This is a missing **OS-level** dependency, not a bug in eM Client's own binaries — there's no IL
to patch, and the fix has nothing to do with which eM Client version is installed. It belongs
with the other install-time provisioning this project already does outside the numbered IL-patch
pipeline (fonts, file-type associations for the 10.4.5674 line) rather than as a `deploy.sh`
Stage. Folded into `releases/11.0.196-beta/install-msix.sh` instead, immediately after the MSIX
extraction step, downloaded fresh from the GitHub release each run (same pattern already used
there for the .NET runtimes and the msixbundle itself) rather than vendored into this repo.

### Verification

Installed into `emClient_11_beta_win_11` live: copied the DLLs and data into place, set the three
registry overrides, closed and relaunched the app (a fresh launch was needed — DLL resolution
failures aren't retried mid-session), then drafted an email with a deliberately misspelled word.
Confirmed working — spell-check now underlines the misspelling instead of crashing.

## Takeaway

Don't trust a plausible-sounding fix (`"just drop this well-known DLL in"`) without actually
inspecting what it contains — the first `icuuc.dll` tried was real, present, and load-bearing on
genuine Windows, but useless here because it forwards to something Wine doesn't have either.
`objdump -p`'s export table (specifically, `Forwarder RVA` entries) is the fast way to tell "self-
contained implementation" from "compatibility shim expecting something else to already exist."
