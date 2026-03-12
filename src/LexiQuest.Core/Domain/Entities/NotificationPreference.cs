namespace LexiQuest.Core.Domain.Entities;

public class NotificationPreference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public bool PushEnabled { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool StreakReminder { get; private set; }
    public TimeSpan StreakReminderTime { get; private set; }
    public bool LeagueUpdates { get; private set; }
    public bool AchievementNotifications { get; private set; }
    public bool DailyChallengeReminder { get; private set; }

    private NotificationPreference() { }

    public static NotificationPreference CreateDefault(Guid userId)
    {
        return new NotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PushEnabled = true,
            EmailEnabled = true,
            StreakReminder = true,
            StreakReminderTime = TimeSpan.FromHours(21),
            LeagueUpdates = true,
            AchievementNotifications = true,
            DailyChallengeReminder = true
        };
    }

    public void Update(
        bool pushEnabled,
        bool emailEnabled,
        bool streakReminder,
        TimeSpan streakReminderTime,
        bool leagueUpdates,
        bool achievementNotifications,
        bool dailyChallengeReminder)
    {
        PushEnabled = pushEnabled;
        EmailEnabled = emailEnabled;
        StreakReminder = streakReminder;
        StreakReminderTime = streakReminderTime;
        LeagueUpdates = leagueUpdates;
        AchievementNotifications = achievementNotifications;
        DailyChallengeReminder = dailyChallengeReminder;
    }
}
