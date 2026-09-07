# eM Client 11 beta — startup "white screen" stack-overflow investigation

## Status: THREE confirmed root causes, ALL fixed. App launches and renders normally, including
## the message preview-pane header font.

**Not the bug originally suspected.** This started as a hunt for a "white screen" rendering bug
matching the same class already fixed twice elsewhere in this project (Wine clip-region /
layered-window paint bugs — see `reports/settings-panel-clip-region-findings.md` and
`reports/notification-empty-until-fade-findings.md`). It is not that. Both mechanisms found here
are genuine **native stack-overflow crashes on the main UI thread during startup** — the thread
dies before ever painting the real window content, leaving an orphaned blank window on screen
that looks identical to a paint bug but has a completely different cause and fix shape.

Found and confirmed on the **local** test bottle (`emClient_11_beta_win_11`, this dev
environment's own `~/.cxoffice/` bottle, real exec access) after it was deleted and recreated
fresh via `releases/11.0.196-beta/install-msix.sh --new-bottle` (see that script's own history
for the Core Fonts / bottle-creation work done alongside this). Reproduced identically across
many launches at DLL patch revision 3. Not yet checked against the remote bottle
(`emClient_11_beta`) — everything here used direct `cxstart`/`gdb` access this dev environment
has that the remote one does not.

## How this was found: `ps`/`ntsync_schedule` first, not tracing

The first real signal wasn't a trace line — it was `ps -eLo pid,tid,pcpu,stat,wchan` during a
live "frozen white screen" showing **every** thread across every eM Client/CEF process in
`ntsync_schedule.isra` at 0% CPU, unchanged across repeated checks 20+ seconds apart. Zero CPU
anywhere is a genuine hang, not the already-documented CPU-bound sync-freeze mechanism
(`reports/emclient11-msgraph-sync-freeze-findings.md`'s "root cause #3", which shows a real,
actively-executing thread pegged at high CPU). That ruled the known bug out immediately and
redirected the whole investigation.

A `CX_DEBUGMSG="+message,+thread"` trace of a fresh launch then showed the smoking gun directly:
the main UI thread logs normally for ~10-20 seconds of real startup activity, then simply
**stops appearing in the trace at all** — no more `WM_PAINT`, nothing — at the exact same moment
Wine itself logs a `stack overflow` line. The thread died; nothing crashed loudly enough to
produce a `bug.*.txt` report (see below for why).

## Bug A: `IUISettings2::get_TextScaleFactor` — Wine WinRT stub, Chromium-side unbounded retry

**Confirmed root cause.** Chromium's own `ui/display/win/uwp_text_scale_factor.cc` (confirmed by
extracting the exact error-message strings from `libcef.dll` — `"..\ui\display\win\
uwp_text_scale_factor.cc"`, `"RoActivateInstance failed:"`, `"IUISettings2::TextScaleFactor
failed:"`) activates the WinRT runtime class `Windows.UI.ViewManagement.UISettings` to detect the
OS-level accessibility text-scaling setting, then calls `IUISettings2::get_TextScaleFactor`
repeatedly — 1371 times, confirmed via `grep -c` on multiple independent trace captures — each
call logging Wine's own `fixme:ui:uisettings2_get_TextScaleFactor` line with a monotonically
**decreasing** pointer value each time (direct evidence of a real, deepening recursive call
stack, not just repeated unrelated calls), until Wine's own guard-page detector prints
`fixme:ui:uisettings2_get_TextScaleFactor stack overflow N bytes addr ... stack 0x450000-
0x451000-0x5d0000` and the thread dies.

**Confirmed NOT Wine's `windows.ui.dll` implementation recursing itself**: disassembled the real
function (`/opt/cxoffice/lib/wine/i386-windows/windows.ui.dll`, string `
uisettings2_get_TextScaleFactor` at file offset `0xa2a0` → RVA `0x1000c4a0` → containing function
at `0x100052d8`) — it's a trivial ~15-instruction stub: check a "have I logged this fixme
already" flag byte, log once if not, unconditionally `return E_NOTIMPL`. It cannot recurse on
its own. The runaway recursion is happening one layer up, in Chromium's own call/retry logic
around this activation — not something this repo's IL-patching approach can reach (it's native
code inside `libcef.dll`/Chromium, not a MailClient managed assembly).

**Working mitigation, deployed to the local bottle**: a Wine DLL override forcing `windows.ui`
to fail to load at all —
```
wine reg add "HKCU\Software\Wine\DllOverrides" /v "windows.ui" /t REG_SZ /d "" /f
```
This makes `RoActivateInstance` fail immediately and cleanly (a code path Chromium already
handles without looping — it has a distinct, non-looping logged error string for exactly this
case), instead of reaching the buggy repeated-`get_TextScaleFactor` path. **Confirmed fully
effective for Bug A specifically**: after this override, `uisettings2_get_TextScaleFactor` never
appears in the trace at all, on any subsequent launch.

**Not part of `deploy.sh`/`il-patcher` yet** — this is a bottle-level Wine registry setting, not
a MailClient.dll IL patch, so it doesn't belong in the existing patch pipeline shape. Needs its
own mechanism (a new `deploy.sh` step, or folded into `install-msix.sh`'s bottle setup) if this
is to ship as a real fix rather than a manual one-off. Not done yet — pending a decision on
whether Bug B (below) needs solving first, since fixing only Bug A does **not** fix the white
screen; Bug B still crashes the app.

## Bug B: a second, different recursion inside `libcef.dll` itself — NOT FIXED

With Bug A's path closed off, the app still crashes — deterministically, every time, ~10-20
seconds after launch — via a **different** stack overflow, this time reported generically
(`trace:seh:dispatch_exception stack overflow ...`, no distinguishing fixme) at a *different*
crash address each Wine-DLL-relocation but the same *offset within* `libcef.dll` every time.

### Why this one needed a debugger, and why that was hard here

No fixme is logged per-iteration this time — the recursion is pure compiled C++ inside Chromium,
invisible to every `CX_DEBUGMSG` channel tried (`+message,+thread,+ole,+combase,+activation,
+winrt` — the `ole`/`combase` channels confirmed real (10,915 lines) but showed only a single,
normal, non-looping COM drag-and-drop marshal/unmarshal sequence right before the ~400ms of total
silence that precedes the crash).

Getting a real native backtrace required root, which this session's own account doesn't have —
worked around because **the user has root on this same machine** and ran `gdb` themselves, with
several real obstacles along the way, each worth recording since they'll recur for anyone
picking this up:

1. **`ptrace_scope=1`** blocks a same-user, non-ancestor `ptrace` attach even with a matching
   uid — confirmed via `/proc/sys/kernel/yama/ptrace_scope`. Root bypasses this
   (`CAP_SYS_PTRACE`), a plain `sudo` from this session's own account does not (blocked by the
   sandbox itself, not a real "no root exists" condition).
2. **PID-matching by command line is a trap here.** `cxstart`'s wrapper chain execs all the way
   through to the real Wine process without ever forking — the PID bash reports via `$!` for the
   *launcher* script IS the final PID of the real `MailClient.exe`/`CrBrowserMain` process by the
   time it's fully started. A naive `pgrep -f 'MailClient\.exe$'` can also match the launcher's
   own argv (which literally contains that string) well before the real process exists — use
   `/proc/*/comm == "CrBrowserMain"` instead (Chromium renames the process via `prctl` once its
   browser-process init reaches that point) for a reliable, race-free target.
3. **The "main" process shows as a zombie in `ps`/`ps aux` almost immediately, with live sibling
   threads under the same TGID.** This is *not* evidence of an early crash — it's normal
   CEF/Chromium-under-Wine process/thread architecture (already flagged as an open question in
   `reports/emclient11-msgraph-sync-freeze-findings.md`'s "two-top-level-process" discovery, now
   further characterized but still not fully explained). `gdb -p <that PID>` refuses to attach
   ("process is a zombie"); attaching to any live sibling thread ID under the same TGID works
   fine once ptrace_scope is bypassed.
4. **`catch signal SIGSEGV` never fires for this crash at all.** Wine's own "stack overflow N
   bytes" message is emitted by a **software** stack-limit check (comparing against the TEB's
   stack-limit fields), not by touching a real hardware guard page — no genuine `SIGSEGV` is ever
   raised by the kernel for it, so `catch signal SIGSEGV` sits waiting forever. The fix: compute
   the crashing function's actual runtime address (`ntdll.dll`'s runtime base, read from
   `/proc/<pid>/maps`, plus the function's fixed file RVA `0x5f3f0` found via `objdump`/manual PE
   parsing — see below) and set a real breakpoint there directly, conditioned on `$esp` already
   being within the known guard-page range (`0x430000`-`0x480000`, from the crash's own reported
   `stack 0x450000-0x451000-0x5d0000` range) so it only fires once, deep in the recursion,
   instead of on every normal (non-recursive) call to the same function.
5. **`.NET`'s own CoreCLR uses real `SIGSEGV`s internally, constantly, for legitimate stack-probe
   growth** — a batch `catch signal SIGSEGV` / `continue` stopped almost immediately on one of
   these (a `.NET TP Worker` thread, JIT-compiled code at a low heap address, completely benign),
   consuming the batch script's fixed command list before ever reaching the real target. Fixed by
   `handle SIGSEGV nostop noprint pass` (plus the same for `SIGUSR1`/`SIGUSR2`/`SIGPIPE`, which
   Wine also uses internally for cross-thread signaling — a well-known, standard Wine-gdb
   annoyance) and relying entirely on the software breakpoint (a `SIGTRAP`, unaffected by any of
   this) for the actual stop.
6. A `gdb` **python** script (rather than a fixed `-ex` command chain) was needed to loop
   `continue` an arbitrary number of times, checking after each stop whether the hit `$pc`
   actually matches the target breakpoint address (vs. some other incidental stop), only printing
   `thread apply all bt`/`info registers` once it genuinely does. A fixed `-ex` chain can't
   express "keep going until stop reason X" and kept exhausting itself on unrelated stops.

The final working script (kept for reference, not tracked elsewhere):
`/tmp/claude-1001/-home-portagent-project/b9cec93a-25f4-4417-9caa-5bd9a12691bc/scratchpad/
break-ntdll-func.gdb` (session-local scratch, not in the repo — recreate from the description
above if needed again).

### The real backtrace, and what it shows

Captured cleanly on the first successful attempt once all six problems above were fixed. Thread 1
(`CrBrowserMain`) stopped exactly at the conditional breakpoint, `esp` deep inside the known
guard-page range, showing a clean, exactly-repeating 7-frame cycle (`bt 60` truncated only by the
requested depth, not by any unwind failure):
```
0x6c5ab1b9 -> 0x6c5ab6d5 -> 0x6c5ac0d1 -> 0x6c0126a6 -> 0x6c0128a3 -> 0x6c0123bd -> 0x6c01220a -> (repeats)
```
All seven addresses resolved (via this run's own confirmed `libcef.dll` runtime base from the
trace's own `MODULE_InitDLL` log, `0x6ABC0000`, mapped back to the file's declared `ImageBase`
`0x10000000` for `objdump -d`) to real code **entirely inside `libcef.dll`** — no ntdll, no
MailClient.dll, no managed code anywhere in this specific cycle:

- A function that builds a text/font-metrics measurement request, writing the literal ASCII
  string `"sans"` (confirmed via `movl $0x736e6173,-0x1c(%ebp)`, little-endian bytes `73 6e 61
  73`) into a local buffer — i.e., the CSS-style generic font family name `sans-serif`,
  truncated/abbreviated — strongly indicating this is Chromium's own font-fallback/measurement
  code trying to resolve that generic family.
- ...calls a small wrapper (`0x11452200`/`0x11452210`) which populates five separate metric
  fields via a shared helper (`0x114527b0`) that does floating-point DPI-scale multiplication
  (`mulsd`) and rounding (`call` into what disassembles as a `round()`-shaped libm call) for
  each.
- ...which, from inside that same wrapper, calls another function (`0x119ec010`) that loops back
  around to calling the **first** function in the cycle again — closing a genuine, unbounded
  mutual-recursion loop entirely within Chromium's own font code, not any Wine stub.

### What was tried and did NOT fix Bug B (ruled out, don't re-try blindly)

- `--disable-features=CalculateNativeWinOcclusion` (Chromium's native window-occlusion-tracking
  feature, which also happens to poll `DwmGetWindowAttribute(DWMWA_CLOAKED)` — a fixme seen
  ~3 times right before every crash, initially suspected as related). Deployed for real via a
  one-off Cecil edit to `MailClient.dll`'s `Program.DemoApp.OnBeforeCommandLineProcessing`
  (confirmed via decompile that the flag actually reached the real CEF subprocess command lines)
  — crash still happened, identical address. The `DwmGetWindowAttribute` fixme burst is very
  likely just incidental/periodic, unrelated to this recursion. This patch was reverted (no
  benefit, no reason to keep it).
- Adding `HKLM\...\FontSubstitutes` entries for `sans` and `sans-serif` → `Arial` (reasoning:
  the literal `"sans"` string in the crashing cycle, and the classic GDI generic-family
  substitution mechanism having no entry for it at all in this bottle). No effect — crash still
  identical. Left in place anyway (harmless, may help font fidelity for unrelated CSS
  `sans-serif` requests elsewhere in the app) but confirmed **not** the fix. Most likely
  explanation: modern Chromium resolves font fallback via DirectWrite (`dwrite.dll`), not the
  legacy GDI `FontSubstitutes` registry table this fix targeted — the real bug is more likely in
  Wine's DirectWrite font-enumeration/fallback implementation, a substantially different and
  bigger area than what was explored this session.
- Disabling `windows.ui.dll` alone (Bug A's fix) does not touch Bug B at all — confirmed the
  crash still happens with that override active, just via the different `libcef.dll` cycle
  instead of the WinRT one.
- **Installing a genuine "Microsoft Sans Serif" TrueType font** (`micross.ttf`, sourced from a
  real licensed Windows install, same provenance convention as `fonts/*.ttf` elsewhere in this
  repo — added to `fonts/` alongside a second font, `SansSerifCollection.ttf`). Motivated by a
  real, concrete finding: this bottle's `HKLM\...\CurrentVersion\Fonts` maps `"MS Sans Serif"` to
  `sserife.fon`, a **legacy bitmap font** Wine ships as a built-in stub — DirectWrite (which
  Chromium uses for font matching, not the legacy GDI path the earlier `FontSubstitutes` test
  targeted) cannot use bitmap fonts at all, so this looked like a strong, concrete candidate for
  "DirectWrite resolves the generic family to a font it then can't actually load, and retries
  without terminating." Installed properly: copied into the bottle's `windows/Fonts`, registered
  via `HKLM\...\CurrentVersion\Fonts` with real Windows-convention value names (`"Microsoft Sans
  Serif (TrueType)" = "micross.ttf"`, `"Sans Serif Collection (TrueType)" =
  "SansSerifCollection.ttf"`, confirmed via the TTF `name` table that these are the fonts' own
  real internal family names, not just filenames). **No effect whatsoever** — identical crash,
  identical address (`0x7b61f3fe`), on the very next launch after installing.

  This is meaningful negative evidence, not just another failed guess: three independently
  plausible, different-mechanism fixes (a Chromium feature flag, a GDI-level registry
  substitution, and now an actual missing font file) have all failed to change the crash's
  address or behavior AT ALL. That consistency across genuinely different fix shapes suggests the
  bug is likely **not actually about font/family resolution failing** in the way the `"sans"`
  string first suggested — that string may be an incidental piece of data the recursive code
  happens to be carrying/processing, not the actual cause of the non-termination. The real bug is
  more likely in the *recursion/retry control logic itself* (whatever decides "try again" vs.
  "give up") rather than in *what* is being looked up. Both `micross.ttf` and
  `SansSerifCollection.ttf` were left installed in the bottle regardless (harmless, may improve
  general font fidelity) but should not be assumed to matter for Bug B in any future
  investigation — don't re-try font-provisioning-shaped fixes without a new, different piece of
  evidence pointing at fonts specifically.

### Update: root-caused to source-file level, via real Chromium source cross-reference

Continued disassembly (same gdb capture, same technique) resolved two of the cycle's indirect
call targets to literal strings baked into `libcef.dll`:

- `0x1c693a80` → the string `"sans\0"` immediately followed by
  `"..\\..\\ui\\gfx\\platform_font_skia.cc"` — the literal Chromium **source file path**, compiled
  into the binary as part of a `NOTREACHED`/logging/DCHECK string table entry.
- `0x1c693ae0` → `"gfx::CreateSkTypeface"` / `"skia::MakeTypeface"` — real Chromium/Skia function
  names, also compiled-in as logging strings.

This pins the recursive cycle to `ui/gfx/platform_font_skia.cc`, and further disassembly (the
exact self-recursive call, `call 0x119eb100` at file address `0x119eb6d0`, and the branch
controlling it, `cmpb $0x0,-0x18(%ebp)` / `je <recurse>` at `0x119eb614`) confirmed this is
genuine, deliberate self-recursion in one function, gated by a local "have I already succeeded"
flag that a called sub-function is supposed to set via an out-parameter.

**Cross-referenced against the real, current Chromium source** (`github.com/chromium/chromium`,
fetched directly) for this exact file:
- `gfx::CreateSkTypeface` itself has **no loop** — it tries the requested font once, falls back to
  a single hardcoded default (`"sans"` on non-Android platforms — confirmed, matches the string
  found in the binary) exactly once, and returns failure cleanly if even that fails. It cannot
  recurse on its own.
- Its caller, `InitFromDetails`, on failure calls `EnsuresDefaultFontIsInitialized()` then
  `InitFromPlatformFont(GetDefaultFont().get())` — a real potential recursion path (re-entering
  font initialization using whatever the "default font" resolves to).
- Critically, `EnsuresDefaultFontIsInitialized()` has an explicit early-return guard
  (`if (GetDefaultFont()) return;`) specifically designed to make this safe — even if
  `CreateSkTypeface` fails every single time, the guard should ensure the default font gets
  "initialized" (successfully or not) exactly once, and every subsequent call becomes a cheap,
  non-recursive no-op.

**Conclusion:** the unbounded recursion we're actually observing is Chromium hitting a case its
own authors explicitly guarded against — the `GetDefaultFont()`/"already initialized" state is
either never sticking under Wine (a static-initialization or TLS issue specific to this
environment), or, more likely given the evidence, **typeface creation is failing unconditionally
for every font Chromium tries, not just specific ones** — explaining cleanly, for the first time,
why three independent, differently-shaped fixes (a Chromium feature flag, a GDI-level font
substitution registry entry, and installing a genuinely correct, real `micross.ttf`) all failed
identically: none of them could matter if the underlying typeface-creation call itself never
succeeds for *any* font on this system, regardless of which fonts exist or how they're
registered.

**Supporting evidence, not yet conclusive**: the same trace shows Wine's own `dwrite.dll` (which
Skia's Windows backend uses for font matching/typeface creation — genuinely present in this Wine
build at `/opt/cxoffice/lib/wine/{i386,x86_64}-windows/dwrite.dll`, not a missing-DLL situation)
logging `fixme:dwrite:opentype_decode_namerecord` — an incomplete/stub codepath in Wine's own
OpenType font **name-table** parser, exactly the kind of code every typeface-by-name lookup would
exercise. Only 3 occurrences in a full trace (not thousands), so this specific fixme is not itself
the recursive call — but it's real, independent evidence that Wine's `dwrite.dll` has genuine gaps
in exactly the font-metadata-parsing area this bug lives in, worth investigating further before
assuming it's unrelated.

### Update: standalone DirectWrite test — Wine's `dwrite.dll` is NOT the bug

Built a minimal standalone C# repro tool (same pattern as `il-patches/notification-repro-stub/`
— plain source, `dotnet publish -c Release` for `win-x86`, run via `wine`, no IL patching) that
calls DirectWrite directly via raw COM vtable P/Invoke, entirely independent of eM Client/CEF.
Every vtable slot index used was verified against **Wine's own `dwrite.idl`**
(`wine-mirror/wine`, `include/dwrite.idl`, fetched directly) rather than assumed from memory or
from Microsoft's alphabetized documentation page (which does not reflect true declaration/vtable
order) — this caught two wrong guesses before they could produce a misleading result
(`IDWriteFontList::GetFont` is slot 5, not 4; `IDWriteFont::CreateFontFace` is slot 13, not 12).

Result: **everything succeeded, cleanly, every time**:
- `DWriteCreateFactory` → `IDWriteFactory::GetSystemFontCollection` → 275 font families enumerated.
- `FindFamilyName` for `"Arial"`, `"Segoe UI"`, `"Microsoft Sans Serif"`, and the newly-installed
  `"Sans Serif Collection"` all found correctly (`exists=1`, correct index), and `CreateFontFace`
  succeeded for every one of them.
- `FindFamilyName` for the generic keywords `"sans"` and `"sans-serif"` (and, separately,
  `"Tahoma"` — apparently not visible to DirectWrite's collection despite being installed and
  registered, a minor separate curiosity not pursued further) returned a **clean, correct
  "not found"** (`hr=0x00000000`, `exists=0`) — exactly the well-behaved response Chromium's own
  fallback code is designed to handle, not a hang, error, or ambiguous state.
- `GetFaceNames`/`GetFamilyNames`/`GetInformationalStrings` — the exact name-table-decoding
  territory the earlier `fixme:dwrite:opentype_decode_namerecord` pointed at — all succeeded and
  returned real, correct localized name data for every font tested (minor mojibake in a couple of
  non-ASCII locale strings in the console log output is a display/encoding artifact of the test
  tool's own logging, not a DirectWrite defect — the underlying UTF-16 data was retrieved without
  error in every case).

**This conclusively rules out "Wine's DirectWrite implementation is broken" as Bug B's cause.**
Every operation plausibly involved in font-by-name resolution, typeface creation, and name-table
decoding works correctly and terminates cleanly, including the exact "generic family not found"
case that seemed like the most likely trigger. The `opentype_decode_namerecord` fixme, while real,
evidently doesn't affect any font actually tested here — likely a genuinely narrow gap (an
unsupported platform/encoding ID combination in some other font) unrelated to this bug.

**Where this leaves Bug B**: the recursion is very likely either (a) a genuine logic bug in
Chromium/Skia's own calling code, independent of Wine, that happens to only manifest given some
condition specific to this environment (e.g., threading/timing, or a different, untested
DirectWrite/Skia call this standalone tool didn't exercise — `CreateTextFormat`/`CreateTextLayout`
or lower-level glyph-outline calls are architecturally closer to what real text *measurement*
needs than the font-enumeration calls tested here), or (b) a real Wine bug in a DirectWrite
surface not covered by this test. Distinguishing between these needs either a test that more
closely mimics Skia's actual calling pattern (concurrent/background-thread calls, the specific
metrics/layout APIs rather than enumeration), or resuming the native-gdb approach to catch two or
three consecutive recursion iterations and diff their state (see below) — this update does not
resolve Bug B, it substantially narrows where the defect can be.

### Update: Bug B SOLVED — root cause and fix confirmed, app now starts and renders normally

**Multi-iteration diff, via an extended version of the gdb technique above.** Built a new gdb
script (`trace-recursion.gdb`, session-scratch) that sets breakpoints at the exact recursive call
site (`call 0x119eb100` at file address `0x119eb6d0`) and its controlling decision branch
(`0x119eb614`), logging iteration number, `$esp`, and the local "success" flag byte on *every*
hit, auto-continuing without stopping, until the final overflow. Result, captured cleanly on the
first attempt:

- **1367 total iterations** before the overflow (strikingly close to Bug A's own 1371 — strong
  supporting evidence that both bugs are structurally the same *kind* of failure, an unbounded
  recursion exhausting the same fixed-size thread stack, rather than two unrelated mechanisms that
  coincidentally produce similar counts).
- `$esp` decreased by an **exactly constant 1136 bytes every single lap** (`0x5cd4f4` →
  `0x5cd084` → `0x5ccc14` → ... , every delta identical) — confirming a uniform, unchanging stack
  frame per call, not a growing/varying one.
- The local "success" flag was **`0` on all 1367 iterations, without a single exception.** This
  directly rules out "different fonts are being tried and each fails slightly differently" — it's
  the exact same operation, on the exact same input, failing identically, forever.

**Root cause, now fully confirmed**: Chromium's font code (`ui/gfx/platform_font_skia.cc`, per the
earlier source cross-reference) falls back to its hardcoded ultimate default family name —
literally the 4-character string `"sans"` (confirmed both from the binary's own embedded string
table and from the real Chromium source's `kFallbackFontFamilyName` constant) — when a requested
font can't be resolved. On real Windows, an OS/DirectWrite-level generic-family resolution step
normally ensures *something* always answers to `"sans"` before Chromium's own code ever needs its
last-resort fallback to actually succeed standalone. **This bottle had no font anywhere whose
DirectWrite-visible family name is literally `"sans"`** (confirmed directly: the standalone
DirectWrite test tool's `FindFamilyName("sans")` cleanly returned `exists=0` before the fix) — so
Chromium's own hardcoded fallback-of-last-resort itself fails, and whatever guards this failure
upstream (see the `EnsuresDefaultFontIsInitialized()` early-return discussion above) don't hold,
producing the observed infinite recursion. This is why the three earlier font-shaped fixes all
failed identically: none of them provided a font whose real DirectWrite family name is the exact,
narrow, literal string `"sans"` — `Arial`, `Microsoft Sans Serif`, and GDI-level `FontSubstitutes`
entries for `sans`/`sans-serif` all miss this exact requirement (DirectWrite doesn't consult the
GDI substitution table at all, and none of the other fonts installed happen to carry `"sans"` as
one of their own name-table entries).

**The fix**: create a font whose Windows-platform (`platformID=3`) `name` table records for
family (nameID 1) and full name (nameID 4) are the literal string `"sans"`, install it into the
bottle, and register it. Implemented as a minimal, dependency-free Python patcher
(`make-sans-font.py`, session-scratch — no `fontTools` available in this environment) that copies
an existing, already-installed font (`Arial.TTF`, from Core Fonts) and **shrinks** (never grows)
its Windows-platform family/full-name string records in place to `"sans"` — safe without needing
to restructure the `name` table's offset layout, since the replacement is always shorter than the
original. (A Macintosh-platform, `platformID=1`, record for the same font shared the same
underlying string storage as an optimization and ended up with harmless garbled bytes as a side
effect of the shrink — irrelevant, since DirectWrite only reads Windows-platform records; not
worth avoiding for a fix this narrow in purpose.) Installed as `windows/Fonts/sans.ttf`, registered
via `HKLM\Software\Microsoft\Windows NT\CurrentVersion\Fonts\"sans (TrueType)" = "sans.ttf"`.

**Verified two ways**: (1) the standalone DirectWrite test tool's `FindFamilyName("sans")` flipped
from `exists=0` to `exists=1`, with `GetFontFamily`/`GetFont`/`CreateFontFace` all succeeding
cleanly; (2) a full real launch of `MailClient.exe` — **no `stack overflow` anywhere in the trace,
and the actual Inbox rendered completely normally** (real folder tree, real message list, real
invites panel, confirmed via screenshot) — the white screen is gone.

**Not yet productionized.** This was done by hand, directly against the local bottle's live files
— not yet wired into `deploy.sh`/`install-msix.sh`, and the derivative font file itself should
almost certainly **not** be vendored into this repo the way `fonts/*.ttf` are (it's a modified
derivative of Arial, a licensed font, unlike the vendored files which are genuine, unmodified
Microsoft font files someone holds a license for) — the right shape is very likely to ship
`make-sans-font.py` (or a proper C#/Cecil equivalent matching this repo's other tooling
conventions) as a small script that **generates** the `sans.ttf` fallback fresh from whatever font
is already installed in the *target* bottle at install/deploy time, the same "never copy a
pre-built derivative, always regenerate fresh from what's actually there" philosophy this repo
already applies to DLL patches and font-linking (see `il-patches/font-systemlink-writer/`'s own
doc comment). See "Recommended next steps" below for the concrete productionization plan.

### Recommended next steps, in order of expected value

1. **Productionize the Bug B fix**: port `make-sans-font.py`'s logic into
   `releases/11.0.196-beta/install-msix.sh` (or `deploy.sh`, wherever fonts already get installed)
   as a step that generates `sans.ttf` fresh from an already-installed real font (Arial or
   whichever is confirmed present) in the target bottle, installs it, and registers it — matching
   the "regenerate fresh, never vendor a derivative" pattern already used everywhere else in this
   pipeline. Needs a decision on language/tooling (a small Python script vendored alongside the
   shell scripts is the simplest fit; a C# tool would match `font-systemlink-writer/`'s own
   convention more closely if this project prefers that consistency).
2. **Productionize Bug A's fix too** (the `windows.ui` Wine DLL override) into the same
   install/deploy step — both bugs must be fixed together for the app to start at all, so there's
   no reason to ship one without the other now that both are confirmed and understood.
3. **Verify the fix survives a bottle recreated from scratch** (delete `emClient_11_beta_win_11`
   again, recreate via `install-msix.sh --new-bottle`, apply both fixes via whatever the
   productionized mechanism from (1)/(2) turns out to be, confirm the white screen never appears)
   before considering this closed — everything above was verified against the bottle in its
   current, already-somewhat-lived-in state, not a truly from-scratch one.
4. **Consider filing this upstream** (Wine's bug tracker, and/or as feedback to eM Client re: a
   Chromium/CEF version bump) — this is a genuine, narrow, well-understood gap between what real
   Windows guarantees (something always resolves the generic `"sans"` fallback name) and what a
   fresh Wine bottle provides (nothing does, by default) — likely to affect any other
   CEF/Chromium-embedding application run under Wine/CrossOver that hits the same
   default-font-initialization path, not just eM Client.

## Files/registry state left on the local bottle (`emClient_11_beta_win_11`)

- `HKCU\Software\Wine\DllOverrides\windows.ui` = `""` (disabled) — Bug A mitigation, keep.
- `HKLM\Software\Microsoft\Windows NT\CurrentVersion\FontSubstitutes\sans` = `Arial`,
  `...\sans-serif` = `Arial` — harmless, didn't fix Bug B, left in place.
- `HKCU\Software\Microsoft\Accessibility\TextScaleFactor` = `100` (`REG_DWORD`) — an earlier,
  independently-tried mitigation for Bug A (reasoning: WinForms' own `.NET` 8+ text-scale
  accessibility feature reads this same registry value directly, not via WinRT — see its own
  code reference in `System.Windows.Forms.Primitives.dll`/`MailClient.Common.UI.dll`, both
  confirmed via string search to reference `Software\Microsoft\Accessibility\TextScaleFactor`
  literally, not WinRT). Confirmed **not** what fixes Bug A (the DLL override does) — this value
  being present or absent made no observed difference in any test — but harmless, left in place.
- `MailClient.dll` — back to the stock, unpatched revision-3 build (the `CalculateNativeWinOcclusion`
  test patch was reverted).
- `windows/Fonts/micross.ttf` ("Microsoft Sans Serif") and `windows/Fonts/SansSerifCollection.ttf`
  ("Sans Serif Collection"), both installed and registered under `HKLM\Software\Microsoft\
  Windows NT\CurrentVersion\Fonts` (`"Microsoft Sans Serif (TrueType)"` /
  `"Sans Serif Collection (TrueType)"`). Sourced into `fonts/*.ttf` in this repo alongside the
  other vendored fonts (same license-provenance convention). Confirmed **not** the fix for Bug B
  — left installed anyway (harmless, may help general font fidelity).
- `windows/Fonts/sans.ttf` — **the actual Bug B fix**: originally generated from this bottle's
  own `Arial.TTF` (see below for why that was wrong), now regenerated from `segoeui.ttf` instead,
  with its Windows-platform `name` table family/full-name records overwritten to the literal
  string `"sans"` (`releases/11.0.196-beta/make-sans-fallback-font.py`, now a tracked script, not
  session-scratch — see the fix writeup above and the "Update" below). Registered via
  `HKLM\...\CurrentVersion\Fonts\"sans (TrueType)" = "sans.ttf"`. This is a modified derivative of
  whichever font it's generated from, NOT suitable to vendor into this repo's own `fonts/`
  directory as a binary — `install-msix.sh` generates it fresh at install time instead (see the
  "Productionized" update below).

### Update: productionized into `install-msix.sh`, AND a real visual regression found + fixed

Both fixes (`windows.ui` DLL override, `sans.ttf` generation+registration) were ported into
`releases/11.0.196-beta/install-msix.sh` as an unconditional step alongside the ICU DLL
provisioning (same "OS-level gap, not an eM Client bug" treatment), right after a newly-added
vendored-fonts install step (ported verbatim from `deploy.sh`'s own `--install-fonts`/`--no-fonts`
logic, so `install-msix.sh` no longer needs a separate `deploy.sh --install-fonts` pass
afterward). The font-patching logic itself moved from session-scratch into a tracked script,
`releases/11.0.196-beta/make-sans-fallback-font.py`.

**Then a real, user-visible regression was found**, once the app could finally render long enough
for anyone to notice: with `sans.ttf` generated from **Arial**, message body text and general UI
text rendered visibly wrong — the wrong font family entirely (Arial-shaped, not eM Client's real
intended default). Confirmed by direct comparison against a working reference (the remote bottle,
which doesn't have this bug at all — a coincidence of its own separate, much longer-lived install
history happening to already satisfy whatever the real underlying font need is, not because it
lacks the same Chromium behavior).

**Root cause of the regression**: the literal `"sans"` lookup this fix satisfies is used by
Chromium for more than just the true last-resort case its name and the source code's comment
suggest — it appears to feed into the *default* font Chromium's renderer falls back to for
ordinary, otherwise-unstyled content too, taking precedence over whatever font eM Client actually
configures as its intended default (Segoe UI, matching this project's own vendored-fonts
convention and how the app is clearly designed to look). Not fully traced to a specific line of
Chromium source this time (the earlier `platform_font_skia.cc` investigation focused on the crash
mechanism, not on which downstream consumers read the resulting cached default font) — but
empirically, unambiguously confirmed: regenerating `sans.ttf` from `segoeui.ttf` instead of
`Arial.TTF`, with no other change, visibly fixed the font back to the correct one on a live,
already-logged-in session (graceful close via `wmctrl -ic`, relaunch, screenshot comparison).

**Fix**: `install-msix.sh`'s source-font search order for `sans.ttf` now prefers `segoeui.ttf`
first, falling back to `Tahoma`/`Arial`/`Verdana` only if Segoe UI wasn't installed (e.g. the
vendored-fonts license prompt earlier in the same script run was declined) — those fallbacks
still fix the startup crash, just without matching the app's real intended look as closely.

**Not yet re-verified against a truly fresh bottle** — this was confirmed on the same
already-running local bottle used throughout this whole investigation, not a from-scratch
`install-msix.sh --new-bottle` run exercising the new script logic itself. Still on the
"Recommended next steps" list above.

### Update: the Segoe UI regeneration was a red herring — the real remaining issue was a
### genuinely unrelated missing font (Aptos), now fixed and productionized too

After the Segoe UI-based `sans.ttf` regeneration above, message body text was **still** wrong —
confirmed this time by direct comparison against a real reference screenshot (the remote
bottle, `emClient_11_beta`, which doesn't have this problem) rather than by eyeballing a single
screenshot in isolation, which is what led to the premature "looks fixed" claim earlier in this
same investigation. Lesson: a font/rendering claim needs a side-by-side comparison or a trace,
not an isolated screenshot judged from memory of what "should" look different.

Traced properly this time (`CX_DEBUGMSG="+dwrite"`, which — unlike most Wine debug channels used
elsewhere in this project — logs regular `trace:` lines for ordinary, successful DirectWrite
calls, not just `fixme:` for unimplemented ones, making it possible to see the *exact* sequence
of `FindFamilyName` calls Chromium made while rendering the actual message being viewed).
Extracting every family name requested, in order, showed the real culprit immediately: after a
block of CJK fallback probing (unrelated) and normal UI-chrome font resolution (`Tahoma`,
`sans` — the Bug B fix, working as intended), the message body content itself requested
`"Aptos"` — Microsoft's default Office/Outlook font since 2023 (replacing Calibri) — which
wasn't installed anywhere in this bottle, so DirectWrite correctly reported "not found" and
Chromium correctly fell back to Arial. This is **not a Wine bug or related to Bugs A/B at all**
— it's a plain missing font, the same shape as Core Fonts/Segoe UI/Microsoft Sans Serif earlier
in this project, just for a newer, less commonly-vendored font family. The Segoe UI vs. Arial
change to `sans.ttf` made no difference to this specific symptom because it was never the `sans`
fallback path at all — a different, unrelated code path (real per-font-family resolution for
actual message content) was responsible the whole time.

**Fix**: downloaded the full Aptos family (28 files — Regular through Black weights, Display/
Mono/Narrow/Serif variants, each with Bold/Italic/BoldItalic) directly from Microsoft's own
official download URL, installed into the bottle's `Fonts` folder, and registered each under its
own real "Full Name" (`name` table, nameID 4, Windows platform) read directly from the file
rather than guessed from its filename.

**Verified rigorously, not by eyeballing**: re-traced with the same `+dwrite` method before and
after. Before: every `FindFamilyName("Aptos")` call was immediately followed by a
`FindFamilyName("Arial")` call (the fallback firing, every time). After: `FindFamilyName("Aptos")`
succeeds and goes straight into `GetFontFamily(..., 1, ...)` to retrieve it — **the `"Arial"`
fallback call no longer happens at all.** This is a directly comparable, unambiguous before/after
trace difference, not a subjective visual impression.

**Productionized into `install-msix.sh`** as a new step right after the ICU DLL installation
(before the vendored-fonts step) — downloads the same official Microsoft zip fresh every run
(same "fetch from the real source, don't vendor a redistributable-questionable binary" pattern as
the ICU DLLs and .NET runtimes already in this script; Aptos never touches this repo's own
`fonts/` directory at all), extracts every `.ttf` (skipping the bundled EULA `.rtf`), and
registers each file under its real Full Name using the same name-table-reading technique
`make-sans-fallback-font.py` already uses for writing. Tested standalone (extracted just this
block into an isolated script and ran it against the live bottle, confirming both the initial
install and a safe idempotent re-run) before folding it into the real script — not yet re-run as
part of a complete `install-msix.sh --new-bottle` pass from scratch, same caveat as the other
fixes above.

**Not yet checked**: whether other commonly-referenced modern fonts (e.g. Calibri, the *previous*
Office default, likely still appears in plenty of real-world email) are also missing from a
fresh bottle — this was found by noticing one specific email, not by a systematic audit of what
fonts real-world HTML email tends to reference. Worth a broader check before considering font
coverage "done" rather than "the one gap someone happened to notice."

### Update: a SECOND, unrelated wrong-font bug — Aptos/Roboto was never the whole story

After installing Aptos, the user reported the font *still* looked wrong. This led to another
avoidable detour (checking Roboto, another real-but-irrelevant missing font found in the same
Chromium/DirectWrite trace) before the actual scope error was caught: **every screenshot the user
had sent, throughout this entire font investigation, was cropped to the message *header* area**
(sender name, "to" recipient, reply-timestamp line) — never the email's own HTML body
paragraph. Aptos and Roboto both affect the Chromium-rendered HTML *body* — a completely
different rendering subsystem from the header, which is classic native WinForms UI text, drawn
via GDI, not DirectWrite/Chromium at all. All of the Aptos/Roboto tracing (`+dwrite`) was
therefore examining the wrong subsystem for what the user was actually looking at — a scope
mistake, not a wrong-font-guess mistake.

**Re-traced with the correct channel** (`CX_DEBUGMSG="+font"`, which logs GDI's own
`font_SelectFont`/`select_font` calls) and parsed requested-name → resolved-name pairs
systematically (a small Python script pairing each request with the resolution that immediately
follows it on the same thread, rather than eyeballing raw trace lines). Found:
```
'MS Shell Dlg' -> 'Tahoma'
```
`MS Shell Dlg` is the classic legacy placeholder name a WinForms control resolves to when it
doesn't set an explicit `Font` (GDI's own font-substitution mechanism, `CreateFont("MS Shell
Dlg")` internally consulting `HKLM\...\CurrentVersion\FontSubstitutes`, the same registry
mechanism already used elsewhere in this investigation) — real modern Windows (Vista+) reports
Segoe UI as the OS-wide default UI font via `SystemParametersInfo`/`NONCLIENTMETRICS`, so a
WinForms control with no explicit font should end up on Segoe UI, not Tahoma. This bottle's own
`FontSubstitutes` had `"MS Shell Dlg"` and `"MS Shell Dlg 2"` both mapped to `Tahoma` instead —
likely a stale/incorrect default baked into this Wine build's `win11_64` bottle template, not
something specific to eM Client.

**Tried (NOT confirmed working — explicitly disputed by the user, do not repeat without new
evidence)**: `HKLM\Software\Microsoft\Windows NT\CurrentVersion\FontSubstitutes`, set both
`"MS Shell Dlg"` and `"MS Shell Dlg 2"` to `"Segoe UI"` instead of `"Tahoma"`.

Re-tracing did confirm the substitution itself took effect at the GDI level
(`'MS Shell Dlg' -> 'Tahoma'` became `'MS Shell Dlg' -> 'Segoe UI'`). **That is not the same as
confirming this is what the visible header text actually uses, or that it fixed the visible
symptom** — the user directly stated afterward, unambiguously, "You are completely WRONG. This
is not fixed at all." The header control in question may not resolve its font via `MS Shell Dlg`
at all (eM Client could set an explicit font, or use a different WinForms default path than
assumed here) — this was a plausible-sounding mechanism found via trace, not a confirmed one, and
it turned out to be wrong. Left in place on the bottle for now (harmless either way, a reasonable
substitution regardless of whether it's the actual fix), but **do not report this as fixed, and
do not add it to `install-msix.sh`**, until the actual visible symptom is genuinely resolved and
confirmed.

**Pattern worth naming plainly**: this session made the same kind of mistake three times in a
row on this one investigation (Aptos, then Roboto, then `MS Shell Dlg`/Tahoma) — finding a
real, genuine gap via tracing, and reporting it as *the* fix before rigorously confirming it
against the actual reported symptom. Tracing correctly identifying *a* real issue is not the same
as confirming it's *the* issue the user is actually looking at. The next step needs to start from
direct, careful agreement with the user on exactly what element, exactly what's wrong with it,
and only then trace — not trace first and pattern-match afterward.

## Bug C: `SystemFonts.MessageBoxFont` returning Tahoma instead of Segoe UI — SOLVED, this was
## the actual fix for the reported font bug

The user finally scoped the remaining symptom precisely, after three wrong turns above: **"The
ONLY part of the interface that is not drawing text correctly with the correct font is the right
hand panel preview pane, upper section which shows sender, recipient, date and sometimes a
message saying 'you replied to this message...'"** — the message-header block above the HTML
body, not the body itself, and not the general window chrome either.

Rather than trace-and-guess a fourth time, went straight to decompiling the responsible code
(`ilspycmd`, plus a throwaway Mono.Cecil-based type-name search tool to locate the right classes
in `MailClient.dll`/`MailClient.Common.UI.dll` without guessing names). Found:

- `MailClient.UI.Controls.ControlMessageDetail` (the preview-pane header control) builds its CSS
  using `FontManager.CreateFont(FontManager.UIFont.FontFamily, ...)`.
- `MailClient.Common.UI.FontManager.UIFont` (via its `GetDefaultUIFont()`) is defined as
  `System.Drawing.SystemFonts.MessageBoxFont ?? SystemFonts.DefaultFont` — i.e. it uses whatever
  the OS reports as its standard message-box font, not a hardcoded name, and NOT the `MS Shell
  Dlg`/`FontSubstitutes` mechanism chased (wrongly) in the previous section — `SystemFonts
  .MessageBoxFont` never calls `CreateFont("MS Shell Dlg")` at all.

`SystemFonts.MessageBoxFont` is backed by the Win32 `SystemParametersInfo(SPI_GETNONCLIENTMETRICS)`
API's `lfMessageFont` field. Wrote a standalone C# console tool
(`messagefont-test/Program.cs`, `net10.0-windows`/WinForms, needs
`<EnableWindowsTargeting>true</EnableWindowsTargeting>` in the `.csproj` to build under Linux)
that calls `SystemFonts.MessageBoxFont`/`SystemFonts.DefaultFont` directly and prints the result.
On this bottle, before any fix:
```
SystemFonts.MessageBoxFont = Tahoma 8.25pt
SystemFonts.DefaultFont = Microsoft Sans Serif 8.25pt
```
Real modern Windows (Vista+) reports **Segoe UI 9pt** as its message-box font. Wine sources
`lfMessageFont` from `HKCU\Control Panel\Desktop\WindowMetrics\MessageFont` — a **binary
`LOGFONTW` structure** (92 bytes: 5 `LONG` fields — height/width/escapement/orientation/weight —
then 8 `BYTE` fields — italic/underline/strikeout/charset/outprecision/clipprecision/quality/
pitch-and-family — then a 64-byte/32-`WCHAR` NUL-padded face name), **not** a plain string value.
This key was confirmed entirely absent on a fresh bottle, which is why Wine fell back to its own
internal Tahoma default instead of reporting whatever a real Windows install's actual default
would be.

**Fix**: built and wrote a correct 92-byte `LOGFONTW` blob for "Segoe UI", 9pt, weight 400, via
`HKCU\Control Panel\Desktop\WindowMetrics\MessageFont` (`REG_BINARY`). Re-ran the
`messagefont-test` tool afterward (after a full `wineserver -k`, not just an app restart — the
NONCLIENTMETRICS cache needed a fresh Wine session to pick up the registry change):
```
SystemFonts.MessageBoxFont = Segoe UI 9pt
SystemFonts.DefaultFont = Microsoft Sans Serif 8.25pt
```
Deployed to the live bottle and confirmed directly by the user, scoped correctly to just the
preview-pane header this time: **"It looks fixed now. That was the actual fix."**

**Disposition of the three earlier wrong/secondary findings**, per explicit user instruction:
- **Aptos and Roboto** (both real, genuine gaps in the Chromium/DirectWrite HTML-body rendering
  path, confirmed via trace to fall back to Arial when requested and absent) — kept in
  `install-msix.sh`'s install pipeline going forward. Not the cause of this bug, but real fonts
  real email HTML asks for; installing them means Chromium's fallback machinery does less
  wasted failed-lookup work, and anything that specifically wants either font renders correctly
  instead of silently substituting.
- **`FontSubstitutes["MS Shell Dlg"/"MS Shell Dlg 2"] = "Segoe UI"`** (left in place on the
  bottle from the earlier wrong turn) — genuinely unrelated to the actual fix (`SystemFonts
  .MessageBoxFont` never consults `FontSubstitutes`), and NOT productionized into
  `install-msix.sh`. Left as-is on this one bottle only because it's a harmless, arguably
  reasonable substitution either way, not because it did anything.

**Productionized into `install-msix.sh`**: the Roboto download/convert/install/register block
(same WOFF→TTF conversion pattern as Aptos, see `woff-to-ttf.py`'s own doc comment for why WOFF
rather than a direct `.ttf` source), and the actual `WindowMetrics\MessageFont` `LOGFONTW` fix
(inline Python, matching the Aptos block's own inline-Python-heredoc convention rather than a
separate tracked script, since the blob-construction logic is short and has no reuse elsewhere).
