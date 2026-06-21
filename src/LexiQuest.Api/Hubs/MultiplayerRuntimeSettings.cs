using Microsoft.Extensions.Configuration;

namespace LexiQuest.Api.Hubs;

public sealed class MultiplayerRuntimeSettings
{
    private readonly int _defaultQuickMatchTimeLimitSeconds;
    private int _quickMatchTimeLimitSeconds;

    public MultiplayerRuntimeSettings(IConfiguration configuration)
    {
        _defaultQuickMatchTimeLimitSeconds = Math.Clamp(
            configuration.GetValue("Multiplayer:QuickMatchTimeLimitSeconds", 180),
            1,
            600);
        _quickMatchTimeLimitSeconds = _defaultQuickMatchTimeLimitSeconds;
    }

    public int QuickMatchTimeLimitSeconds => Volatile.Read(ref _quickMatchTimeLimitSeconds);

    public void SetQuickMatchTimeLimitSeconds(int seconds)
    {
        Volatile.Write(ref _quickMatchTimeLimitSeconds, Math.Clamp(seconds, 1, 600));
    }

    public void Reset()
    {
        Volatile.Write(ref _quickMatchTimeLimitSeconds, _defaultQuickMatchTimeLimitSeconds);
    }
}
