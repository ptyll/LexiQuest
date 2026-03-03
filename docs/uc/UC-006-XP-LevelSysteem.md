# UC-006: XP a Level systém

## Popis
Systém zkušenostních bodů (XP) progresuje hráče napříč aplikací a odemyká nové obsahy.

## Zdroje XP

| Akce | Základní XP | Bonusy |
|------|-------------|--------|
| Správná odpověď | 10 | Rychlost, Combo, Streak |
| Boss level dokončený | 50 | - |
| Denní výzva splněná | 25 | - |
| Cesta dokončená | 100 | - |
| Achievement odemčený | 10-100 | Podle vzácnosti |
| Perfektní level (bez chyby) | 25 | - |

## Level struktura

```csharp
public static class LevelCalculator
{
    // Exponenciální křivka
    public static int XPNeededForLevel(int level)
    {
        return (int)(100 * Math.Pow(1.5, level - 1));
    }
    
    public static int GetLevelFromXP(int totalXP)
    {
        int level = 1;
        int xpNeeded = 100;
        
        while (totalXP >= xpNeeded)
        {
            totalXP -= xpNeeded;
            level++;
            xpNeeded = XPNeededForLevel(level);
        }
        
        return level;
    }
}
```

| Level | XP potřeba | Celkem XP | Odemknutí |
|-------|------------|-----------|-----------|
| 1 | 100 | 100 | - |
| 2 | 150 | 250 | Path 2 |
| 3 | 225 | 475 | Nový avatar |
| 4 | 337 | 812 | - |
| 5 | 506 | 1318 | Path 3 |
| 10 | 3844 | 11348 | Zlatá liga |
| 20 | 221684 | 665796 | Diamantová liga |

## XP Bar komponenta

```
┌─────────────────────────────────────────┐
│ ████████████░░░░░░░░░░░░░░░░░░░░  1250  │
│ Lvl 5 ────────────────────────▶ Lvl 6   │
│ 1250 / 1500 XP (83%)                     │
└─────────────────────────────────────────┘
```

## Level Up ceremonie

```
Když TotalXP >= XPNeeded:
1. Pauza hry
2. Animace: XP bar se plní do konce
3. Exploze konfety
4. "LEVEL UP!" velký text
5. Zobrazení odměny (co se odemklo)
6. Pokračování hry
```

## DTOs

```csharp
public record XPProgress(
    int CurrentLevel,
    int CurrentXP,
    int XPNeededForNextLevel,
    int XPProgressInCurrentLevel,
    double ProgressPercentage,
    List<UnlockableReward> UnlockedRewards
);

public record UnlockableReward(
    string Type,  // Path, Avatar, League, Feature
    string Name,
    string Description,
    string IconUrl
);

public record XPGainedEvent(
    int Amount,
    string Source,
    List<XPBreakdown> Breakdown
);

public record XPBreakdown(
    string Type,  // Base, Speed, Combo, Streak, Perfect
    int Amount
);
```

## Resource klíče

```
XP.Bar.Label
XP.Level.Current
XP.Level.Next
XP.Gained.Title
XP.Gained.Base
XP.Gained.Speed
XP.Gained.Combo
XP.Gained.Streak
XP.Gained.Perfect
XP.LevelUp.Title
XP.LevelUp.Message
XP.LevelUp.Reward
XP.Unlock.Path
XP.Unlock.Avatar
XP.Unlock.League
```

## Odhad: 6h
