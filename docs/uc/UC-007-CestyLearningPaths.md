# UC-007: Cesty (Learning Paths)

## Popis
Strukturované herní cesty podobné Duolingu - uživatel postupuje úrovněmi od jednoduchých po obtížná slova.

## Struktura cest

### Cesta 1 - Začátečník (🌱)
- **Levely:** 20
- **Slova:** 3-5 písmen
- **Čas:** Bez limitu
- **Nápověda:** Vždy dostupná
- **Kategorie:** Zvířata, Jídlo, Barvy

### Cesta 2 - Mírně pokročilý (🌿)
- **Levely:** 25
- **Slova:** 5-7 písmen
- **Čas:** 30s na slovo
- **Nápověda:** Max 3x za level
- **Požadavek:** Dokončit Cestu 1 nebo Level 5

### Cesta 3 - Pokročilý (🌳)
- **Levely:** 30
- **Slova:** 7-10 písmen
- **Čas:** 20s na slovo
- **Nápověda:** Stojí 2 XP
- **Požadavek:** Dokončit Cestu 2

### Cesta 4 - Expert (🔥)
- **Levely:** 40
- **Slova:** 10+ písmen
- **Čas:** 10s na slovo
- **Nápověda:** Stojí 5 XP (gamble)
- **Extra:** Falešné stopy, zamíchaná věty
- **Požadavek:** Dokončit Cestu 3

## Vizualizace cesty

```
    🏁
     │
    20 (BOSS)
     │
    19
    ╱ ╲
  17   18
   │
  ...
   │
  🌱 1 (START)
```

## Hlavní tok - Výběr levelu

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel otevře stránku Cesty | - |
| 2 | Systém zobrazí všechny dostupné cesty | Zamčené jsou šedé |
| 3 | Uživatel vybere aktivní cestu | - |
| 4 | Systém zobrazí mapu levelů | Aktuální pozice je zvýrazněná |
| 5 | Uživatel klikne na dostupný level | Nedokončené nebo opakování |
| 6 | Systém zobrazí detail levelu | Počet slov, odměna XP |
| 7 | Uživatel klikne "Start" | - |
| 8 | Zahájí se herní smyčka UC-004 | - |

## Stav levelu

```csharp
public enum LevelStatus
{
    Locked,      // Zamčeno - šedá ikona
    Available,   // Dostupné - zelená/obrysová
    Current,     // Aktuální - pulzuje, animace
    Completed,   // Dokončeno - zlatá/zelená plná
    Perfect,     // Perfektní - diamant/zlatá s hvězdou
    Boss         // Boss level - speciální ikona
}
```

## DTOs

```csharp
public record LearningPath(
    Guid Id,
    string Name,
    string Description,
    string Icon,
    DifficultyLevel Difficulty,
    int TotalLevels,
    int UnlockedLevels,
    int CompletedLevels,
    bool IsUnlocked,
    string UnlockRequirement
);

public record PathLevel(
    int LevelNumber,
    LevelStatus Status,
    int WordsCount,
    int XPReward,
    bool IsBoss,
    string BossType,  // Maraton, Podminka, Twist
    List<RewardDto> Rewards
);

public record LevelProgress(
    int CurrentWordIndex,
    int TotalWords,
    int CorrectAnswers,
    int WrongAnswers,
    int XPEarnedSoFar
);
```

## Resource klíče

```
Paths.Title
Paths.SelectPath
Paths.Level.Locked
Paths.Level.Available
Paths.Level.Current
Paths.Level.Completed
Paths.Level.Perfect
Paths.Boss.Label
Paths.Boss.Description
Paths.Reward.XP
Paths.Reward.Unlock
Paths.Start.Button
Paths.Retry.Button
Paths.Continue.Button
Paths.Path1.Name
Paths.Path1.Description
Paths.Path2.Name
Paths.Path2.Description
Paths.Path3.Name
Paths.Path3.Description
Paths.Path4.Name
Paths.Path4.Description
```

## Odhad: 12h
