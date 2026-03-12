namespace LexiQuest.Shared.Enums;

public enum NotificationType
{
    StreakWarning = 0,
    StreakLost = 1,
    DailyChallenge = 2,
    LeagueUpdate = 3,
    AchievementUnlocked = 4,
    Milestone = 5,
    SystemMessage = 6
}

public enum NotificationSeverity
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3
}
