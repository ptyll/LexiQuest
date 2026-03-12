using LexiQuest.Shared.DTOs.Multiplayer;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing multiplayer game sessions.
/// </summary>
public interface IMultiplayerGameService
{
    /// <summary>
    /// Creates a new multiplayer match.
    /// </summary>
    Task<Guid> CreateMatchAsync(Guid player1Id, Guid player2Id, bool isPrivateRoom = false, RoomSettingsDto? settings = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Starts the match and returns the first round.
    /// </summary>
    Task<MultiplayerRoundDto> StartMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Submits an answer for a player.
    /// </summary>
    Task<(bool IsCorrect, int Score, bool IsMatchComplete)> SubmitAnswerAsync(Guid matchId, Guid playerId, string answer, int timeSpentMs, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Forfeits the match for a player.
    /// </summary>
    Task ForfeitAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current match state.
    /// </summary>
    Task<MatchStateDto?> GetMatchStateAsync(Guid matchId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Ends the match and calculates results.
    /// </summary>
    Task<MatchResultDto> EndMatchAsync(Guid matchId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the opponent's progress.
    /// </summary>
    Task<OpponentProgressDto> GetOpponentProgressAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if a match exists and is active.
    /// </summary>
    Task<bool> IsMatchActiveAsync(Guid matchId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Handles player disconnect with grace period.
    /// </summary>
    Task HandleDisconnectAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Handles player reconnect.
    /// </summary>
    Task<bool> HandleReconnectAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalizes disconnect after grace period - forfeits the match for the disconnected player.
    /// </summary>
    Task FinalizeDisconnectAsync(Guid matchId, Guid playerId, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for match state.
/// </summary>
public record MatchStateDto(
    Guid MatchId,
    Guid Player1Id,
    Guid Player2Id,
    int CurrentRound,
    int TotalRounds,
    int Player1Score,
    int Player2Score,
    TimeSpan TimeRemaining,
    bool IsActive,
    DateTime StartedAt
);
