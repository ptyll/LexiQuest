using LexiQuest.Shared.DTOs.Game;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for managing user streaks.
/// </summary>
public interface IStreakService
{
    /// <summary>
    /// Checks and updates the user's streak.
    /// </summary>
    Task<StreakStatus> CheckStreakAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the fire level based on streak days.
    /// </summary>
    FireLevel GetFireLevel(int days);
}

/// <summary>
/// Fire level enum for streak visualization.
/// </summary>
public enum FireLevel
{
    Cold,       // 0 days
    Small,      // 1-3 days
    Medium,     // 4-7 days
    Large,      // 8-30 days
    Legendary   // 30+ days
}
