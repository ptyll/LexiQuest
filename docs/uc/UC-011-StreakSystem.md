# UC-011: Streak systém

## Popis
Systém denního návyku - uživatel musí splnit alespoň 1 level denně pro udržení streaku.

## Pravidla

```csharp
public class StreakRules
{
    // Reset nastane pokud uživatel nesplní level
    // v časovém okně 24h od posledního splnění
    
    public const int DailyRequirement = 1;  // Min 1 level
    public TimeSpan StreakWindow = TimeSpan.FromHours(48);  // Grace period
    
    // Fire levels
    public static string GetFireLevel(int days) => days switch
    {
        0 => "cold",
        <= 3 => "small",      // 🔥
        <= 7 => "medium",     // 🔥🔥
        <= 30 => "large",     // 🔥🔥🔥
        _ => "legendary"      // 🔥🔥🔥🔥
    };
}
```

## Hlavní tok - Udržení streaku

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel hraje | Jakýkoliv režim |
| 2 | Dokončí 1 level | Minimální požadavek |
| 3 | Systém kontroluje poslední aktivitu | - |
| 4a | Poslední aktivita < 24h | Streak +1 |
| 4b | Poslední aktivita > 24h ale < 48h | Streak pokračuje (grace period) |
| 4c | Poslední aktivita > 48h | Streak reset na 1 |
| 5 | Aktualizace LastActivityDate | - |
| 6 | Zobrazení aktuálního streaku | Animace |

## Streak stavy

```
┌──────────────────────────────────────┐
│                                      │
│     🔥🔥🔥                           │
│                                      │
│   15 dní v řadě!                     │
│                                      │
│   Další den za: 8h 32m               │
│   ▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░ 68%           │
│                                      │
│   🛡️ Streak Shield dostupný         │
│                                      │
└──────────────────────────────────────┘
```

## Milestones (Achievementy)

| Dny | Odměna |
|-----|--------|
| 3 | Badge "Začátek cesty" |
| 7 | Badge "Týden věrnosti" + 50 XP |
| 14 | Badge "Dva týdny" + Avatar frame |
| 30 | Badge "Měsíc mistra" + 200 XP |
| 50 | Badge "Nezastavitelný" + exkluzivní téma |
| 100 | Badge "Legenda" + speciální titul |
| 365 | Badge "Rok v řadě" - Ultimate achievement |

## DTOs

```csharp
public record StreakStatus(
    int CurrentStreak,
    int LongestStreak,
    DateTime LastActivityDate,
    DateTime NextResetTime,
    TimeSpan TimeUntilReset,
    double ProgressPercentage,  // % dne uběhlo
    string FireLevel,  // small, medium, large, legendary
    bool IsAtRisk,  // < 6 hodin do resetu
    bool IsInGracePeriod,
    bool CanFreeze,  // Premium funkce
    int ShieldsAvailable
);

public record StreakMilestone(
    int DaysRequired,
    string BadgeName,
    string BadgeIcon,
    int? XPReward,
    string? RewardType  // Badge, Frame, Theme, Title
);
```

## Resource klíče

```
Streak.Title
Streak.Days.Singular
Streak.Days.Plural2-4
Streak.Days.Plural5Plus
Streak.Status.Active
Streak.Status.AtRisk
Streak.Status.Reset
Streak.TimeUntilReset
Streak.Milestone.Reached
Streak.Fire.Small
Streak.Fire.Medium
Streak.Fire.Large
Streak.Fire.Legendary
```

## Odhad: 10h
