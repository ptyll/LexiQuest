# UC-014: Denní výzva

## Popis
Jedno speciální slovo denně pro všechny uživatele - stejná výzva, různá obtížnost.

## Struktura denní výzvy

```csharp
public class DailyChallenge
{
    public DateTime Date { get; set; }
    public Word Word { get; set; }
    public DailyModifier Modifier { get; set; }  // Speciální pravidlo
    public int BaseXP { get; set; } = 25;
}

public enum DailyModifier
{
    None,           // Normální
    Speed,          // 2x XP za rychlost
    NoHints,        // Bez nápověd, 2x XP
    DoubleLetters,  // Každé písmeno 2x ve slově
    Category,       // Slovo z konkrétní kategorie
    Hard,           // Dlouhé slovo
    Easy            // Krátké slovo, 0.5x XP
}
```

## Denní harmonogram

| Den | Modifier | Popis |
|-----|----------|-------|
| Pondělí | Category: Jídlo | Všechna slova o jídle |
| Úterý | Speed | 2x XP za rychlost |
| Středa | NoHints | Bez nápověd, 2x XP |
| Čtvrtek | DoubleLetters | "BANANA" → písmena se opakují |
| Pátek | Team | Týmový součet XP |
| Sobota | Hard | Expert úroveň |
| Neděle | Easy | Relaxační den |

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Uživatel otevře Denní výzvu | - |
| 2 | Zobrazení dnešního slova | Zamíchané |
| 3 | Zobrazení modifikátoru | Dnešní speciální pravidlo |
| 4 | Hráč řeší | Stejná mechanika jako normální hra |
| 5 | Vyhodnocení | XP podle modifikátoru |
| 6 | Zobrazení žebříčku | Kdo dnes vyřešil nejrychleji |

## Žebříček denní výzvy

```
┌────────────────────────────────────────┐
│ 📅 Dnešní výzva - Úterý               │
│ Modifikátor: ⚡ Rychlost (2x XP)       │
├────────────────────────────────────────┤
│ Slovo:  P R O G R A M Á T O R          │
│                                        │
│ Tvůj čas: 4.2s                        │
│ Získáno: 50 XP                        │
├────────────────────────────────────────┤
│ Dnešní žebříček:                       │
│ 1. 🥇 Rychlík     2.1s                │
│ 2. 🥈 Ty          4.2s  👤            │
│ 3. 🥉 SlovoMistr  5.8s                │
└────────────────────────────────────────┘
```

## DTOs

```csharp
public record DailyChallengeDto(
    DateTime Date,
    string ScrambledWord,
    int WordLength,
    DailyModifier Modifier,
    string ModifierDescription,
    int BaseXP,
    bool IsCompleted,
    TimeSpan? UserTime,
    int? UserXPEarned
);

public record DailyLeaderboardEntry(
    int Rank,
    string Username,
    TimeSpan SolveTime,
    int XPEarned,
    bool IsCurrentUser
);
```

## Resource klíče

```
Daily.Title
Daily.Today
Daily.Modifier.None
Daily.Modifier.Speed
Daily.Modifier.NoHints
Daily.Modifier.DoubleLetters
Daily.Modifier.Category
Daily.Modifier.Hard
Daily.Modifier.Easy
Daily.Leaderboard.Title
Daily.Status.Completed
Daily.Status.NotCompleted
Daily.Time.Your
Daily.XP.Earned
```

## Odhad: 8h
