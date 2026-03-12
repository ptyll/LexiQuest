namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Result of checking if a guest can start a new game.
/// </summary>
public class GuestLimitResult
{
    public bool Allowed { get; set; }
    public int RemainingGames { get; set; }
    public DateTime? ResetTime { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Status of guest game usage.
/// </summary>
public class GuestLimitStatus
{
    public int TotalAllowed { get; set; }
    public int Used { get; set; }
    public int Remaining { get; set; }
    public DateTime? ResetTime { get; set; }
}

/// <summary>
/// Service for limiting guest game usage.
/// Tracks games per IP address with 24h reset window.
/// </summary>
public interface IGuestLimiter
{
    /// <summary>
    /// Checks if a guest can start a new game.
    /// </summary>
    GuestLimitResult CanStartGame(string ipAddress);

    /// <summary>
    /// Records a game start for the IP address.
    /// </summary>
    void RecordGame(string ipAddress);

    /// <summary>
    /// Gets current limit status for the IP address.
    /// </summary>
    GuestLimitStatus GetStatus(string ipAddress);
}
