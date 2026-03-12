using LexiQuest.Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;

namespace LexiQuest.Core.Services;

/// <summary>
/// Service for limiting guest game usage.
/// Tracks games per IP address with 24h reset window.
/// Uses IMemoryCache for storing game counts.
/// </summary>
public class GuestLimiter : IGuestLimiter
{
    private readonly IMemoryCache _cache;
    private const int MaxGamesPerDay = 5;
    private static readonly TimeSpan ResetPeriod = TimeSpan.FromHours(24);

    public GuestLimiter(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Checks if a guest can start a new game.
    /// Resets counter if 24h have passed since last game.
    /// </summary>
    public GuestLimitResult CanStartGame(string ipAddress)
    {
        var countKey = $"guest_games_count_{ipAddress}";
        var lastKey = $"guest_games_last_{ipAddress}";

        // Check if there's an existing count
        if (_cache.TryGetValue(countKey, out int gameCount))
        {
            // Check when the first game was played
            if (_cache.TryGetValue(lastKey, out DateTime lastGameTime))
            {
                // If 24h have passed, reset the counter
                if (DateTime.UtcNow - lastGameTime >= ResetPeriod)
                {
                    gameCount = 0;
                }
            }

            // Check if limit reached
            if (gameCount >= MaxGamesPerDay)
            {
                // Calculate reset time - 24h from last game
                var lastGameTimeValue = _cache.TryGetValue(lastKey, out DateTime lastGame)
                    ? lastGame
                    : DateTime.UtcNow;
                var resetTime = lastGameTimeValue.Add(ResetPeriod);
                
                return new GuestLimitResult
                {
                    Allowed = false,
                    RemainingGames = 0,
                    ResetTime = resetTime,
                    Message = $"Denní limit {MaxGamesPerDay} her dosažen. Zaregistruj se pro neomezený přístup."
                };
            }

            // Allow game, return remaining count after this game
            return new GuestLimitResult
            {
                Allowed = true,
                RemainingGames = MaxGamesPerDay - gameCount - 1,
                Message = null
            };
        }

        // No games played yet today
        return new GuestLimitResult
        {
            Allowed = true,
            RemainingGames = MaxGamesPerDay - 1,
            Message = null
        };
    }

    /// <summary>
    /// Records a game start for the IP address.
    /// </summary>
    public void RecordGame(string ipAddress)
    {
        var countKey = $"guest_games_count_{ipAddress}";
        var lastKey = $"guest_games_last_{ipAddress}";

        // Get current count
        int currentCount = 0;
        if (_cache.TryGetValue(countKey, out int existingCount))
        {
            // Check if we need to reset based on time
            if (_cache.TryGetValue(lastKey, out DateTime lastGameTime))
            {
                if (DateTime.UtcNow - lastGameTime < ResetPeriod)
                {
                    currentCount = existingCount;
                }
            }
        }

        // Increment and store
        currentCount++;
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(ResetPeriod);

        _cache.Set(countKey, currentCount, options);
        _cache.Set(lastKey, DateTime.UtcNow, options);
    }

    /// <summary>
    /// Gets current limit status for the IP address.
    /// </summary>
    public GuestLimitStatus GetStatus(string ipAddress)
    {
        var countKey = $"guest_games_count_{ipAddress}";
        var lastKey = $"guest_games_last_{ipAddress}";

        int usedGames = 0;
        DateTime? resetTime = null;

        if (_cache.TryGetValue(countKey, out int gameCount))
        {
            if (_cache.TryGetValue(lastKey, out DateTime lastGameTime))
            {
                // Check if we need to reset
                if (DateTime.UtcNow - lastGameTime >= ResetPeriod)
                {
                    usedGames = 0;
                }
                else
                {
                    usedGames = gameCount;
                    resetTime = lastGameTime.Add(ResetPeriod);
                }
            }
            else
            {
                usedGames = gameCount;
            }
        }

        var remaining = Math.Max(0, MaxGamesPerDay - usedGames);

        return new GuestLimitStatus
        {
            TotalAllowed = MaxGamesPerDay,
            Used = usedGames,
            Remaining = remaining,
            ResetTime = resetTime
        };
    }
}
