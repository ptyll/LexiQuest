# UC-008: Boss Level - Maraton

## Popis
Speciální boss level, kde hráč musí zodpovědět 20 slov bez obnovy životů mezi koly.

## Pravidla Maratonu

```csharp
public class MarathonBossRules
{
    public const int WordCount = 20;
    public const int StartingLives = 3;  // Neobnovují se!
    public const int MaxTimePerWord = 15;  // sekund
    public const int PerfectBonusXP = 50;  // Bez ztráty života
}
```

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Hráč začne Boss Level | Úvodní animace |
| 2 | Zobrazí se pravidla | "20 slov, 3 životy, neobnovují se" |
| 3 | Hra začne - slovo 1/20 | - |
| 4 | Hráč odpoví | Správně/Špatně |
| 5a | Správně | Pokračování na další slovo |
| 5b | Špatně | -1 život, pokračování |
| 6 | Kontrola životů | - |
| 6a | Životy > 0 | Slovo 2/20, 3/20... |
| 6b | Životy = 0 | Game Over Boss |
| 7 | Slovo 20/20 dokončeno | Boss poražen! |
| 8 | Výpočet odměn | XP + odměny |
| 9 | Zobrazení výsledků | Statistiky maratonu |

## Vizualizace průběhu

```
┌─────────────────────────────────────┐
│ 🔥 BOSS MARATON 🔥                  │
│                                     │
│ Postup: ████████████████░░░░ 16/20  │
│ Životy:  ❤️ ❤️ 🖤                   │
│                                     │
│ Aktuální slovo:                     │
│         R E T E Z E C               │
│                                     │
│ [________________] [Potvrdit]      │
│                                     │
│ Combo: x5 | XP: 240                 │
└─────────────────────────────────────┘
```

## Odměny

| Výsledek | XP Bonus | Odměna |
|----------|----------|--------|
| Dokončeno (se ztrátou životů) | +100 XP | Marathon Survivor badge |
| Perfektní (bez ztráty životů) | +200 XP | Marathon Master badge + Avatar frame |
| Rychlost (< 5 min celkem) | +50 XP | Speed Demon badge |

## DTOs

```csharp
public record MarathonBossSession(
    Guid Id,
    int CurrentWordNumber,
    int TotalWords,
    int LivesRemaining,
    int StartingLives,
    TimeSpan TotalTimeElapsed,
    int WordsCorrect,
    int WordsWrong,
    bool IsPerfectSoFar
);

public record MarathonResult(
    bool Defeated,
    bool Perfect,
    int TotalXP,
    TimeSpan TimeElapsed,
    int WordsSolved,
    List<string> EarnedBadges
);
```

## Resource klíče

```
Boss.Marathon.Title
Boss.Marathon.Intro
Boss.Marathon.Rules.Line1
Boss.Marathon.Rules.Line2
Boss.Marathon.Rules.Line3
Boss.Marathon.Progress
Boss.Marathon.Lives.Remaining
Boss.Marathon.Victory.Title
Boss.Marathon.Victory.Message
Boss.Marathon.Defeat.Title
Boss.Marathon.Defeat.Message
Boss.Marathon.Reward.Perfect
Boss.Marathon.Reward.Survivor
Boss.Marathon.Reward.Speed
```

## Odhad: 8h
