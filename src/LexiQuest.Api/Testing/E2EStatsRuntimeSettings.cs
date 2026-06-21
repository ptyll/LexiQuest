namespace LexiQuest.Api.Testing;

public sealed class E2EStatsRuntimeSettings
{
    private readonly object _delayLock = new();
    private int _failNextUserStatsRequest;
    private TaskCompletionSource? _nextUserStatsDelay;
    private TaskCompletionSource? _activeUserStatsDelay;

    public void FailNextUserStatsRequest()
    {
        Interlocked.Exchange(ref _failNextUserStatsRequest, 1);
    }

    public bool ConsumeFailNextUserStatsRequest()
    {
        return Interlocked.Exchange(ref _failNextUserStatsRequest, 0) == 1;
    }

    public void DelayNextUserStatsRequest()
    {
        TaskCompletionSource? previousNext;
        TaskCompletionSource? previousActive;
        lock (_delayLock)
        {
            previousNext = _nextUserStatsDelay;
            previousActive = _activeUserStatsDelay;
            _nextUserStatsDelay = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _activeUserStatsDelay = null;
        }

        previousNext?.TrySetResult();
        previousActive?.TrySetResult();
    }

    public Task? ConsumeDelayForUserStatsRequest()
    {
        lock (_delayLock)
        {
            if (_nextUserStatsDelay is null)
            {
                return null;
            }

            _activeUserStatsDelay = _nextUserStatsDelay;
            _nextUserStatsDelay = null;
            return _activeUserStatsDelay.Task;
        }
    }

    public void ReleaseUserStatsRequest()
    {
        TaskCompletionSource? next;
        TaskCompletionSource? active;
        lock (_delayLock)
        {
            next = _nextUserStatsDelay;
            active = _activeUserStatsDelay;
            _nextUserStatsDelay = null;
            _activeUserStatsDelay = null;
        }

        next?.TrySetResult();
        active?.TrySetResult();
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _failNextUserStatsRequest, 0);
        ReleaseUserStatsRequest();
    }
}
