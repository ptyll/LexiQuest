namespace LexiQuest.Api.Testing;

public sealed class AdjustableTimeProvider : TimeProvider
{
    private readonly object _lock = new();
    private TimeSpan _offset = TimeSpan.Zero;

    public override DateTimeOffset GetUtcNow()
    {
        lock (_lock)
        {
            return DateTimeOffset.UtcNow.Add(_offset);
        }
    }

    public void Advance(TimeSpan offset)
    {
        lock (_lock)
        {
            _offset = _offset.Add(offset);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _offset = TimeSpan.Zero;
        }
    }
}

public sealed record E2ETimeAdvanceRequest(int Seconds);

public sealed record E2EQuickMatchTimeLimitRequest(int Seconds);

public sealed record E2EExpireRoomRequest(string RoomCode);
