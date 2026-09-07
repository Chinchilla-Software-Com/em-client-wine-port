# eM Client 11 beta — Microsoft Graph sync freeze investigation

**Status (latest session): TWO real root-cause mechanisms found and fixed at the DNS/socket
level; a THIRD, different, CPU-bound mechanism found and NOT fixed; a concurrency-limiting
mitigation for the third was tried and reverted after making things worse, not better.** See
"Session N: DNS bypass and bounded socket I/O" onward below for the full, current story — the
original "root MECHANISM confirmed... NOT fixed" framing directly below is now superseded for the
DNS/timeout part of this bug (fixed), but the underlying investigation history is kept intact
since it's still the correct explanation of *why* the DNS/socket fixes were the right next step.
None of this session's fixes are deployed via `deploy.sh` or part of any numbered release yet —
they exist as real tracked source in `il-patches/MailClient.Wine/` (`DnsConnectHelper.cs`,
`TokenRefreshRetryHelper.cs`, `AccountConcurrencyGate.cs`) plus a scratch, untracked fork of
`il-patcher` with the new patch flags (`--patch-http-dns-connect-callback`,
`--patch-token-refresh-retry`, `--patch-account-concurrency-gate`, plus several diagnostic-only
flags) — none of it has been merged into the tracked `il-patches/il-patcher-Program.cs` or wired
into a release script yet. This report exists so the next session doesn't have to re-derive any
of this from scratch, and doesn't re-tread the dead ends (or the diagnostic-tooling bugs — see
IL-patching lessons 18-19 in CLAUDE.md) that were already found and fixed.

---

**Original status (earlier session, kept for history): root MECHANISM confirmed directly and
quantified, NOT fixed.** A live capture measured
a single Graph HTTP command (`GenericMSGraphCommand.Execute`, zero retries, one attempt) taking
**~46 seconds to return**, correlating exactly with an observed UI freeze window (see "Update:
bisection rounds" below) — no code-level bug required beyond "this call has no application-level
timeout, and Wine's confirmed `GetAddrInfoExW` cancellation gap can make it take however long it
takes." Two experimental mitigations (`ThreadPool.SetMinThreads`, `MaxConnectionsPerServer=4`)
were tried live and neither resolved it on their own — both documented below so they aren't
re-tried blindly. The most direct untried fix candidate at the time was: **add an explicit
`HttpClient` timeout** to the client(s) `MSGraphAccount.CreateGraphClient()` builds — this was
attempted and superseded by a better approach (see below).

## Symptom

`emClient_11_beta` (a Microsoft 365 / Graph API account, not classic IMAP/EWS) freezes — the
main window either shows a fixed image behind a "not responding" overlay or is visibly
unresponsive to input — most reliably right at "sync on load" (app startup), but also
recurring later during normal use. Severity and duration are highly variable: from a few
seconds to well over a minute, and in every case observed this session it **eventually
recovered on its own** without being force-killed — this is a severe throttling/contention
problem, not a hard deadlock. Reported by the user as "reliably worse" on this physical host
than in a VM running the same build and settings.

## Root cause, confirmed directly via a bracketed `CX_DEBUGMSG +winsock` trace

At the exact moment a Graph account goes online, **~10 concurrent `GetAddrInfoExW` calls for
`graph.microsoft.com` fire from 10 different threads in the same instant**, and Wine's own trace
shows, for these calls:

```
fixme:winsock:GetAddrInfoExW Unsupported namespace 0
fixme:winsock:GetAddrInfoExW Unsupported cancel handle
```

This is **the exact same underlying Wine defect** already documented in
`reports/exchange-sync-freeze-findings.md` (Wine's `GetAddrInfoExW` doesn't properly support
async DNS resolution's namespace or cancellation semantics) — that fix targeted the classic
EWS/IMAP pipeline's raw `Socket`/`Dns` calls; this is the same gap hit through **`HttpClient`'s
own DNS resolution** when the Microsoft Graph SDK makes its HTTP calls, and hit **far more
concurrently** (~10x at once vs. the old bug's one-at-a-time pattern with a 20s completion-port
fallback bounding the damage).

### Why it's concurrent: the startup fan-out, confirmed via decompile

`AccountManager`'s constructor brings every configured account online synchronously (still on
`Program.RunInitOnBackground`'s own `Task.Run` thread, not the UI thread). For a Graph account,
`GoOnline()` → `GoOnlineInt()` → (if `AutoSynchronize`, the default) fires
`Synchronize(SynchronizationPriority.BackgroundForced)` → `SynchronizeIntAsync(forced)`, which
fans out via `Task.WhenAll` into:

- `folderSynchronizer.SynchronizeSubfoldersAsync(...)`
- **one task per well-known special folder** (`MSGraphFolderSynchronizer.wellKnownFolderNames`
  — Inbox, Sent, Drafts, Deleted Items, Junk, Archive, Outbox, etc.)
- one task for `categoriesSynchronizer`

8–10+ concurrent `Task.Run` chains, all sharing **one `HttpClient`/`GraphServiceClient` instance
per account** (built once via `RefreshCredentials()`/`CreateGraphClient()`), fire essentially
simultaneously the instant the account goes online — and `MSGraphAccount.SynchronizeAsync`
additionally fans out over any sub-accounts (archive folders, public folders, delegated
mailboxes) on top of that.

### A genuine architectural fragility independent of Wine, found along the way

`MailClient.Protocols.MSGraph.dll`'s item synchronizers (`MSGraphMailItemSynchronizer`,
`MSGraphFolderSynchronizer`, `MSGraphItemSynchronizer<T>`, etc.) are riddled with
`.ConfigureAwait(false).GetAwaiter().GetResult()` — genuine synchronous blocking calls inside
paging/delta-sync/retry loops that look async on the surface. Each occupies a full ThreadPool
thread for the *entire* duration of its Graph HTTP call rather than releasing it during I/O
wait, unlike real end-to-end async. This is a real .NET anti-pattern in eM Client's own Graph
integration, not a Wine-specific bug — it would degrade under load on real Windows too, just
probably less visibly (Windows' `GetAddrInfoExW` cancellation actually works).

## What was ruled out this session (don't re-investigate these)

- **`onlineLock` (a `System.Threading.Lock` on `AccountBase`) held across a blocking call while
  the UI thread waits on it** — the leading hypothesis for a while, DISPROVEN via live
  instrumentation (see Diagnostic tooling below): `GoOnlineInt()` was measured completing in
  3–105ms across every capture. The lock is not held for anything close to freeze-duration
  timescales.
- **`ReadUserProfile()`'s Graph call blocking `GoOnlinePre()`** — checked via decompile;
  `RunRequest(...)` there is genuinely fire-and-forget, `GoOnlinePre()` returns immediately.
  Not a factor.
- **Broad ThreadPool exhaustion as the sole mechanism** — `ThreadPool.SetMinThreads(64, 64)`
  (pre-warming the pool to remove the slow ~1-thread/500ms-1s injection ramp documented in
  CLAUDE.md's IL-patching lesson 14) was deployed and tested live. The freeze still occurred
  with the pool pre-warmed, and thread snapshots kept showing just ONE thread busy with
  everything else idle/parked — the signature of one slow serialized operation, not broad
  starvation. Ruled out as sufficient on its own (see "Mitigations tried" below — it may still
  be marginally useful, just not sufficient).
- **An early-startup hang before `formMain`/`OnShown` ever fires, with every thread completely
  idle** — captured once, looked like a totally different and simpler bug at first. Turned out
  to be **a bug in this session's OWN diagnostic instrumentation**, not a real product issue:
  `AccountManager` checks accounts via `Parallel.ForEach` internally, and an early revision of
  the `--patch-diag-onlinelock` tool called `File.AppendAllText` directly from 8+ concurrent
  threads, which threw `IOException`/`AggregateException` ("file being used by another
  process") and crashed the app for real (confirmed via the app's own `bug.*.txt` crash report).
  Fixed by centralizing all diagnostic writes through one `try/catch`-wrapped static helper
  (`__safeAppendDiagLog`, swallows write failures under contention rather than crashing) — see
  Diagnostic tooling below. **If a future session sees this exact "everything idle before the
  UI even shows" pattern again, check whether a diagnostic patch is active and buggy before
  assuming it's a new product bug.**

## Mitigations tried live against `emClient_11_beta` — both INCONCLUSIVE/INSUFFICIENT

Neither is part of `il-patches/il-patcher-Program.cs` (the tracked release tool), `deploy.sh`,
or any release line. Both were built as one-off scratch patches (a session-local fork of the
patcher, not saved anywhere in the repo) and applied directly to the live bottle's `MailClient.dll`
files on top of release/11.0.196-4, purely to test hypotheses. **They should not be assumed
present in the bottle by a future session** — the bottle's live files are outside git, and unless
explicitly redeployed they may already be gone by the time this is read.

1. **`ThreadPool.SetMinThreads(64, 64)`** — inserted as the literal first statement of
   `MailClient.Program.Main`, to pre-warm the pool before the startup sync burst. Tested live,
   twice (with and without host CPU/memory contention from a second VM running on the same
   physical host). The freeze still occurred both times, with the same "one thread busy, rest
   idle" signature — inconsistent with pool exhaustion being the (sole) mechanism.
2. **`SocketsHttpHandler.MaxConnectionsPerServer = 4`** — set on every `HttpClient`
   `MailClient.Protocols.InteractionController.CreateHttpClient` builds (reached via the app's
   own existing reflection pattern for reaching a `HttpClientHandler`'s private `Handler`
   property — confirmed safe by the app's own pre-existing, shipped `authLog`-gated code doing
   the exact same thing nearby). The intent: since one account's 8-10 concurrent per-folder sync
   requests all share ONE `HttpClient`, capping its connection ceiling should throttle how many
   can open a brand-new connection (and thus fire a brand-new, Wine-broken `GetAddrInfoExW`
   call) at once, without needing to touch the sync fan-out logic itself (staggering the fan-out
   was explicitly rejected by the user as not scaling with account count). Tested live: **the
   freeze still occurred, with the identical thread signature.** Not confirmed to have made
   things better OR worse — no rigorous A/B was done, and host contention was a confound across
   the test runs.

Both are cheap, safe, plausible, and neither was suficient alone. If picking this back up: it
may be worth keeping both (they're low-risk) while looking for the actual remaining mechanism,
rather than reverting them — but don't expect them to be the fix.

## The open question: why the UI thread specifically freezes

Every thread-state capture this session showed a **single background `.NET TP Worker` thread
pegged at 70-90% CPU** while the UI thread (`CrBrowserMain`) sat at low CPU, parked in Wine's
`ntsync_schedule` wait state — the natural reading ("busy background thread, UI thread stuck
waiting on something from it") was never fully nailed down to a specific lock or call, despite
extensive searching:

- **`onlineLock` contention** — checked and ruled out (see above): the lock isn't held for
  freeze-duration timescales.
- **Classic WinForms `Control.Invoke`-based marshaling backpressure** (background thread
  synchronously pushing per-item UI updates faster than the UI thread can drain them) — the
  leading remaining hypothesis, strengthened by one specific late capture: **the UI thread
  itself was caught in the `R` (running, not parked) state at 18.3% CPU**, simultaneous with two
  `Microsoft Graph`-named threads at 54.8% and 8.9% — the first time this session the UI thread
  was confirmed doing real work rather than sitting idle. This is consistent with the UI thread
  being saturated processing a backlog of legitimate rendering/update work, not blocked on a
  lock. However, an extensive search (the shared `ControlDataGrid` grid control, the mail list
  view `ControlMainViewMail`, ~140 call sites of the app's own `Marshalling.InvokeForm`
  synchronous-marshal helper) did not turn up a clear single "per-item synchronous Invoke" call
  site feeding this. **Not confirmed — the exact code path was not found before this
  investigation session ended.**
- **Important caveat discovered along the way**: `ps -L`'s `WCHAN=ntsync_schedule` does NOT by
  itself distinguish a genuinely stuck thread from a perfectly healthy, idle UI thread — a
  responsive WinForms UI thread's own `GetMessage`/`WaitMessage` idle wait goes through the same
  underlying Wine NT-synchronization wait state. Every thread-state capture in this
  investigation needs to be read with that in mind; only the heartbeat-based captures (below)
  give an unambiguous signal.

## Diagnostic tooling built this session (not merged into any tracked file)

All of the following are scratch, one-off additions — a session-local fork of
`il-patches/il-patcher-Program.cs` and edits to the deployed `emclient-runner.sh` copy on the
remote bottle's own `drive_c/Temp/`. None of this is tracked in git. If a future session needs
to resume this investigation, these need to be rebuilt from the descriptions here (the exact
technique/insertion points are documented so they can be reconstructed quickly, same convention
as `project_emclient_runner_script.md`'s memory entry for the original marker-file runner):

- **`--patch-ui-heartbeat`** — adds a `System.Windows.Forms.Timer` to `formMain`, ticking every
  250ms, writing an incrementing counter to `C:\Temp\claude-heartbeat` (same insertion pattern
  as the tracked `--patch-close-listener`: prepend to `OnShown`). This is the single most useful
  diagnostic added — it settles definitively whether the UI thread's message pump is genuinely
  stalled vs. just slow, which `ps -L`'s WCHAN cannot. Confirmed two distinct behaviors across
  captures: completely stopped (a real, if temporary, stall) and severely throttled (ticking
  ~1/30th normal rate, i.e., roughly one tick per several seconds instead of every 250ms) — the
  freeze is closer to severe starvation than a hard deadlock, matching that it always eventually
  recovered.
- **`--patch-diag-onlinelock`** — two instrumentation points in
  `MailClient.Protocols.Common.AccountBase` (in `MailClient.Protocols.dll`): an entry-only log
  on `get_IsOnline` (thread id + tick), and an ENTER/EXIT pair bracketing the `GoOnlineInt()`
  call inside `ChangeOnlineState` (thread id, tick, elapsed ms). Both log to
  `C:\Temp\claude-diag.log` through a shared `__safeAppendDiagLog(string)` helper added to
  `AccountBase` — a small `try { File.AppendAllText(...); } catch (Exception) { }` wrapper. This
  wrapper is not optional: seeAppendAllText is not safe for the concurrent multi-writer access
  `AccountManager`'s internal `Parallel.ForEach` account-checking produces, and calling it
  directly caused a real, confirmed crash (see "What was ruled out" above). A dropped log line
  under contention is an acceptable tradeoff for a diagnostic; crashing the app under
  investigation is not.
- **`--patch-close-listener`** (restored from git history, commit `2993a03`, NOT the tracked
  current source — it was intentionally removed from the tracked release tool) — same mechanism
  as before (polls `C:\Temp\claude-close-signal`, calls the real `menuItem_File_Exit_Click` for
  a graceful shutdown indistinguishable from File > Exit), with one adaptation: the watched path
  changed from the original v10 investigation's global `Z:\tmp\claude-close-signal` to
  bottle-local `C:\Temp\claude-close-signal`, matching the CURRENT `emclient-runner.sh`'s own
  `TEMP_DIR` convention (bottle-local, not global `/tmp`) — that convention didn't exist yet
  when the original patch was written. Confirmed both type (`MailClient.UI.Forms.formMain`) and
  method (`menuItem_File_Exit_Click`) are unchanged in eM Client 11 before adapting.
- **`emclient-runner.sh` additions** (on the remote bottle's own copy, not the tracked source in
  `il-patches/`... actually not tracked anywhere at all, per `project_emclient_runner_script.md`
  — this session's changes need to be manually re-applied to that memory file's source if they
  should persist):
  - **`runner-threaddump`** — one-shot `ps -L -o pid,tid,pcpu,pmem,stat,wchan,comm` sweep of
    every process in the bottle, no relaunch/tracing needed. The cheapest, fastest, most-used
    diagnostic this session — always reach for this first.
  - **`runner-native-stack`** — attaches `gdb` briefly to the main eM Client process only
    (explicitly excludes both `winewrapper.exe` and any CEF `--type=...` helper — a real bug hit
    live: the first version attached to `winewrapper.exe` instead of the actual main process,
    since `find_running_pids` matches ANY cmdline containing "MailClient.exe", which
    `winewrapper.exe --start -- C:\...\MailClient.exe`'s cmdline also does). Needs
    `kernel.yama.ptrace_scope=0` (or root) on the host — the default `ptrace_scope=1` blocks a
    non-ancestor process from attaching. **Limited value in practice**: this is a 32-bit Wine
    build with no debug symbols for `ntdll.so`/the vsyscall trampoline, so every thread's
    backtrace bottoms out after 2-4 frames at `__kernel_vsyscall` with no further symbol
    resolution — confirms *that* every thread is in some native wait, never *which* one
    specifically. A real symbol-resolved managed stack (e.g., via `dotnet-dump`/SOS, untested
    this session — likely complicated by this being a Windows-PE CoreCLR process under Wine
    emulation rather than a native Linux ELF one) would be needed to go further than the
    heartbeat/diagnostic-log approach above.

## Update: found the real choke point candidate, and got a definitive UI-thread capture

A later session picked this back up and found the actual mechanism behind the "one thread busy,
UI parked" signature: **`MailClient.Storage.Application.TaskQueue`** — a single, dedicated
background thread (named `"EventScope TaskQueue"` internally, though that name doesn't reliably
propagate to the Linux `comm` field for a Windows-PE process under Wine, so it shows up as a
plain, unnamed `MailClient.exe` thread in `ps -L`) that processes **every**
`NewItems`/`RemoveItems`/`RefreshItems` notification for **every item type in the whole app**
(mail, contacts, events, tasks, notes), one action at a time, off one shared `Queue<Action>`:

```csharp
public class TaskQueue {
    Thread thread;  // "EventScope TaskQueue"
    Queue<Action> queue;
    // loop: lock, wait for work, dequeue ONE action, run it, repeat -- strictly sequential
}
```

Every `Folder.MailItems.NewItems`/etc. raise funnels through `Folder.taskQueue.Enqueue(...)`,
which just runs the subscriber's callback on this one thread. If any subscriber does a
synchronous UI-thread `Invoke` in response, this thread is the strict serialization point for
the entire app's item-update pipeline — worth targeting directly instead of searching UI code
for the marshal call.

**Instrumented directly** (`--patch-diag-taskqueue`, scratch/session-local, not tracked): logs
every `Enqueue` (thread, tick, queue depth) and brackets every dequeued `action()` call
(thread, elapsed ms) to `C:\Temp\claude-diag2.log`. Hit one real IL-patching bug building this,
worth recording as a new lesson: the dequeue loop's `ldloc.0` (loading the dequeued action) is
both a `leave` branch target *and* the method's own `finally` handler's `HandlerEnd` boundary —
inserting new code before it without also reassigning `HandlerEnd` (not just retargeting the
branch) silently grew the finally handler's range to swallow the new code, which decompiled as
`ilspycmd` CFG-resolution errors ("Could not find block for branch target", "Discarded
unreachable code") rather than a clean decompile — a variant of IL-patching lesson 3 worth
adding to CLAUDE.md if this diagnostic pattern is ever reused: **a single instruction can
simultaneously be a branch target AND an exception-handler boundary marker, and both need
retargeting, independently, when inserting before it.**

Live data collected at startup was healthy (individual task durations of 2-20ms, queue depth
never exceeding 3) — so the TaskQueue itself was NOT backed up in that specific capture window.
But a later freeze in the SAME session produced the most conclusive single capture of the whole
investigation:

```
1155672 1155672  77.7  1.0  R  -                CrBrowserMain    <- the process's own PID/main thread
```

**The UI thread itself — confirmed by TID matching the process's own PID, not just a
similarly-named thread — was caught actively running (state `R`, not parked, `WCHAN=-`) at
77.7% CPU**, with **no "Microsoft Graph" threads present at all** in that snapshot, and the
TaskQueue diagnostic log had gone quiet (not backed up). This is the clearest evidence gathered
this investigation: at least some of these freezes are the UI thread **directly executing
expensive synchronous work itself** — not blocked on a lock, not waiting on the TaskQueue, not
waiting on a Graph network call. Something CPU-heavy runs right there on the UI thread's own
call stack.

This still doesn't pin the exact method — that would need either a symbol-resolved native/managed
stack sample taken at the right instant (not available with the tooling on hand this session —
see the `gdb`/native-stack limitations below) or further, more targeted instrumentation directly
inside UI rendering code (`ControlDataGrid`'s paint/refresh entry points would be the next place
to add timing, rather than more thread-state snapshots, which have reached their information
ceiling for this specific question).

## Update: bisection rounds — the real mechanism, measured directly

A follow-up session ran a systematic set of bisection experiments (deliberately disabling one
mechanism at a time and re-testing, rather than only observing passively) plus one more layer of
instrumentation. This settled several previously-open questions with real numbers instead of
inference.

**Round 5 — disable delta-sync entirely (`--patch-bisect-disable-deltasync`, a single `ret`
prepended to `IterateDeltaPages`, making it a full no-op for every item type):** the freeze
still happened at least once with delta-sync completely disabled, and "Microsoft Graph" threads
were still present and active — proving delta-sync is not the *only* path capable of triggering
the freeze. But the same build then ran dramatically longer without freezing than any prior
un-bisected run (over a minute of healthy, climbing heartbeat before the next freeze), suggesting
delta-sync is a major contributor to *frequency* even though it isn't the sole cause. This reads
as a probabilistic trigger (whichever Graph command's network call happens to hit the Wine bug at
a given moment), not a deterministic one tied to one specific code path — consistent with the
user's own expectation going in that timing/races would make this look like several different
things happening depending on when you look.

**Round 6 — instrument the shared retry wrapper (`--patch-diag-graphcommand`,
`GenericMSGraphCommand.Execute`):** every single Graph command (not just delta-sync — folder
listing, profile reads, categories, everything) funnels through this one `while(true)` retry
loop. On `HttpRequestException` wrapping an `IOException` (exactly what Wine's broken
`GetAddrInfoExW`/connection layer would produce) it retries up to 5 times with **no delay at
all**; on 5xx/408/429 it does a real blocking `CancellationToken.WaitHandle.WaitOne(500 *
attemptNumber)` backoff (up to ~7.5s cumulative). Live capture showed **zero retries fired** —
every logged attempt was `retrycount=0` — ruling out the retry/backoff loop itself as the
mechanism. What it *did* reveal directly: a single command attempt (`tid=48`, one specific
`Execute()` call, `retrycount=0`) logged at one tick, and the next log line — the very next
command starting anywhere in the app — didn't appear until **46165ms (~46 seconds) later**. No
retry, no backoff, no second attempt of the same command — just one single Graph HTTP call that
took ~46 seconds to return, blocking whatever thread was waiting on it for that whole time. This
lines up exactly with the observed freeze window (heartbeat stuck, then resuming healthy
immediately after). **This is the most direct, quantified evidence gathered in this
investigation**: one in-flight Graph command, one very long real-world stall, no code-level bug
required beyond "this call has no timeout and Wine's DNS/connection layer can make it take
however long it takes."

A genuine IL-patching bug was hit and fixed while building round 6's instrumentation — see
IL-patching lesson 19 in CLAUDE.md: a static helper added to a generic type
(`MSGraphItemSynchronizer<TItem,TStorageItem,TGraphItem>`) must be called through that type's own
generic parameters as the instantiation, not the bare open generic definition — the wrong version
decompiled and built cleanly but threw `InvalidOperationException` at runtime, visibly (the user
caught it via the app's own exception-reporting dialog), and — worse — silently disabled the
instrumented code path entirely, which could easily have been mistaken for "the freeze is fixed."

### Practical implication

Since the root mechanism is "a single Graph HTTP call with no application-level timeout, made
through a networking layer with a known, confirmed Wine cancellation gap" — the most direct real
fix candidate (not yet attempted) is adding an explicit timeout to the `HttpClient`(s)
`MSGraphAccount.CreateGraphClient()` builds, short enough that a hung call fails and gets
reported/retried in a few seconds rather than blocking for up to a minute. This wouldn't fix the
underlying Wine defect, but would bound its damage the same way `--patch-account-manager-sync-
async`/`--patch-folder-sync-async` (Stage 15/Stage 4) bounded the original Exchange-sync bug's
damage — not by fixing Wine, but by making sure nothing waits on it forever.

## Confirmed: not limited to startup

A later freeze was captured during ordinary steady-state use (well after startup, "doing a
sync" per the user, no new `GoOnlineInt`/`IsOnline` diagnostic activity logged at all) —
confirming this isn't purely a "sync on load" phenomenon. Heartbeat crawled from 437 to 438 over
6 seconds (same severe-throttling signature as every startup freeze). The thread snapshot showed
the same overall shape but with plain `MailClient.exe`-named threads (not yet CEF/`.NET`-renamed)
also busy alongside `Microsoft Graph` and `.NET TP Worker` threads, and — notably — TWO different
threads both named `CrBrowserMain` (one at 36.9%, the process's actual main thread at only 6.6%
this time), a reminder that thread *names* alone aren't a reliable way to identify "the" UI
thread; the owning TID (matching the process's own main thread ID, shown as the PID in `ps -L`)
is the only reliable identifier.

## Host environment as a severity amplifier (confirmed, not the root cause)

The user reported this bottle performs "reliably worse" on this physical host than in a VM with
identical settings. `runner-sysload` confirmed real contention: 16 CPUs, load average 9.2,
**7.8GB of swap actively in use**, and other VMs (`qemu-system-x86`) consuming 42%+8% CPU
concurrently. After the user shut down the heaviest other VM, load dropped to 4.93 and swap
usage roughly halved (5.1GB) — and the freeze was reported as "mildly better" but **still
occurred**. Host contention is a real, confirmed severity multiplier (a slower/contended CPU
makes each already-slow Graph/DNS-bound operation take even longer, and swapping introduces
its own unpredictable multi-hundred-ms latency spikes) but is not sufficient by itself to
explain the freeze — it still reproduces reliably on an otherwise-idle host, just less severely.

## Recommended next steps, in rough order of expected value

1. **Find the actual UI-thread marshaling call site.** The late capture showing the UI thread
   itself busy (not parked) is the strongest lead left unresolved — confirming what specifically
   it's doing (rendering a backlog vs. something else) would likely settle the remaining
   mystery. Worth a fresh, more systematic pass (e.g., instrumenting `ControlDataGrid`'s own
   refresh/invalidate entry points directly with the same heartbeat-adjacent technique, rather
   than keyword-searching for the call site) rather than more `gdb`/`ps` capture cycles, which
   have hit their information ceiling.
2. **Test on real Windows if at all possible.** This would immediately settle whether the
   sync-over-async architecture problem alone (independent of the Wine `GetAddrInfoExW` gap) is
   enough to cause visible freezes there too, which would mean this is at least partly a genuine
   eM Client bug worth reporting upstream, not something a Wine-side patch could ever fully fix.
3. **If continuing to patch around it**: the `GetItemContent` unbounded `Monitor.Wait` (no
   timeout at all, found via decompile — see the "sync-over-async" architecture discussion
   above) is a second, narrower, independently-worth-fixing bug regardless of the main freeze's
   resolution — a UI-thread read of undownloaded message content can hang forever with zero
   safety net. Not yet patched or tested this session.

---

## Session N: DNS bypass and bounded socket I/O — the first real fix

Picked the "add an explicit HttpClient timeout" idea back up, but rejected a naive
`HttpClient.Timeout`/`CancellationToken`-based approach up front: that mechanism depends on
exactly the same cancellation machinery Wine's `GetAddrInfoExW` gap already breaks, so it might
not actually unblock a hung call at all. Instead, applied the same strategy that already worked
for the *original* Exchange-sync bug (bypass the broken async API rather than try to cancel it)
one level lower in the stack, at the actual HTTP transport layer.

### Fix 1 — `DnsConnectHelper` / `DnsCache` (`il-patches/MailClient.Wine/DnsConnectHelper.cs`)

A custom `SocketsHttpHandler.ConnectCallback`, wired in via a new IL patch
(`--patch-http-dns-connect-callback`, scratch tool only) into
`MailClient.Protocols.InteractionController.CreateHttpClient` — **the single shared `HttpClient`
factory every protocol in the app uses** (IMAP, Exchange, AirSync, CalDav, WebCal, MSGraph, chat/
cloud-storage connectors, confirmed via decompile to be referenced from every protocol assembly),
inserted right next to an existing reflection-based `MaxConnectionsPerServer` tweak already living
in that exact method (so the reflection pattern needed was already proven safe in this codebase).
Deliberately universal, not Graph-specific, since the underlying Wine defect is systemic.

- `DnsCache`: resolves a host via the older, synchronous `Dns.GetHostAddresses` (hypothesis: this
  goes through Wine's plain `getaddrinfo`, not the broken async/cancellable `GetAddrInfoExW`) on a
  background thread (`Task.Run`), caches the result (5 min TTL, 500-entry cap — cardinality is
  bounded by "distinct servers this user's accounts talk to," not per-email or otherwise
  unbounded), and dedupes concurrent lookups for the same host via
  `ConcurrentDictionary<string, Lazy<Task<Entry>>>` (plain `GetOrAdd` doesn't guarantee its factory
  runs only once under a race; `Lazy` does).
- `ConnectAsync`: resolves via the cache, then connects to the resolved IP directly via a plain
  `Socket`, so `SocketsHttpHandler`'s own internal DNS step (the broken one) never runs at all for
  a cached host.

**Verified live**: DNS+connect dropped from the previously-measured ~46-second hangs to a
consistently fast 40–120ms, across many captures.

### Discovery: the hang didn't go away, it moved

With DNS+connect fixed, a **~59.7-second** hang still occurred on a live capture, with the
`claude-dnsconnect.log` diagnostic proving DNS and connect were both fast throughout that exact
window — the hang had moved to *after* a successful connect: TLS handshake, request send, or
response read, none of which `ConnectCallback` touches. This makes sense: Wine's broken
async-cancellation almost certainly isn't DNS-specific, it's a general property of whatever
overlapped-I/O/completion-port mechanism backs .NET's async socket operations
(`Socket.ReceiveAsync`/`SendAsync`, which is what a plain `NetworkStream` uses under the hood) —
directly consistent with the *original* Exchange-sync bug's own description ("could block on a
20s completion-port fallback"), which was never actually DNS-specific either.

### Fix 2 — `TimeoutBoundedNetworkStream` (same file)

Instead of returning a plain `NetworkStream` from `ConnectAsync`, returns a custom `Stream` whose
`Read`/`Write` — both the sync AND async overloads, including the `Memory<byte>`-based ones
`SslStream`/`HttpClient` internals actually call — go through a genuinely **synchronous**
`Socket.Receive`/`Send`, bounded by `SO_RCVTIMEO`/`SO_SNDTIMEO` (`Socket.ReceiveTimeout`/
`SendTimeout`, 30s), wrapped in `Task.Run` purely to satisfy the `Stream` async API surface for
callers above it. Same "avoid the broken async path entirely" strategy as the DNS fix, one layer
deeper — `SO_RCVTIMEO`/`SO_SNDTIMEO` are old, simple, synchronous-blocking socket options, not
part of the overlapped/cancellation-token family already confirmed broken.

**Verified live, decisively**: a real ~30s stall occurred reading from `login.microsoftonline.com`
(a token-refresh call, see below); the timeout fired at almost exactly the configured 30000ms
(`elapsedMs=30001`/`30000`/`30001` across four threads), producing a normal `SocketException`
(`WSAETIMEDOUT`, Win32 code `0x274c`) instead of hanging indefinitely — **and the UI heartbeat
diagnostic never stopped ticking through that entire event.** The user independently confirmed
this live: manually triggering a full sync (previously a reliable freeze trigger) no longer froze
the UI.

Also observed, and worth noting as benign: a cluster of `SocketException` `0x2745`
(`WSAECONNABORTED`) on ordinary HTTP keep-alive connection recycling — fast (0–200ms), not a
stall, not a concern.

### Functional-correctness check: does a bounded timeout actually break anything?

Directly asked and traced, not assumed: when the 30s timeout fires mid-request, what happens to
that request?

- **Graph API calls** (`graph.microsoft.com`, via `GenericMSGraphCommand.Execute`) have real retry
  logic already (5 attempts, catches `HttpRequestException` wrapping `IOException`) — a timeout
  here should be retried transparently. Not exhaustively proven end-to-end this session, but the
  shape matches.
- **OAuth2 token refresh** (`login.microsoftonline.com`, via
  `MailClient.Accounts.Credentials.GetAccessTokenRefreshResponse`) — confirmed via decompile to
  have **zero retry for a network failure**: its only catch clauses are for
  `UntrustedCertificateException` (its own separate trust-decision retry loop) and a generic
  `catch (Exception ex2) { log; throw; }` for everything else, including our new timeout. Confirmed
  live: the observed 30s `WSAETIMEDOUT` events on this exact host correlated with **zero retries
  logged anywhere**, matching the decompiled code exactly. This is a real, separate, narrower gap
  — a token-refresh timeout previously failed hard with no recovery at that layer (whatever
  higher-level "mark account as connection-error, retry sync later" mechanism exists was the only
  safety net, and wasn't traced this session).

### Fix 3 — `TokenRefreshRetryHelper` (`il-patches/MailClient.Wine/TokenRefreshRetryHelper.cs`)

A small, narrowly-scoped retry wrapper (3 attempts, 2s delay, only for
`SocketException`/`IOException`/`HttpRequestException`/`TaskCanceledException` — deliberately not
a generic retry utility) wired in via a new IL patch (`--patch-token-refresh-retry`, scratch tool
only). `GetAccessTokenRefreshResponse` is `async`, which makes hand-authoring IL impractical (a
real compiler-generated state machine) — instead, the exact "move the existing body to a new
method, give the original name a brand new, small wrapper body" technique already used for the
classic engine's sync-freeze fix (CLAUDE.md IL-patching lesson 13) was applied here too, just for
the first time on an *async* method: the original method's entire existing body (its
state-machine-kickoff boilerplate, which turned out to have zero exception handlers of its own —
the real try/catch/finally lives in the compiler-generated `MoveNext()`, untouched by this patch)
moved verbatim onto a new `__GetAccessTokenRefreshResponseCore` method, and the original name got
a new one-line body: `return TokenRefreshRetryHelper.WithRetryAsync(__GetAccessTokenRefreshResponseCore, parameters, cancellationToken);`.
Verified via decompile (moved body is byte-for-byte the original) and `--dump-handlers` (zero
handlers on the new wrapper, the original's full handler table intact and unchanged on the moved
core method).

Deployed live; not yet stress-tested specifically for this path recovering correctly under a real
token-expiry-during-heavy-load scenario (the session moved on to the concurrency investigation
before this could be isolated).

### New IL-patching technique note (not yet promoted to a numbered CLAUDE.md lesson)

**A public method on an `internal` type is still only as accessible as the type itself from
outside the assembly** — hit for real deploying `DnsConnectHelper`: declaring the type `internal`
(matching this project's usual "smallest necessary visibility" instinct) decompiled clean, built
without error, and crashed on the very first launch with `System.MethodAccessException` the
instant `MailClient.dll`'s `CreateHttpClient` tried to call it. Same "decompiling clean is not
sufficient proof" shape as several other lessons in CLAUDE.md, new trigger. Fix: the type itself
must be `public` if any of its members are called from another assembly, regardless of the
member's own declared accessibility.

## Session N: architecture investigation — is folder-sync concurrency the problem?

The user asked, at a high level, whether multiple folder syncs run in parallel per account, and
proposed capping concurrency if so. This was investigated properly rather than assumed, and the
first answer given was **wrong and had to be retracted** — worth recording the correction, not
just the final answer:

- **Confirmed via decompile**: `Command.Process()` (`MailClient.Commands.dll`) is genuinely
  synchronous and serial within a `SingleSynchronizationQueue`'s own dispatch loop — the loop
  calls `command.Process(status)` and blocks on it before dequeuing the next command, for both the
  normal per-account worker-thread path and the "run in place" fast path. No `Task.Run`, no
  fire-and-forget. This is real, and means a single account's own queue cannot run two of its own
  commands concurrently.
- **But live evidence clearly showed concurrent Graph network calls** across many different
  managed thread IDs at the same instant — which looked, at first, like it contradicted the above.
- **Resolved by adding a new diagnostic** (`--patch-diag-graphcommand-account`, logs which
  account each `GenericMSGraphCommand` attempt belongs to) and reading the live log directly:
  **every managed thread ID mapped to exactly one, consistent account** (e.g., tid=44 was always
  "Personal", tid=48 always "Accounts") across the whole capture — never two accounts sharing a
  thread, never one account's work split across threads. This account has **9** distinct
  account/pseudo-account labels (some are archive-folder/public-folder pseudo-accounts per
  `MSGraphAccount`'s `GraphAccountType`, not literally 9 separate mailboxes), each with its own
  dedicated `SingleSynchronizationQueue` instance and worker thread. So the "concurrency" observed
  was **real, but entirely cross-account**, not intra-account — each account is already fully
  serialized internally, exactly as the decompile predicted; what looked like a contradiction was
  simply several different accounts' independent, correctly-serial pipelines overlapping in
  wall-clock time in an aggregate log that didn't distinguish which account each line belonged to.

**Lesson for future sessions**: when live evidence seems to contradict a decompile-confirmed
mechanism, look for a confound (here: multiple independent instances of the "serial" thing) before
concluding the mechanism is wrong. The first-pass answer ("yes it's parallel, let's cap it") given
to the user before this diagnostic was run was retracted once actually checked.

## Session N: a third, different freeze mechanism — CPU-bound, not I/O-bound

With the DNS and socket-timeout fixes deployed, freezes still occurred under heavy load (user
manually triggered "sync all folders for offline use" across all 9 accounts). Captured live with a
two-sample `ps -L` delta (a single snapshot's `%CPU` is a lifetime average, useless for spotting an
instantaneous spike — two samples a few seconds apart, comparing `STAT`/`WCHAN`, is what actually
worked):

- **Thread `MailClient.exe` TID 1548957**: genuinely `STAT=R` (running, not sleeping),
  `WCHAN=-` (not blocked on anything), sustained ~70% CPU across both samples, for well over a
  minute continuously. Not a ThreadPool worker (those show up separately as named ".NET TP
  Worker") — a dedicated thread, consistent with one of the per-account
  `SingleSynchronizationQueue` worker threads found above.
- A `gdb thread apply all bt 32` capture of this exact thread showed a genuine, actively-executing
  **32-frame-deep managed call stack** (no symbols resolvable — JIT'd code under Wine has none
  gdb can read — but real, live, unresolved addresses, not a wait-state top frame). The raw
  addresses show real recursion: `0x064eeb4a`/`0x064eac7d` repeat as a pair at two different
  points in the stack, and `0x135bf6a5` repeats — a small cycle of methods calling into each other
  repeatedly, not a single long linear chain.
- Simultaneously, a **second thread, in CEF's own `CrBrowserMain` process**, was at ~34% CPU,
  also genuinely running.

**Conclusion**: this is a real, different mechanism from the DNS/socket ones — a CPU-bound,
apparently-recursive computation on a per-account sync worker thread (triggered specifically by
"sync all folders for offline use," i.e. a deep folder-walk + full message-download cycle),
sustained long enough and combined with CEF's own concurrent CPU use that the UI thread gets
starved of scheduling — the "OS-level scheduling starvation" hypothesis from much earlier in this
investigation, now with a concrete, reproducible trigger and a (symbol-less but real) call stack
showing genuine recursion, not just inference from `ps` output. **The exact method could not be
identified** — no usable symbols were obtainable for JIT'd Wine-hosted CoreCLR code via `gdb`, and
`dotnet-dump`/ClrMD were not attempted (see "Tooling notes" below for why that avenue looks
unpromising here).

### Two process-architecture facts discovered while chasing this (both real, both matter for
future tooling)

1. **eM Client 11's CEF integration runs as TWO separate top-level OS processes with the
   IDENTICAL command line** (`C:\Program Files (x86)\eM Client\MailClient.exe`, no `--type=`) —
   one is the real .NET/WinForms host (holds the UI thread, all the per-account sync threads),
   the other is CEF's own `CrBrowserMain` browser-process (Chromium's multi-process architecture
   always spawns a distinct browser process, even embedded via CefGlue). `find_running_pids`
   cannot tell them apart by cmdline. A native-stack capture that only grabs "the first match"
   can silently attach to the wrong one — hit for real, fixed by capturing **both** (see Tooling
   notes). This also means the earlier "TID == PID row is the UI thread" heuristic (used
   elsewher in this report, e.g. the "Update: found the real choke point" section above) is only
   reliable for identifying *a* main thread, not necessarily *the* WinForms UI thread specifically
   — it could be either process's own main thread depending on which PID you're looking at.
2. **Wine's `GetCurrentThreadId()` returns a Wine-internal Windows-style thread ID, not the real
   Linux kernel TID `ps -L`/`gdb` actually use.** Tried adding a P/Invoke
   (`--patch-ui-thread-tid`, writes the result to `C:\Temp\claude-ui-thread-tid.txt` once at
   `OnShown`) specifically to close the long-standing "`Environment.CurrentManagedThreadId` vs OS
   TID" gap noted earlier in this report — it does return a stable value, but that value (e.g.
   `1224`) never appears anywhere in the real `ps -L` output for that process, whose TIDs are all
   in the hundreds-of-thousands range. **This approach doesn't achieve its goal** — don't rely on
   it for cross-referencing a managed thread to a native TID. The `TID == PID` heuristic (point 1
   above, with its own caveat) remains the best available option.

## Session N: the concurrency-gate experiment — tried, made things worse, reverted

Given the CPU-bound finding above, and 9 accounts each getting a dedicated worker thread, the user
proposed a hard cap on how many accounts' worker threads can be doing real work at once
(independent of the per-account serialization already confirmed to exist), starting at 1 and
raising until instability returns.

### Implementation: `AccountConcurrencyGate` (`il-patches/MailClient.Wine/AccountConcurrencyGate.cs`)

A `SemaphoreSlim`-based gate wired around `Command.Process()` itself (via a new IL patch,
`--patch-account-concurrency-gate`, same "move body, new wrapper" technique as the token-refresh
fix, needed here too since `Process()` has **six** nested exception-handler regions of its own —
directly editing inside it would be real risk per CLAUDE.md lessons 3/18). Chosen specifically
because `Process()` is the one chokepoint every queue implementation and both dispatch paths
(normal + "run in place") already funnel through, so gating it caps concurrent execution app-wide
regardless of protocol, with zero changes to the already-correct per-account serialization.
Deliberately configurable via a plain text file (`C:\Temp\claude-account-concurrency.txt`, read
once at class-load time) rather than hardcoded, specifically to support the user's planned
iterative testing (change the file, relaunch, no rebuild needed).

### Results, in order tried

- **`limit=1`**: **catastrophic.** A real, near-permanent deadlock — confirmed directly via the
  gate's own logging, which recorded threads waiting up to **35,952,811ms (~10 hours)** for a
  slot. Root cause: something in the app has one `Command` waiting on another `Command`'s
  completion (the already-known `SingleSynchronizationQueue.DisposeQueue`'s `command.Wait()` is
  exactly this shape), and with only one global permit, that's unresolvable by construction — the
  one active command can never release its slot because whatever it's waiting on can't even start.
  Two separate `runner-panic` (`kill -9`) attempts **both silently failed** to actually terminate
  the stuck process (a real, separate bug — see Tooling notes) — it kept running, still deadlocked,
  for the entire ~10-hour gap until a *different* code path (the graceful-close fallback's plain
  `kill`, not `-9`) finally succeeded.
- **`limit=2`**: no deadlock — recovered on its own every time. But real, substantial freezes still
  occurred under heavy "sync all" load: one measured at **~4 minutes**, with `CrBrowserMain`
  climbing to 73%+ CPU throughout. Wait times in the gate log grew to 60–120s during the backlog
  but always eventually cleared. A genuine improvement over `limit=1` (bounded instead of
  effectively infinite) but not a fix.
- **`limit=4`**: **worse, not better** — a freeze lasting **4.5+ minutes with no sign of recovery**
  when observation stopped (host CPU climbing to 94.8%+ throughout, no gate-log progress the whole
  time), directly contradicting the "maybe there's a minimum thread count needed" hypothesis that
  prompted trying a higher limit. More concurrent heavy account threads competing for the same
  finite CPU cores appears to make the starvation *worse*, not better, at least under this
  specific synthetic "sync everything at once" stress test.

**Reverted**: limit set to `999` (i.e., large enough to never actually constrain anything in
practice — chosen over fully removing the IL patch to avoid another deploy cycle, since the gate
code itself is inert at this limit) rather than left at any of the tested values, pending further,
more careful analysis. **Net conclusion so far: lower concurrency avoids the catastrophic deadlock
case, but no value tried (1, 2, 4) has been shown to actually fix the underlying CPU-bound freeze
under heavy load — it only changes how it fails.** `3` was never tried. Whether there's a genuine
sweet spot, or whether this whole approach is the wrong lever (vs. fixing/bounding the underlying
recursive computation directly, or capping concurrency *and* adding a time/depth bound to whatever
the recursive walk is), is still open.

## Tooling notes from this session (for whoever picks this back up)

- **`emclient-runner.sh`'s freeze watcher (already existed) was extended** with an automatic,
  one-shot context dump at freeze onset AND recovery (`freeze_context_dump` function): load
  average, top CPU consumers, the `claude-ui-thread-tid.txt` value, open windows, and the tail of
  every diagnostic log (`claude-dnsconnect.log`, `claude-diag5.log`, `claude-diag6.log`) — so a
  freeze's context is captured automatically without anyone needing to catch it live. This is a
  real, working, valuable improvement — **but not merged into `project_emclient_runner_script.md`'s
  tracked memory source yet**, only on the remote bottle's own live copy. Should be back-ported to
  that memory file so it isn't lost.
- **Automatic `gdb` attachment at freeze onset was tried and reverted** — `ptrace(PTRACE_ATTACH)`
  suspends *every* thread in the target process, not just the one being inspected, and enumerating
  + backtracing all threads in a large, busy process under a loaded host measurably extended a
  real freeze (one measured at 44.2s against an expected ~30s bound — the extra ~14s lines up with
  gdb's own attach/enumerate/detach overhead). The automatic capture now deliberately does NOT
  include a native-stack dump; use the on-demand `runner-native-stack` marker for a single,
  deliberate capture instead, accepting the intrusion as a one-time cost.
- **`action_native_stack` (the on-demand marker) was fixed to capture BOTH top-level "main-looking"
  processes**, not just the first match — see the two-top-level-process discovery above. Previously
  it silently attached to whichever of `CrBrowserMain`/`MailClient.exe` happened to be found first
  by `find_running_pids`, which could be the wrong one for a given investigation.
- **A new `--patch-ui-thread-tid` diagnostic was built and found not useful** for its intended
  purpose (see the Wine `GetCurrentThreadId()` finding above) — kept deployed since it's harmless,
  but don't rely on `claude-ui-thread-tid.txt` to identify a native thread in `ps -L`/`gdb` output.
- **A `CX_DEBUGMSG` trace channel set was tested specifically for "why does the UI thread lock up"**
  (as opposed to the existing `FREEZE_TRACE_CHANNELS_LIGHT`'s `+mountmgr`, which was carried over
  from an unrelated earlier investigation and isn't obviously relevant to this question). Measured
  live, in order: `+message,+sync,+thread` produced **~100MB / 1.6M lines in the first 10 seconds
  of startup alone** (`+sync` alone was 99%+ of that volume — tracing every synchronization
  primitive operation is far too chatty for continuous use). Dropped to `+message,+thread` alone:
  ~10MB/82K lines in the first 10s (startup burst), then a measured **steady-state rate of
  ~5.2MB / 40K lines per 30 seconds** (~10MB/min, ~600MB/hour) — lighter, but still not obviously
  sustainable for a multi-hour "leave it running and wait for a freeze" session, and it was never
  actually correlated against a real freeze before the investigation moved to the CPU-bound
  finding instead. Added as `runner-trace-uithread-on`/`-off` markers
  (`FREEZE_TRACE_CHANNELS_UITHREAD`) for whoever picks this back up; not proven useful yet, only
  proven "lighter than the alternative that clearly wasn't going to work."
- **`dotnet-dump`/ClrMD were considered but not attempted** for getting a real, symbol-resolved
  managed stack (which would settle the "what is the recursive method" question directly, unlike
  a symbol-less `gdb` capture). Reasoning for not pursuing it: this is a Windows-PE CoreCLR
  process running under Wine, which almost certainly creates its .NET diagnostics IPC port as a
  Windows-style named pipe (emulated entirely within Wine, not exposed as a real Linux Unix-domain
  socket at the conventional `/tmp/dotnet-diagnostic-*` path `dotnet-dump` expects) — a native
  Linux `dotnet-dump` very likely cannot see or connect to it. **Not actually verified on the
  remote host** (no shell access there to check for a `dotnet-diagnostic-*` socket under the
  bottle's own environment) — worth a quick, cheap check before ruling it out for certain, since
  if it *does* work, it would be a far better tool for the CPU-bound investigation than anything
  used this session.
- **A real, separate bug in `runner-panic`**: its `kill -9` failed silently, twice, against the
  same stuck process (confirmed by the process's own `ELAPSED` time in `ps` continuing to climb
  for 10+ hours after two separate "PANIC: force-killing" log lines). The *graceful-close* path's
  own fallback chain (signal → `wmctrl` → plain `kill`, no `-9`) is what eventually worked. Not
  root-caused this session — worth fixing before relying on `runner-panic` again for something
  urgent.

## Updated recommended next steps

1. **Don't re-try the concurrency gate at intermediate values (3, 5, ...) without a better
   hypothesis first** — the data so far (1 catastrophic, 2 bounded-but-freezes, 4 worse) doesn't
   suggest a clean "sweet spot exists, just tune it" story; it may be the wrong lever entirely for
   the CPU-bound mechanism (capping *how many* heavy things run at once doesn't help if any *one*
   of them running is already enough to starve the UI thread on this host).
2. **Identify and fix (or at least bound) the actual recursive/CPU-heavy operation directly.**
   This is the highest-value remaining lead: a symbol-resolved managed stack (via `dotnet-dump` if
   it turns out to work through Wine — check this first, cheaply — or via new, targeted
   instrumentation added directly to the most likely candidates: `MSGraphFolderSynchronizer`'s
   folder-tree walk, or the mail item download/prefetch path "sync all folders for offline use"
   would exercise) would settle what's actually recursing and why, which a fixed depth limit,
   iterative rewrite, or per-item yield/checkpoint could then address directly — a much more
   targeted fix than throttling account concurrency.
3. **Verify the token-refresh retry fix (`TokenRefreshRetryHelper`) under a real scenario**
   (an actual token expiry during heavy load) — deployed but not stress-tested end-to-end this
   session.
4. **Back-port `emclient-runner.sh`'s freeze-context-dump improvements** into
   `project_emclient_runner_script.md`'s tracked memory source before they're lost.
5. **Fix `runner-panic`'s silent `kill -9` failure** before depending on it again.
6. Once the DNS/socket-timeout fixes (`DnsConnectHelper`/`TimeoutBoundedNetworkStream`) have had
   more real-world soak time without regressions, consider promoting them from the scratch
   `il-patcher` fork into the tracked `il-patches/il-patcher-Program.cs` and wiring into a new
   `releases/11.0.196-beta` stage — they're the most solid, best-verified result of this whole
   investigation so far.
