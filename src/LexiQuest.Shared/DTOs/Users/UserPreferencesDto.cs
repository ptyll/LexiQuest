using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Users;

/// <summary>
/// User preferences for display and behavior.
/// </summary>
public class UserPreferencesDto
{
    public AppTheme Theme { get; set; } = AppTheme.Light;
    public string Language { get; set; } = "cs";
    public bool AnimationsEnabled { get; set; } = true;
    public bool SoundsEnabled { get; set; } = true;
    public TimeSpan? StreakReminderTime { get; set; }
    public bool PushNotificationsEnabled { get; set; } = true;
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool LeagueUpdatesEnabled { get; set; } = true;
    public bool AchievementNotificationsEnabled { get; set; } = true;
    public bool DailyChallengeReminderEnabled { get; set; } = true;
}
