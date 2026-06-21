namespace LexiQuest.Api.Testing;

public sealed class E2EHttpDelayRuntimeSettings
{
    private readonly object _lock = new();
    private readonly Dictionary<string, TaskCompletionSource> _nextDelays = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<TaskCompletionSource> _activeDelays = [];

    public void DelayNextRequest(string path)
    {
        var normalizedPath = NormalizePath(path);
        TaskCompletionSource? previous;

        lock (_lock)
        {
            _nextDelays.TryGetValue(normalizedPath, out previous);
            _nextDelays[normalizedPath] = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        previous?.TrySetResult();
    }

    public Task? ConsumeDelayForPath(PathString requestPath)
    {
        var normalizedPath = NormalizePath(requestPath.Value ?? string.Empty);
        lock (_lock)
        {
            if (!_nextDelays.Remove(normalizedPath, out var delay))
            {
                return null;
            }

            _activeDelays.Add(delay);
            return delay.Task;
        }
    }

    public void ReleaseAll()
    {
        List<TaskCompletionSource> delays;
        lock (_lock)
        {
            delays = [.. _nextDelays.Values, .. _activeDelays];
            _nextDelays.Clear();
            _activeDelays.Clear();
        }

        foreach (var delay in delays)
        {
            delay.TrySetResult();
        }
    }

    public void Reset() => ReleaseAll();

    private static string NormalizePath(string path)
    {
        var trimmed = path.Trim();
        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }
}

public sealed record E2EHttpDelayRequest(string Path);
