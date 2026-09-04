# eM Client freeze during Exchange sync — investigation findings

**Status: fixed and deployed as release/10.4.5674-3 (Stage 15 in deploy.sh). Confirmed working
live — the severe, recurring, minutes-long freezes are gone; general operation reported as "much
better." One smaller, startup-only entry point (`GetFolderList`) and one separate, unresolved
issue (a native-level crash during the heaviest "download all messages for offline use" folder
walks) remain — see their own sections below.**

## Symptom

On a real user desktop (referred to below as "the host") running eM Client 10.4.5674 under
CrossOver, the app periodically freezes solid — no repaint, no input response — for anywhere
from ~20-30 seconds up to several minutes. Freezes are most common around the once-a-minute
auto-sync cycle, but not on every cycle. The host has 8 real mail accounts configured, all
Microsoft 365 Exchange/EWS (confirmed via `.acn` filenames in
`AppData/Roaming/eM Client/<GUID>/Exchange Web Services *.acn` — zero IMAP/POP3 accounts).

Initially suspected: the host's OS auto-mounting SMB shares (`/media/<user>/*` via systemd
autofs units) whenever something scans all filesystems. This was investigated first and is a
**real, separate, confirmed Wine behavior** (see "Ruled out / separate finding" below) but does
**not** explain the long freezes actually observed.

## Tooling used

A watcher script (`emclient-runner.sh`, not part of this repo — ad hoc for this investigation)
was deployed to the host's bottle, polling `drive_c/Temp` for marker files and running fixed,
non-parsed actions: launch, process snapshot, mount snapshot, screenshot, and two tiers of
`CX_DEBUGMSG` tracing (`light` = `+mountmgr` only, safe to leave running all session; `deep` =
adds `+file,+ntdll,+module,+server,+process,+thread,+sync,+event,+winsock`, used only for short
bursts around one reproduction). A parallel debug build of `il-patcher` re-added (for this
investigation only, not shipped) three previously-removed dev-only trigger flags
(`--patch-close-listener`, `--patch-test-monogram-avatar`, `--patch-theme-switcher`) repointed at
`C:\Temp\` instead of `Z:\tmp\`, so the app could be driven without needing hands-on access to
the host.

Later extended with a fourth debug-only patch, `--patch-ui-heartbeat` (also never shipped): a
`System.Windows.Forms.Timer` writing an incrementing counter to `C:\Temp\claude-heartbeat` every
250ms from the UI thread. Paired with a background watcher loop inside `emclient-runner.sh`
polling that file's *content* (not `mtime` — `stat -c %Y` has no sub-second resolution, which
would make staleness detection ambiguous by up to ~1s at second boundaries) every 200ms — the
moment it stops changing, that's an unambiguous, immediate freeze signal with no human
reaction-time lag, and the watcher automatically starts capturing per-thread CPU snapshots
(`ps -L -o pid,tid,pcpu,stat,comm`, scoped to the bottle's own PIDs) every poll until it recovers.
Removed a genuine `find_bottle_pids` bug along the way: `/proc/[pid]/environ` reads can fail with
`EPERM` due to the kernel's `ptrace_scope` policy for a same-UID process that isn't a
ptrace-permitted descendant, which the file's own nominal permission bits (what `[[ -r ]]` checks)
can't predict — fixed by reordering the redirection so `2>/dev/null` is set up before the `<`
input redirect is attempted, not after (the same class of "redirection failure happens before a
later `2>/dev/null` on the same line takes effect" issue documented for `find_running_pids`
elsewhere in this project's own history, just triggered by a different underlying cause here).

## Ruled out / separate finding: mountmgr's UDisks2 device scan

Wine's `mountmgr.sys` enumerates **every block device on the host via UDisks2** at startup and
again in bursts (confirmed live: the same local devices — `dm-1`/`extra`, `dm-2`/`data-minty`,
`nvme1n1p3`/`win-11-os` — were re-added 4 times in a 4-second window during one app launch, each
via a fresh `udisks2_add_device`/`add_volume` D-Bus round-trip). This is real and worth cleaning
up eventually, but it's a high-CPU (125% observed), *short* (~4 second) event confined to the
load window. The autofs-managed SMB shares under `/media/<user>/*` never even appeared in the
mountmgr trace during this — UDisks2 only tracks real block devices, and autofs entries are
invisible to it until the kernel actually triggers the mount. **This is not the cause of the
long freezes** — confirmed by capturing a live ~30-second freeze where CPU was only 21.9% (i.e.
mostly idle/blocked, not spinning), and mountmgr activity had gone completely silent minutes
before the freeze in question started.

## Confirmed root cause chain

1. **The trigger**: the app's once-a-minute (and manual "Send/Receive") sync path is
   `formMain`'s `Synchronize()` handlers → `AccountManager.SendAndReceiveAll(...)` —
   **every call site in the app uses this synchronous method**, never the async sibling.
2. `SendAndReceiveAll` → `SendAllInternal(...)` then `ReceiveAllInternal(...)`.
3. `ReceiveAllInternal` is a **plain sequential `foreach`** over every configured account,
   calling `mailAccount.Receive()` (synchronous) one account at a time — each account's call
   fully completes (or times out) before the loop moves to the next account. A parallel sibling,
   `ReceiveAllInternalAsync`, exists in the same class and correctly uses
   `Task.WhenAll(...)` — **it is never called by anything in the app.**
4. For an Exchange account, this eventually reaches Microsoft's own EWS Managed API SDK
   (`Microsoft.Exchange.WebServices.Data.dll`, vendored, not eM Client's own code):
   `EwsHttpWebRequest.GetResponse()` contains
   `return GetResponse(httpCompletion, cancellationToken).GetAwaiter().GetResult();`
   — a genuine sync-over-async block. Whatever thread runs the `foreach` above (very likely the
   UI thread, given `Synchronize()`'s call sites in `formMain` are typical UI-thread timer/event
   handlers, and nothing in the traced call chain hops to a background thread) is truly blocked
   here, not just logically waiting.
5. **The Wine gap**: `GetAddrInfoExW` (the async DNS resolution API this HttpClient-based path
   uses) logs `fixme:winsock:GetAddrInfoExW Unsupported cancel handle` on this build — Wine does
   not support canceling an in-flight async DNS lookup. Confirmed live via a captured freeze: on
   thread `018c:0514`,
   ```
   101498.149  GetAddrInfoExW name "outlook.office365.com" ...
   101498.149  fixme: GetAddrInfoExW Unsupported cancel handle
   101498.149  GetQueuedCompletionStatus (..., timeout=20000)
   ─────────────── exactly 20.001s gap on this thread, nothing ───────────────
   101518.150  (thread resumes)
   ```
   A paired thread (`018c:07b8`) went silent and resumed at the *identical millisecond*,
   consistent with one being the blocked caller and the other the I/O-completion worker for the
   same stuck socket. Meanwhile several unrelated threads (heavy local-file I/O — thousands of
   trace lines each) ran continuously through the same window, confirming this is **not** a
   global process stall — it's specific to whatever's waiting on this one connection.

Net effect: whichever account's connection attempt happens to hit real network/DNS slowness that
cycle blocks the *entire subsequent* sequential loop for up to 20 seconds per affected account,
with the app frozen (assuming this runs on the UI thread, not yet 100% confirmed — see below) for
the whole duration. Multiple affected accounts in the same cycle compound linearly — a captured
~2m48s freeze is consistent with several accounts each hitting the timeout in sequence.

## UI-thread blocking — confirmed directly

Built a `+message`-channel deep trace plus a lightweight in-app heartbeat (a debug-only
`System.Windows.Forms.Timer` writing an incrementing counter to a file every 250ms from the UI
thread, `--patch-ui-heartbeat`, never shipped) so an external watcher script could detect a
freeze automatically (comparing the counter between 200ms polls) instead of relying on a human
noticing and reporting it in time. This caught a live freeze with a clean, direct signature: the
thread with by far the most `+message`-channel activity (confirmed as the UI thread) showed
`NtWaitForSingleObject(handle, timeout=infinite)` — a genuine blocking wait, not a normal
message-pump idle — for exactly 2.698 seconds, with multiple unrelated threads resuming at the
identical instant once the wait handle was finally signaled. Ruled out network I/O, file I/O, and
GC/`VirtualAlloc` activity as the direct cause of that specific window (all channels silent);
whatever was actually being computed was pure managed CPU work invisible to Wine-level tracing.

## Fix implemented and deployed (release/10.4.5674-3, Stage 15)

Two separate, independently-guarded entry points needed the same treatment — patching only one
left the other still freezing the app:

- **`AccountManager.SendAndReceiveAll`** (`MailClient.Accounts.dll`) — the account-level sync
  entry point. Originally thought to be reachable only via the once-a-minute timer
  (`DesktopAccountManager.timerSendAndReceive_Tick`, a plain `System.Windows.Forms.Timer` whose
  `Tick` always fires on the UI thread); live testing found it's *also* called directly and
  synchronously by eM Client's own "check for mail on startup" option and by
  `GoOnlineInt()`/manual refresh/menu items — patching only the timer's own call site (a first,
  narrower draft) left the app still freezing immediately on launch. Fixed by moving the
  guard+dispatch down into `SendAndReceiveAll` itself instead of any one caller, so every call
  site is covered uniformly: a new `volatile bool __syncInProgress` field, checked and set on
  whichever thread calls in, then the original method body (moved verbatim into a new
  `__RunSendAndReceiveAllCore`, to avoid hand-authoring its two null-conditional delegate/event
  invokes) runs on a dedicated background thread.
- **`Folder.Synchronize(bool, bool)`** (`MailClient.Accounts.dll`) — a second, independent
  synchronous entry point, called directly by the manual-refresh path
  (`formMain.sendReceiveAll()` calls `SelectedFolder?.Synchronize(forced: true, fromUI: true)`
  *before* it ever reaches `SendAndReceiveAll`) and very likely by the "download messages for
  offline use" recursive folder-tree walk too (its own `OfflineSynchronizationScope`/`Mode`
  settings only gate how much gets downloaded per folder, not a separate trigger path). Its
  private recursive overload (`Synchronize(SynchronizationPriority, bool)`, which does the actual
  subfolder walk) is untouched — the public entry point alone is guarded+dispatched the same way,
  so the entire recursive walk runs on one background thread regardless of depth.

**A real regression was caught and fixed during this work**: the first working version of both
patches used `Task.Run` to dispatch onto the background thread. Live testing then showed a *new*
stutter pattern (freeze, ~1s unfreeze, freeze again) once multiple accounts' and folders'
`Task.Run` calls could be in flight together — `Task.Run` schedules onto the shared .NET
ThreadPool, and each dispatched sync could still individually block a pooled worker for up to 20s
on the same Wine DNS issue; the ThreadPool's own slow ramp-up under sustained demand (roughly one
new thread per 500ms-1s) produced exactly that stutter shape. Fixed by switching both patches to
a dedicated `System.Threading.Thread` per dispatch instead — matching the app's own existing
pattern for this exact situation (`MailClient.Commands.DefaultSynchronizationQueue` already uses
one raw background `Thread` per account, never the ThreadPool, for the same reason).

**Verified working live**: after deploying both fixes, manual refresh stopped freezing
noticeably; the once-a-minute auto-sync's freezes shrank from minutes-long to a few seconds and
became much rarer; after the `Task.Run`→`Thread` fix, reported as "behaving much better" with "no
freeze" over an extended session of normal use (composing/replying, navigating folders).

## Why the dev VM never shows this (resolved)

Raised during review: the host and the VM used for general development/testing in this repo are
configured with **the same Microsoft 365 accounts** (VM has a few additional ones) — not
different/test accounts as initially assumed. The VM also has substantially less CPU and memory
than the host, yet never exhibits this freeze while the host does, with as few as 3-4 accounts.
The mechanism above doesn't depend on CPU/RAM at all (it's serialization + real network latency,
not resource contention), so resourcing differences don't explain the gap, and the VM is a guest
*on* the host, sharing its network egress — so "different real-world network path" in a general
sense is a weak explanation too.

Two candidates were checked directly (`ip -6 addr`/`route`, `resolv.conf`/`resolvectl status` on
both machines):

- **IPv6/IPv4 dual-stack "happy eyeballs" behavior — ruled out.** Neither machine has a global
  IPv6 address or an IPv6 default route; both are IPv4-only. No difference here.
- **DNS resolver path — the actual answer.** The VM resolves DNS via a plain local
  `systemd-resolved` stub with no search domain, a direct, fast, uncomplicated path. The host
  resolves DNS through a small set of **private (RFC1918) DNS servers** with a **corporate search
  domain attached**, unambiguous evidence of an active VPN client relaying all of the host's DNS
  traffic — including completely unrelated lookups like the Exchange endpoint — through remote
  corporate DNS infrastructure. That relay path is a plausible, everyday source of the occasional
  slow/hung DNS resolution that this whole bug chain requires in the first place.

This fully resolves the discrepancy without needing account-count, protocol-mix, or
CPU/IPv6-based explanations: the *same* Wine `GetAddrInfoExW` cancel-handle gap and the *same*
sequential-sync-loop design exist in both environments, but the bug only becomes visible when a
DNS lookup is actually slow enough to need canceling — and only the host's DNS path (relayed
through a VPN) realistically produces that. The VM's simple, direct resolver essentially never
does, regardless of CPU/RAM or how many accounts are configured.

This also means the fix that shipped (move the blocking call off the UI thread, not parallelize
the per-account loop itself) was the right target — the VPN/DNS-latency trigger is an
environmental *condition*, not something to patch around; the actual bug worth fixing is that one
slow account (for any reason — VPN-relayed DNS, a genuinely offline server, a transient network
blip) could freeze the whole app instead of just delaying its own sync in the background, where
it belongs.

## Smaller, startup-only entry point found but not patched: `GetFolderList`

Traced a third, independent call chain: `MailClient.Protocols.Common.FolderSynchronizer.
RunEnqueueGetFolderList` — the "ask the server what folders exist at all" step, which has to run
once per account before any individual folder can be synced, visible in the Operations monitor as
"Synchronizing folder list". Its own `IFolderSynchronizer.SynchronizeSubfolders` entry point and
the `FolderSynchronizerExtensions.SynchronizeSubfoldersAsync` wrapper around it are both already
genuinely async (a real `TaskCompletionSource`, no blocking bridge) — but `ExchangeAccount.
GoOnlineInt()` calls `SynchronizableMailAccount.Synchronize(SynchronizationPriority)` directly at
account connect/startup time (when `AutoSynchronize` is on), and that method's own synchronous
prefix (before `SynchronizeAsync`'s internal `Task.Run` even starts) does a modest amount of real
work per account. With several accounts connecting around the same time at launch, this produced
a real but much smaller (2-3 second, one-time per app launch) freeze, unrelated to the two fixed
entry points above. Not patched — traded off against Stage 15's own two fixes given how much
smaller and rarer it is (once per launch vs. every sync cycle); would use the same
guard+dedicated-Thread pattern if pursued.

## Separate, unresolved issue: a real crash during heavy offline-download folder walks

While testing the Stage 15 fix, the app crashed outright on two separate occasions — once on the
original host (after ~7 minutes of apparent "freezing" that later evidence showed was actually
the main process having silently terminated within the first few seconds of that window, based on
it vanishing entirely from repeated `ps` snapshots while Wine's own helper/service processes kept
running normally around it), and once locally in this project's own dev VM (attributed there to
the VM's own memory having been reduced externally at the time, a separate, understood cause).

Neither crash produced eM Client's own `bug.<timestamp>.txt` crash report (see this project's
"Investigation method" #1) — consistent with a *native*-level fault (something failing inside
Wine or native code) rather than a normal .NET managed exception, since only the latter goes
through eM Client's own crash-report writer. In the host's case, the crash coincided with a large
burst of SQLite WAL-journal writes (`folders.dat-wal`, `indexed_attachment_index.dat-wal`) across
dozens of separate account/folder subdirectories simultaneously — strongly consistent with the
"download all messages for offline use" recursive folder walk actively writing to many folders'
local databases at once, right up to whenever the crash happened.

Not yet investigated further. The `runner-freeze-watch.log` automatic capture (see "Tooling used"
below) was extended, once this was found, to distinguish "still frozen" from "process actually
exited" (checking `find_running_pids` before each snapshot) — previously it couldn't tell the two
apart and kept capturing pointless empty snapshots of leftover Wine processes for minutes after
the app had already died. If this recurs, the log will now say so immediately rather than needing
the same after-the-fact reconstruction (checking when the main PID last appears in the snapshots)
this write-up required.
