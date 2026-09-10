// Wine's GetAddrInfoExW -- the async/cancellable DNS resolution entry point .NET's default
// SocketsHttpHandler connection path calls into -- doesn't properly support cancellation or the
// namespace parameter under this CrossOver/Wine build (confirmed via a +winsock CX_DEBUGMSG
// trace: "fixme:winsock:GetAddrInfoExW Unsupported namespace 0" / "Unsupported cancel handle" --
// see reports/emclient11-msgraph-sync-freeze-findings.md). A DNS lookup that would normally take
// milliseconds can instead hang for tens of seconds with no way to cancel it, regardless of
// whatever timeout/cancellation token the caller supplied -- directly measured live as a single
// Graph API call taking ~46 seconds to return with zero retries, correlating exactly with an
// observed UI freeze.
//
// This callback sidesteps that entry point entirely: it resolves the target host itself via the
// older, synchronous Dns.GetHostAddresses (confirmed via trace to go through Wine's plain
// non-cancellable getaddrinfo rather than the broken async path), caches the result, and connects
// by IP directly, so SocketsHttpHandler's own internal DNS step never runs for a host this cache
// already knows.
//
// Wired in from InteractionController.CreateHttpClient (MailClient.dll) via
// --patch-http-dns-connect-callback, which calls InstallConnectCallback below onto the
// SocketsHttpHandler reached there by reflection -- that method is the single shared HttpClient
// factory every protocol in the app uses (IMAP, Exchange, AirSync, CalDav, WebCal, MSGraph,
// chat/cloud-storage connectors, ...), so this cache and callback are deliberately generic, not
// Graph-specific, even though Microsoft Graph accounts are what surfaced the bug.
//
// The reflection-based property access (not a direct cast) matters: SocketsHttpHandler is only
// reachable via HttpClientHandler's own PRIVATE "Handler" property under .NET's HttpClientHandler
// abstraction -- there is no public API to reach it directly. This exact reflection pattern is
// already used, unconditionally safe, by the app's own shipped authLog-gated code in the same
// method (see CreateHttpClient's PlaintextStreamFilter hookup).
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MailClient.Wine;

// Must be public, not internal -- InstallConnectCallback/ConnectAsync are called from
// MailClient.dll (a different assembly), and a public member on an internal type is still only
// as accessible as the type itself from outside that assembly. Confirmed the hard way in an
// earlier round: this decompiled clean and built without error when internal, but threw
// System.MethodAccessException the instant InteractionController.CreateHttpClient actually called
// it at runtime -- ilspycmd doesn't check cross-assembly accessibility any more than it checks
// handler-region bounds (CLAUDE.md IL-patching lesson 3) or generic self-instantiation
// (lesson 19); this is the same "decompiling clean is not sufficient proof" shape, a new trigger.
public static class DnsConnectHelper
{
    // Called from InteractionController.CreateHttpClient (MailClient.dll) right after
    // httpClientHandler is fully configured, regardless of whether the authLog/loggingWriter path
    // is taken -- the DNS/connect bug affects every protocol and account, not just logged ones.
    // No-ops silently if the private "Handler" property shape ever changes in a future .NET
    // version (same fail-safe posture as the app's own existing authLog reflection block right
    // next to this call site).
    public static void InstallConnectCallback(HttpClientHandler handler)
    {
        PropertyInfo? property = typeof(HttpClientHandler).GetProperty("Handler", BindingFlags.Instance | BindingFlags.NonPublic);
        if (property?.GetValue(handler) is SocketsHttpHandler socketsHttpHandler)
        {
            socketsHttpHandler.ConnectCallback = ConnectAsync;
        }
    }

    public static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        string host = context.DnsEndPoint.Host;
        int port = context.DnsEndPoint.Port;

        IPAddress[] addresses = await DnsCache.ResolveAsync(host).ConfigureAwait(false);

        Exception? lastError = null;
        foreach (IPAddress address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(address, port, cancellationToken).ConfigureAwait(false);

                // Direct evidence (see reports/emclient11-msgraph-sync-freeze-findings.md): with
                // DNS+connect fixed, a Graph command still hung for ~60 real seconds with zero
                // retries -- the hang moved to whatever runs on the stream AFTER connect succeeds
                // (TLS handshake, request send, or response read), all of which go through
                // .NET's normal async Stream.ReadAsync/WriteAsync -- for a plain NetworkStream,
                // that's Socket.ReceiveAsync/SendAsync, the same overlapped-I/O-with-cancellation
                // machinery family as the GetAddrInfoExW entry point already confirmed broken.
                //
                // TimeoutBoundedNetworkStream sidesteps it the same way DnsCache sidesteps the
                // broken async DNS API: never call the async socket path at all. Every
                // Read/WriteAsync call underneath does a genuinely SYNCHRONOUS Socket.Receive/Send
                // (bounded by SO_RCVTIMEO/SO_SNDTIMEO -- decades-old, simple blocking socket
                // options, not part of the overlapped/cancellation-token family this Wine build
                // gets wrong), wrapped in Task.Run purely so the Stream's async signature is still
                // satisfied for SslStream/HttpClient's internals above it.
                return new TimeoutBoundedNetworkStream(socket, host);
            }
            catch (Exception ex)
            {
                lastError = ex;
                socket.Dispose();
            }
        }

        throw new HttpRequestException(
            $"DnsConnectHelper: failed to connect to {host}:{port}",
            lastError);
    }
}

// Wraps a connected Socket as a Stream, but every Read/Write path -- sync AND async -- goes
// through a genuinely blocking Socket.Receive/Send call bounded by SO_RCVTIMEO/SO_SNDTIMEO
// (via Socket.ReceiveTimeout/SendTimeout), never Socket.ReceiveAsync/SendAsync or
// NetworkStream's own async path. See the ConnectAsync call site's comment for why: the same
// class of Wine bug already confirmed for async/cancellable DNS resolution is the leading
// suspect for a still-observed ~60-second hang happening AFTER a successful DNS+connect, which
// can only be in TLS handshake / request send / response read -- all of which run through
// whatever Stream this class returns. A per-operation timeout here can't make the underlying
// Wine bug (if this hypothesis is right) go away, but confining every I/O call to the old,
// simple, synchronous blocking socket API avoids the specific broken code path entirely, the
// same way DnsCache avoids GetAddrInfoExW -- if that hypothesis is wrong, the per-operation
// timeout is still a real, working bound (unlike a CancellationToken-based timeout, which
// depends on the exact mechanism already shown not to work here).
internal sealed class TimeoutBoundedNetworkStream : Stream
{
    private const int TimeoutMs = 30000;

    private readonly Socket socket;

    public TimeoutBoundedNetworkStream(Socket socket, string host)
    {
        this.socket = socket;
        socket.ReceiveTimeout = TimeoutMs;
        socket.SendTimeout = TimeoutMs;
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;
    public override bool CanSeek => false;
    public override bool CanTimeout => true;
    public override int ReadTimeout
    {
        get => socket.ReceiveTimeout;
        set => socket.ReceiveTimeout = value;
    }
    public override int WriteTimeout
    {
        get => socket.SendTimeout;
        set => socket.SendTimeout = value;
    }
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        return socket.Receive(buffer, offset, count, SocketFlags.None);
    }

    public override int Read(Span<byte> buffer)
    {
        byte[] rented = new byte[buffer.Length];
        int n = Read(rented, 0, rented.Length);
        rented.AsSpan(0, n).CopyTo(buffer);
        return n;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        socket.Send(buffer, offset, count, SocketFlags.None);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Write(buffer.ToArray(), 0, buffer.Length);
    }

    // These four overrides are the ones that actually matter -- SslStream and HttpClient's
    // internals call the Memory<byte>-based ReadAsync/WriteAsync almost exclusively in modern
    // .NET, not the byte[]-based ones (those exist here only for completeness/older callers).
    // Task.Run is doing real work here, not just ceremony: it moves the genuinely blocking
    // Socket.Receive/Send call off whatever thread awaited this method and onto a pool thread,
    // so the caller still gets a proper awaitable back -- the timeout bound comes entirely from
    // SO_RCVTIMEO/SO_SNDTIMEO on the socket itself, not from anything cancellation-token-based.
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Task.Run(() => Read(buffer, offset, count), CancellationToken.None);

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return new ValueTask<int>(Task.Run(() =>
        {
            if (MemoryMarshal.TryGetArray((ReadOnlyMemory<byte>)buffer, out ArraySegment<byte> segment))
            {
                return Read(segment.Array!, segment.Offset, segment.Count);
            }
            byte[] rented = new byte[buffer.Length];
            int n = Read(rented, 0, rented.Length);
            rented.AsSpan(0, n).CopyTo(buffer.Span);
            return n;
        }, CancellationToken.None));
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => Task.Run(() => Write(buffer, offset, count), CancellationToken.None);

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return new ValueTask(Task.Run(() =>
        {
            if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> segment))
            {
                Write(segment.Array!, segment.Offset, segment.Count);
            }
            else
            {
                byte[] arr = buffer.ToArray();
                Write(arr, 0, arr.Length);
            }
        }, CancellationToken.None));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            socket.Dispose();
        }
        base.Dispose(disposing);
    }
}

// Thread-safe get-or-resolve-once-and-cache, with a bounded size (defensive -- this callback is
// only ever reached via the shared CreateHttpClient factory, so real cardinality is "one entry
// per distinct server a user's own accounts talk to", not per-email or otherwise unbounded) and
// a short TTL, refreshed lazily on read rather than via a background sweep (the working set is
// small enough that a sweep thread would be pure overhead).
internal static class DnsCache
{
    private sealed class Entry
    {
        public Entry(IPAddress[] addresses, DateTime expiresAtUtc)
        {
            Addresses = addresses;
            ExpiresAtUtc = expiresAtUtc;
        }

        public IPAddress[] Addresses { get; }
        public DateTime ExpiresAtUtc { get; }
    }

    private const int MaxEntries = 500;
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private static readonly ConcurrentDictionary<string, Lazy<Task<Entry>>> cache = new();

    public static async Task<IPAddress[]> ResolveAsync(string host)
    {
        while (true)
        {
            // Lazy<Task<T>> (not a bare Task<T> value in the dictionary) guarantees the
            // resolution factory runs exactly once per miss even under concurrent callers for
            // the same host -- ConcurrentDictionary.GetOrAdd's own factory can otherwise run
            // more than once under a race, which would fire duplicate lookups.
            Lazy<Task<Entry>> lazy = cache.GetOrAdd(host, static h => new Lazy<Task<Entry>>(() => ResolveNowAsync(h)));

            Entry entry;
            try
            {
                entry = await lazy.Value.ConfigureAwait(false);
            }
            catch
            {
                // Don't poison the cache with a failed lookup -- remove exactly this failed
                // attempt (only if nobody already replaced it) so the next caller gets a fresh
                // try instead of the same cached exception.
                cache.TryRemove(new KeyValuePair<string, Lazy<Task<Entry>>>(host, lazy));
                throw;
            }

            if (entry.ExpiresAtUtc > DateTime.UtcNow)
            {
                return entry.Addresses;
            }

            // Expired -- drop this exact stale entry (a no-op if another thread already
            // refreshed it first) and loop to pick up or perform a fresh resolution.
            cache.TryRemove(new KeyValuePair<string, Lazy<Task<Entry>>>(host, lazy));
        }
    }

    private static async Task<Entry> ResolveNowAsync(string host)
    {
        if (cache.Count >= MaxEntries)
        {
            TrimExpired();
        }

        // Task.Run: Dns.GetHostAddresses is the synchronous overload (see file header for why),
        // so it runs on a pool thread rather than blocking whichever caller reached ResolveAsync.
        IPAddress[] addresses = await Task.Run(() => Dns.GetHostAddresses(host)).ConfigureAwait(false);
        return new Entry(addresses, DateTime.UtcNow + Ttl);
    }

    private static void TrimExpired()
    {
        DateTime now = DateTime.UtcNow;
        foreach (KeyValuePair<string, Lazy<Task<Entry>>> kvp in cache)
        {
            if (kvp.Value.IsValueCreated
                && kvp.Value.Value.IsCompletedSuccessfully
                && kvp.Value.Value.Result.ExpiresAtUtc <= now)
            {
                cache.TryRemove(kvp);
            }
        }
    }
}
