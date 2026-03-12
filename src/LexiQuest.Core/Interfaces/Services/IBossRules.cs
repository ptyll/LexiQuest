using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Interface for boss level game rules.
/// </summary>
public interface IBossRules
{
    /// <summary>
    /// Initializes a new boss game session.
    /// </summary>
    Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Processes a wrong answer according to boss-specific rules.
    /// </summary>
    Task ProcessWrongAnswerAsync(GameSession session);
    
    /// <summary>
    /// Calculates completion bonus based on boss rules.
    /// </summary>
    int CalculateCompletionBonus(GameSession session, bool perfectRun);
    
    /// <summary>
    /// Calculates speed bonus based on completion time.
    /// </summary>
    int CalculateSpeedBonus(TimeSpan duration);
}
