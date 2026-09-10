// MailClient.Accounts.Credentials.GetAccessTokenRefreshResponse (the HTTP call that refreshes an
// OAuth2 access token against login.microsoftonline.com) has NO retry for a network failure --
// confirmed via decompile: its only catch clauses are for UntrustedCertificateException (its own
// separate retry-on-user-trust-decision loop) and a generic `catch (Exception) { log; throw; }`
// for everything else, including a timeout. Directly observed live: this exact call hit the
// DnsConnectHelper/TimeoutBoundedNetworkStream 30-second socket timeout (SocketException,
// WSAETIMEDOUT) and failed hard, immediately, with zero retry -- one of the few remaining
// confirmed failure sources after the DNS and socket-I/O fixes (see
// reports/emclient11-msgraph-sync-freeze-findings.md). This is a small, narrowly-scoped retry
// wrapper for exactly that gap, NOT a general-purpose retry utility -- deliberately specific to
// this one call shape.
//
// Wired in via --patch-token-refresh-retry: the original private
// GetAccessTokenRefreshResponse(IDictionary<string,string>, CancellationToken) method's existing
// compiled body (a real async state machine) is moved onto a new
// __GetAccessTokenRefreshResponseCore method, unchanged, and the original method name gets a
// brand new, one-line body that calls WithRetryAsync below instead -- same "move the body, give
// the original name a new small wrapper" technique already used for the classic engine's
// AccountManager.SendAndReceiveAll / Folder.Synchronize sync-freeze fix (CLAUDE.md IL-patching
// lesson 13), just applied to an async method instead of a synchronous one. An async method's
// entire COMPILED body is only the small state-machine-kickoff boilerplate (construct the state
// machine struct, call MoveNext once, return the task) -- the real await-based logic lives in the
// compiler-generated nested MoveNext(), untouched by moving the outer method -- so this needs no
// special async handling beyond what lesson 13 already covers.
//
// WithRetryAsync deliberately takes the CALLER INSTANCE and re-invokes the moved core method via
// cached reflection (MethodInfo.Invoke) rather than a strongly-typed delegate -- this sidesteps
// hand-constructing a Func<...> delegate object in raw Cecil-authored IL (impractical: the
// generic delegate type needs importing, plus a ldftn/newobj pair against a private instance
// method whose accessibility across the call site would itself need checking) in exchange for a
// trivial 4-instruction wrapper body (ldarg.0, ldarg.1, ldarg.2, call). Reflection Invoke on an
// async method returns its Task immediately without waiting for it to complete, and does NOT wrap
// exceptions raised after the method's first await in a TargetInvocationException (those surface
// through the returned Task faulting normally) -- so the retry catch around `await task` below
// sees the real exception type, not a reflection wrapper, for every case that actually matters
// here.
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;

namespace MailClient.Wine;

public static class TokenRefreshRetryHelper
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(2);

    // Resolved once, lazily, against whichever MailClient.Accounts.dll is actually loaded at
    // runtime -- typeof(MailClient.Accounts.Credentials) binds to the real loaded assembly, not
    // whatever copy this project happened to compile against (see this project's own reference to
    // original/em-11.0.196/MailClient.Accounts.dll: only the type/method NAME needs to match,
    // confirmed identical in every eM Client 11 build tested so far).
    private static readonly Lazy<MethodInfo> coreMethod = new(() =>
        typeof(global::MailClient.Accounts.Credentials).GetMethod(
            "__GetAccessTokenRefreshResponseCore",
            BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingMethodException("MailClient.Accounts.Credentials", "__GetAccessTokenRefreshResponseCore"));

    public static async Task<HttpResponseMessage?> WithRetryAsync(
        object instance,
        IDictionary<string, string> parameters,
        CancellationToken cancellationToken)
    {
        int attempt = 0;
        while (true)
        {
            try
            {
                // Invoke() itself only ever throws synchronously for a failure BEFORE the core
                // method's first await (wrapped in TargetInvocationException, unwrapped below) --
                // everything this wrapper actually cares about (a network failure deep inside the
                // HTTP call) surfaces through the returned Task faulting instead, at the `await`
                // just below, with the real exception type intact (reflection does not wrap
                // exceptions that occur after an async method's first await).
                var task = (Task<HttpResponseMessage?>)coreMethod.Value.Invoke(instance, new object[] { parameters, cancellationToken })!;
                return await task.ConfigureAwait(false);
            }
            catch (TargetInvocationException tie) when (attempt < MaxRetries && tie.InnerException is not null && IsRetryable(tie.InnerException))
            {
                attempt++;
                await Task.Delay(RetryDelay, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (attempt < MaxRetries && IsRetryable(ex))
            {
                attempt++;
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
}
