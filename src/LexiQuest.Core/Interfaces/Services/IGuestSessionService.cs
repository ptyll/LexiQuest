using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing guest game sessions without user registration.
/// All data is stored in-memory only and lost after session ends.
/// </summary>
public interface IGuestSessionService
{
    /// <summary>
    /// Starts a new guest game session with 5 beginner words.
    /// </summary>
    GuestSessionResult StartGame();

    /// <summary>
    /// Submits an answer for a word in the guest session.
    /// </summary>
    GuestAnswerResult SubmitAnswer(Guid sessionId, Guid wordId, string answer);

    /// <summary>
    /// Gets current progress of the guest session.
    /// </summary>
    GuestSessionProgress GetSessionProgress(Guid sessionId);

    /// <summary>
    /// Ends the guest session and returns final stats.
    /// </summary>
    GuestSessionResult EndGame(Guid sessionId);
}
