using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing XP gains and level progression.
/// </summary>
public interface IXpService
{
    /// <summary>
    /// Adds XP to a user and handles level ups.
    /// </summary>
    Task<XPGainedEvent> AddXpAsync(Guid userId, int amount, XpSource source, CancellationToken cancellationToken = default);
}

/// <summary>
/// Source of XP gain.
/// </summary>
public enum XpSource
{
    Game,
    DailyChallenge,
    Achievement,
    Streak
}
