// MailClient.Accounts.Credentials.GetAccessTokenRefreshResponse (the HTTP call that refreshes an
// OAuth2 access token against login.microsoftonline.com) has NO retry for a network failure --
// confirmed via decompile: its only catch clauses are for UntrustedCertificateException (its own
// separate retry-on-user-trust-decision loop) and a generic `catch (Exception) { log; throw; }`
// for everything else, including a timeout. Directly observed live: this exact call hit our new
// DnsConnectHelper/TimeoutBoundedNetworkStream 30-second socket timeout (SocketException,
// WSAETIMEDOUT) and failed hard, immediately, with zero retry -- one of the few remaining
// confirmed freeze/failure sources after the DNS and socket-I/O fixes (see
// reports/emclient11-msgraph-sync-freeze-findings.md). This is a small, narrowly-scoped retry
// wrapper for exactly that gap, NOT a general-purpose retry utility -- deliberately specific to
// this one call shape rather than a generic Func<T> helper, to keep the call site's IL patch
// (which has to build the delegate by hand) as simple as possible.
//
// Wired in via IL patch: the original GetAccessTokenRefreshResponse method's existing compiled
// body (a real async state machine, with everything documented above already inside it) is moved
// onto a new __GetAccessTokenRefreshResponseCore method unchanged, and the original method name
// gets a brand new, simple body that calls WithRetryAsync instead -- same "move the body, give
// the original name a new small wrapper" technique already used for the classic engine's
// AccountManager.SendAndReceiveAll / Folder.Synchronize sync-freeze fix (CLAUDE.md IL-patching
// lesson 13), just applied to an async method instead of a synchronous one (which is why the
// retry LOGIC itself lives here as ordinary compiled C# rather than hand-authored IL -- writing a
// real async state machine by hand in Cecil is impractical; this way the C# compiler generates it
// as normal).
using System.IO;
using System.Net.Http;
using System.Net.Sockets;

namespace MailClient.Wine;

public static class TokenRefreshRetryHelper
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    public static async Task<HttpResponseMessage?> WithRetryAsync(
        Func<IDictionary<string, string>, CancellationToken, Task<HttpResponseMessage?>> action,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                return await action(parameters, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < MaxRetries && IsRetryable(ex))
            {
                attempt++;
                Log($"GetAccessTokenRefreshResponse retry {attempt}/{MaxRetries} after {ex.GetType().Name}: {ex.Message}");
                await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    // Network-level transient failures only. Deliberately NOT UntrustedCertificateException (the
    // wrapped method already has its own dedicated handling for that) and NOT anything implying
    // the server actively rejected the request (a real 401/invalid-grant should surface
    // immediately, not be masked behind three delayed retries).
    private static bool IsRetryable(Exception ex)
    {
        return ex is SocketException
            || ex is IOException
            || ex is HttpRequestException
            || ex is TaskCanceledException;
    }

    private static void Log(string message)
    {
        try
        {
            File.AppendAllText(@"C:\Temp\claude-tokenrefresh.log",
                $"{DateTime.Now:HH:mm:ss.fff} {message}{Environment.NewLine}");
        }
        catch (Exception)
        {
        }
    }
}
