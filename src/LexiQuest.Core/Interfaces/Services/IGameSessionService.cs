using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing game sessions.
/// </summary>
public interface IGameSessionService
{
    /// <summary>
    /// Starts a new game session and returns the first scrambled word.
    /// </summary>
    Task<ScrambledWordDto> StartGameAsync(Guid userId, StartGameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits an answer for the current round.
    /// </summary>
    Task<GameRoundResult> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current state of a game session.
    /// </summary>
    Task<ScrambledWordDto?> GetSessionStateAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forfeits the current game session.
    /// </summary>
    Task<bool> ForfeitGameAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
}
