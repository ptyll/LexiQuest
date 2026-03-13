using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.ValueObjects;

public class UserPreferences
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

    private UserPreferences() { }

    public static UserPreferences CreateDefault()
    {
        return new UserPreferences
        {
            Theme = AppTheme.Light,
            Language = "cs",
            AnimationsEnabled = true,
            SoundsEnabled = true,
            PushNotificationsEnabled = true,
            EmailNotificationsEnabled = true,
            LeagueUpdatesEnabled = true,
            AchievementNotificationsEnabled = true,
            DailyChallengeReminderEnabled = true
        };
    }
}
