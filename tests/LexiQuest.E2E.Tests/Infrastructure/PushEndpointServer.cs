using System.Collections.Concurrent;
using System.Net;

namespace LexiQuest.E2E.Tests.Infrastructure;

internal sealed class PushEndpointServer : IAsyncDisposable
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _stop = new();
    private readonly ConcurrentQueue<PushEndpointRequest> _requests = new();
    private readonly Task _listenTask;

    private PushEndpointServer(int port)
    {
        Url = $"http://127.0.0.1:{port}/push/{Guid.NewGuid():N}";
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        _listener.Start();
        _listenTask = Task.Run(ListenAsync);
    }

    public string Url { get; }

    public int RequestCount => _requests.Count;

    public IReadOnlyList<PushEndpointRequest> Requests => _requests.ToArray();

    public static PushEndpointServer Start() => new(TestPort.GetFreeTcpPort());

    public async Task<bool> WaitForRequestCountAsync(int expectedCount, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (RequestCount >= expectedCount)
            {
                return true;
            }

            await Task.Delay(100);
        }

        return RequestCount >= expectedCount;
    }

    public async ValueTask DisposeAsync()
    {
        _stop.Cancel();
        _listener.Stop();
        _listener.Close();

        try
        {
            await _listenTask.WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Listener shutdown can surface ObjectDisposedException/HttpListenerException.
        }

        _stop.Dispose();
    }

    private async Task ListenAsync()
    {
        while (!_stop.IsCancellationRequested)
        {
            HttpListenerContext context;
            try
            {
                context = await _listener.GetContextAsync().WaitAsync(_stop.Token);
            }
            catch
            {
                if (_stop.IsCancellationRequested)
                {
                    return;
                }

                continue;
            }

            await HandleAsync(context);
        }
    }

    private async Task HandleAsync(HttpListenerContext context)
    {
        using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
        var body = await reader.ReadToEndAsync();

        _requests.Enqueue(new PushEndpointRequest(
            context.Request.HttpMethod,
            context.Request.RawUrl ?? string.Empty,
            body));

        context.Response.StatusCode = 201;
        context.Response.ContentType = "application/json";
        await using var writer = new StreamWriter(context.Response.OutputStream);
        await writer.WriteAsync("{}");
        context.Response.Close();
    }
}

internal sealed record PushEndpointRequest(string Method, string RawUrl, string Body);
