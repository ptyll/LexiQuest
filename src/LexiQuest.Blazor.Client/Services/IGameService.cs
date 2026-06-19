using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Service for interacting with game API.
/// </summary>
public interface IGameService
{
    /// <summary>
    /// Starts a new game session.
    /// </summary>
    Task<ScrambledWordDto?> StartGameAsync(StartGameRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Submits an answer for the current round.
    /// </summary>
    Task<GameRoundResult?> SubmitAnswerAsync(Guid sessionId, string answer, int timeSpentMs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current game state.
    /// </summary>
    Task<ScrambledWordDto?> GetGameStateAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Forfeits the current game.
    /// </summary>
    Task<bool> ForfeitGameAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
