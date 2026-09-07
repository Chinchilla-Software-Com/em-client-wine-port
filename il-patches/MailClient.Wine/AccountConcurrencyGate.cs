// Direct evidence (this session, live): under a heavy "sync all folders for offline use" load,
// one account's dedicated sync worker thread got caught genuinely CPU-bound -- a real, actively
// executing (not blocked) ~70%-CPU, 32-frame-deep recursive-looking managed call stack, sustained
// for well over a minute -- while a second thread (CEF's own) was simultaneously at ~34%. Combined
// with 9 accounts each getting their own dedicated worker thread (confirmed live via a
// per-account-labeled diagnostic -- see reports/emclient11-msgraph-sync-freeze-findings.md), a
// "sync everything" trigger can have every account's worker doing real CPU work at the same
// moment, competing for cores hard enough to starve the UI thread's own scheduling -- a plausible,
// evidence-backed mechanism for "why does a background stall reach the UI thread" that this
// investigation was never able to pin to a specific blocking call (an exhaustive search earlier
// in the session found none for the passive/no-interaction freeze pattern).
//
// This doesn't fix whatever makes that one recursive-looking call path slow in the first place
// (no symbols were obtainable to identify it) -- it caps how many accounts can be doing ANY real
// work at the same instant, app-wide, so the WORST CASE aggregate CPU demand is bounded regardless
// of account count. Deliberately gates Command.Process() itself (see the IL patch's own comment
// for why that's the single shared chokepoint every queue implementation and both the normal and
// "run in place" dispatch paths already funnel through) rather than anything Graph-specific, so it
// protects IMAP/Exchange/etc. accounts the same way.
//
// Limit is read from a plain text file at startup, not hardcoded, specifically to support fast
// iterative testing (start at 1 -- full serialization -- confirm it fixes the freeze, then raise
// by one at a time until instability returns) without a rebuild+redeploy cycle per attempt.
using System.IO;
using System.Threading;

namespace MailClient.Wine;

public static class AccountConcurrencyGate
{
    private const string LimitPath = @"C:\Temp\claude-account-concurrency.txt";
    private const int DefaultLimit = 1;

    private static readonly int Limit = ReadLimit();
    private static readonly SemaphoreSlim Gate = new(Limit, Limit);

    private static int ReadLimit()
    {
        try
        {
            if (File.Exists(LimitPath) && int.TryParse(File.ReadAllText(LimitPath).Trim(), out int n) && n > 0)
            {
                return n;
            }
        }
        catch (Exception)
        {
        }
        return DefaultLimit;
    }

    // Deliberately outside any try/finally at the call site -- if this itself somehow threw,
    // Exit() must never run (nothing was actually acquired). In practice ReadLimit()'s own
    // defensive try/catch means construction can't throw, and SemaphoreSlim.Wait() with no
    // timeout/token essentially can't either.
    public static void Enter()
    {
        long start = Environment.TickCount64;
        Gate.Wait();
        long waitedMs = Environment.TickCount64 - start;
        if (waitedMs > 200)
        {
            Log($"waited {waitedMs}ms for a slot (limit={Limit})");
        }
    }

    public static void Exit() => Gate.Release();

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(@"C:\Temp\claude-concurrency-gate.log",
                $"{DateTime.Now:HH:mm:ss.fff} [tid={Environment.CurrentManagedThreadId}] {message}{Environment.NewLine}");
        }
        catch (Exception)
        {
        }
    }
}
