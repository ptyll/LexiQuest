using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Service for guest game operations.
/// </summary>
public interface IGuestGameService
{
    /// <summary>
    /// Starts a new guest game session.
    /// </summary>
    Task<GuestStartResponse?> StartGameAsync();

    /// <summary>
    /// Gets an existing guest session.
    /// </summary>
    Task<GuestStartResponse?> GetSessionAsync(Guid sessionId);

    /// <summary>
    /// Submits an answer for a word.
    /// </summary>
    Task<GuestAnswerResponse?> SubmitAnswerAsync(Guid sessionId, Guid wordId, string answer);

    /// <summary>
    /// Converts a completed guest session into transferable progress.
    /// </summary>
    Task<GuestConvertResponse?> ConvertAsync(Guid sessionId, bool transferProgress);
}
