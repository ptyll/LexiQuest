namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing matchmaking queue and creating matches.
/// </summary>
public interface IMatchmakingService
{
    /// <summary>
    /// Adds a player to the matchmaking queue.
    /// </summary>
    Task<bool> JoinQueueAsync(Guid userId, int level, string username, string? avatar, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a player from the matchmaking queue.
    /// </summary>
    Task<bool> CancelQueueAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a player is in the queue.
    /// </summary>
    Task<bool> IsInQueueAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the number of players currently in queue.
    /// </summary>
    Task<int> GetQueueCountAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Event raised when a match is found.
    /// </summary>
    event EventHandler<MatchFoundEventArgs>? OnMatchFound;
    
    /// <summary>
    /// Event raised when matchmaking times out.
    /// </summary>
    event EventHandler<MatchmakingTimeoutEventArgs>? OnMatchmakingTimeout;
}

/// <summary>
/// Event args for match found event.
/// </summary>
public class MatchFoundEventArgs : EventArgs
{
    public Guid MatchId { get; set; }
    public Guid Player1Id { get; set; }
    public Guid Player2Id { get; set; }
    public string Player1Username { get; set; } = string.Empty;
    public string Player2Username { get; set; } = string.Empty;
    public int Player1Level { get; set; }
    public int Player2Level { get; set; }
    public string? Player1Avatar { get; set; }
    public string? Player2Avatar { get; set; }
}

/// <summary>
/// Event args for matchmaking timeout event.
/// </summary>
public class MatchmakingTimeoutEventArgs : EventArgs
{
    public Guid UserId { get; set; }
}
