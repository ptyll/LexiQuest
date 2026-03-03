# UC-023: Notifikace

## Popis
Push notifikace a emailová upozornění pro engagement.

## Typy notifikací

### Push notifikace (v prohlížeči/telefonu)

| Trigger | Text | Čas |
|---------|------|-----|
| Streak ending | "Tvůj streak končí za 3h!" | 21:00 pokud nesplněno |
| Streak lost | "Streak přerušen 😢" | 00:01 následující den |
| Daily challenge | "Nová denní výzva je tu!" | 08:00 |
| League update | "Posunul jsi se na 3. místo!" | Real-time |
| Achievement | "Nový achievement: Streak Master!" | Real-time |
| Streak milestone | "🔥 7 dní! Jsi na cestě!" | Při dosažení |

### Email notifikace

| Trigger | Obsah |
|---------|-------|
| Welcome | Vítej v LexiQuest, jak začít |
| Streak warning | Upozornění 24h před ztrátou |
| Weekly league | Výsledky týdne, postup/sestup |
| Inactive | "Chybíš nám!" po 7 dnech neaktivity |
| Premium expiring | Připomenutí konce předplatného |

## Hlavní tok - Notifikace

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Systém detekuje trigger | Event v aplikaci |
| 2 | Kontrola preferencí | Uživatel má zapnuté? |
| 3 | Kontrola frekvence | Nezasílat příliš často |
| 4 | Sestavení obsahu | Personalizace |
| 5 | Odeslání | Push / Email |
| 6 | Tracking | Zaznamenat odeslání |
| 7 | Metriky | Open rate, click rate |

## DTOs

```csharp
public record NotificationPayload(
    string Title,
    string Body,
    string? Icon,
    string? Image,
    string? ActionUrl,
    Dictionary<string, string> Data
);

public record NotificationPreference(
    bool PushEnabled,
    bool EmailEnabled,
    TimeSpan? StreakReminderTime,
    bool LeagueUpdates,
    bool AchievementUnlocks,
    bool DailyChallengeReminder,
    bool WeeklySummary
);

public record NotificationHistory(
    Guid Id,
    string Type,
    string Title,
    DateTime SentAt,
    bool IsRead,
    DateTime? ReadAt,
    string? ActionTaken
);
```

## Resource klíče

```
Notification.Streak.Ending.Title
Notification.Streak.Ending.Body
Notification.Streak.Lost.Title
Notification.Streak.Lost.Body
Notification.Streak.Milestone.Title
Notification.DailyChallenge.New.Title
Notification.DailyChallenge.New.Body
Notification.League.Promotion.Title
Notification.League.Demotion.Title
Notification.Achievement.Unlocked.Title
Notification.Achievement.Unlocked.Body
Notification.Team.Invitation.Title
Notification.Multiplayer.Found.Title
```

## Odhad: 12h
