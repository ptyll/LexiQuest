# UC-016: Achievement systém

## Popis
Odměňování hráčů za specifické milníky a úspěchy ve hře.

## Kategorie achievementů

### 🎯 Výkonnostní
| ID | Název | Popis | Odměna |
|----|-------|-------|--------|
| PERF_001 | První slovo | Vyřeš první slovo | 10 XP |
| PERF_002 | Stovka | 100 vyřešených slov | 50 XP, Badge |
| PERF_003 | Tisícovka | 1 000 slov | 100 XP, Badge |
| PERF_004 | Deset tisíc | 10 000 slov | 500 XP, Legend Badge |
| PERF_005 | XP horečka | 1 000 XP za den | 100 XP |
| PERF_006 | Perfektní den | 10 slov bez chyby | 50 XP |
| PERF_007 | Speed demon | Odpověď pod 2s | 20 XP |
| PERF_008 | Marathon man | Dokonči boss maraton | 100 XP |

### 🔥 Streak
| ID | Název | Popis | Odměna |
|----|-------|-------|--------|
| STRK_001 | Začátek | 3 dny streak | Badge |
| STRK_002 | Týden | 7 dní | 50 XP, Frame |
| STRK_003 | Dva týdny | 14 dní | Avatar Frame |
| STRK_004 | Měsíc | 30 dní | 200 XP, Badge |
| STRK_005 | Půl roku | 183 dní | 500 XP, Theme |
| STRK_006 | Rok | 365 dní | Ultimate Badge, Title |

### 🧠 Obtížnostní
| ID | Název | Popis | Odměna |
|----|-------|-------|--------|
| DIFF_001 | Cesta 1 dokončena | Všechny levely | 50 XP |
| DIFF_002 | Cesta 2 dokončena | Všechny levely | 100 XP |
| DIFF_003 | Cesta 3 dokončena | Všechny levely | 200 XP |
| DIFF_004 | Cesta 4 dokončena | Všechny levely | 500 XP, Legend Title |
| DIFF_005 | Boss bez chyby | Perfektní boss | 100 XP |
| DIFF_006 | Expert režim | 10 slov v expertu | 50 XP |

### 🏆 Speciální
| ID | Název | Popis | Odměna |
|----|-------|-------|--------|
| SPEC_001 | První v lize | #1 v týdenní lize | 200 XP, Crown Badge |
| SPEC_002 | Perfect week | Žádná chyba celý týden | 300 XP |
| SPEC_003 | Early bird | Hraj před 6:00 | 20 XP |
| SPEC_004 | Night owl | Hraj po 22:00 | 20 XP |
| SPEC_005 | Věrný hráč | 100 dnů v aplikaci | Badge |
| SPEC_006 | Slovník | 500 unikátních slov | 100 XP |
| SPEC_007 | Komunikátor | 1v1 výhra | 50 XP |

## Stav achievementu

```
┌──────────────────────────────────────┐
│ 🏆 STREAK MASTERY                    │
│ ━━━━━━━━━━━━━━━━━━━━━━━━             │
│ 30 dní streak!                       │
│                                      │
│    🔥🔥🔥                            │
│                                      │
│    ZÍSKÁNO: 12. 3. 2024             │
│    Odměna: 200 XP + Avatar Frame     │
└──────────────────────────────────────┘

┌──────────────────────────────────────┐
│ 🎯 EXPERT HUNTER                     │
│ ░░░░░░░░░░░░░░░░░░░░                 │
│ 10 slov v expert režimu              │
│                                      │
│ Postup: 6 / 10                       │
│ ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓░░░░ 60%           │
└──────────────────────────────────────┘
```

## Progres tracking

```csharp
public class AchievementService
{
    public async Task CheckAndGrantAchievements(Guid userId, GameEvent gameEvent)
    {
        // Kontrolovat všechny achievementy podle typu události
        switch (gameEvent.Type)
        {
            case GameEventType.WordSolved:
                await CheckWordCountAchievements(userId);
                await CheckSpeedAchievements(userId, gameEvent.TimeMs);
                break;
            case GameEventType.LevelCompleted:
                await CheckPathAchievements(userId);
                break;
            case GameEventType.DailyStreak:
                await CheckStreakAchievements(userId);
                break;
        }
    }
}
```

## DTOs

```csharp
public record AchievementDto(
    string Id,
    string Name,
    string Description,
    string Icon,
    string Category,
    int? XPReward,
    bool IsUnlocked,
    DateTime? UnlockedAt,
    int? Progress,
    int? Target,
    double? ProgressPercentage
);

public record AchievementCategory(
    string Name,
    string Icon,
    int TotalAchievements,
    int UnlockedCount
);
```

## Resource klíče

```
Achievements.Title
Achievements.Category.Performance
Achievements.Category.Streak
Achievements.Category.Difficulty
Achievements.Category.Special
Achievements.Status.Unlocked
Achievements.Status.Locked
Achievements.Status.InProgress
Achievements.Progress.Format
Achievements.Reward.XP
Achievements.Reward.Badge
Achievements.Reward.Frame
Achievements.Reward.Theme
Achievements.UnlockDate
```

## Odhad: 12h
