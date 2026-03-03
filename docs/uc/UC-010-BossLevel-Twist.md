# UC-010: Boss Level - Twist (postupné odkrývání)

## Popis
Slova se odkrývají postupně - hráč vidí jen první N písmen zamíchána a postupně se odkrývají další.

## Pravidla Twist

```csharp
public class TwistBossRules
{
    public const int WordCount = 12;
    public const int StartingRevealed = 2;  // Kolik písmen vidí na začátku
    public const int RevealIntervalMs = 3000;  // Každé 3s odkrytí dalšího
    public const int MaxReveal = 5;  // Max odkrytých před tím než musí odpovědět
}
```

## Hlavní tok

| Krok | Akce | Příklad |
|------|------|---------|
| 1 | Začátek kola | Slovo: "POčítač" (8 písmen) |
| 2 | Zobrazení 2 zamíchaných písmen | "Č _ _ _ _ _ _ _" → zobrazí se "ČP" |
| 3 | Čekání nebo odpověď | Hráč může hádat nebo čekat |
| 4 | Po 3s se odkryje další | "ČPA _ _ _ _ _ _" |
| 5 | Odkrývání pokračuje | "ČPAÍ _ _ _ _ _" → "ČPAÍT _ _ _ _" |
| 6 | Max 5 odkrytých | "ČPAÍTČ _ _ _" |
| 7 | Hráč musí odpovědět | Timeout nebo submit |
| 8 | Vyhodnocení | Bonus za brzkou odpověď |

## Bonus za brzkou odpověď

| Odkrytá písmena | Bonus XP |
|-----------------|----------|
| 2 (ihned) | +10 XP |
| 3 | +7 XP |
| 4 | +5 XP |
| 5 | +2 XP |
| Timeout (vše) | 0 XP |

## Vizualizace

```
┌─────────────────────────────────────┐
│ 🔥 BOSS TWIST 🔥                    │
│                                     │
│ Vidíš jen část slova:               │
│                                     │
│    ┌───┐ ┌───┐ ┌───┐ ┌───┐         │
│    │ Č │ │ P │ │ ? │ │ ? │         │
│    └───┘ └───┘ └───┘ └───┘         │
│                                     │
│ Další písmeno za: 2s ⏱️            │
│                                     │
│ [__________] [Tipnout hned!]       │
│                                     │
│ Tip: Za brzký tip získáš bonus XP   │
└─────────────────────────────────────┘
```

## DTOs

```csharp
public record TwistRoundState(
    int TotalLetters,
    int RevealedCount,
    List<char> RevealedLetters,  // Pozice + písmena
    int TimeUntilNextRevealMs,
    int CurrentBonusXP
);

public record TwistGuessResult(
    bool Correct,
    int RevealedWhenGuessed,
    int BonusXPEarned,
    string CorrectAnswer
);
```

## Resource klíče

```
Boss.Twist.Title
Boss.Twist.Intro
Boss.Twist.Rule
Boss.Twist.Revealed.Label
Boss.Twist.Hidden.Label
Boss.Twist.NextReveal
Boss.Twist.GuessEarly
Boss.Twist.Bonus.Potential
Boss.Twist.Bonus.Earned
Boss.Twist.Timeout
```

## Odhad: 8h
