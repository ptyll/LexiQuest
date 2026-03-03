# UC-009: Boss Level - Podmínka (zakázané písmeno)

## Popis
Boss level kde každé 3. slovo má zakázané určité písmeno - odpověď nesmí toto písmeno obsahovat.

## Pravidla Podmínky

```csharp
public class ConditionBossRules
{
    public const int WordCount = 15;
    public const int ForbiddenInterval = 3;  // Každé 3. slovo
    public const int PenaltyXP = -5;  // Za použití zakázaného písmena
}
```

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Hráč začne Boss Level | - |
| 2 | Zobrazení pravidel | "Každé 3. slovo má zakázané písmeno" |
| 3 | Slovo 1 - normální | Žádné omezení |
| 4 | Slovo 2 - normální | Žádné omezení |
| 5 | Slovo 3 - zakázané písmeno | Zobrazení varování |
| 6 | Hráč píše odpověď | Systém kontroluje zakázané písmeno |
| 7a | Odpověď obsahuje zakázané písmeno | Penalizace -5 XP, počítá se jako chyba |
| 7b | Odpověď neobsahuje zakázané písmeno | Normální XP |
| 8 | Rotace pokračuje | Slovo 4 normální, 5 normální, 6 zakázané... |

## Příklad

```
Slovo 1: "KOCKA" (normální)
  Odpověď: KOCKA ✓ +10 XP

Slovo 2: "MYS" (normální)
  Odpověď: MYS ✓ +10 XP

Slovo 3: "RYBA" (ZAKÁZANÉ: B)
  ⚠️ Varování: "Odpověď nesmí obsahovat písmeno 'B'!"
  
  Odpověď: RYBA ✗ -5 XP (obsahuje B)
  Správná odpověď mohla být: "RYBA" ale to nejde!
  
  Poznámka: V tomto módu se vybírají slova 
  která MAJÍ alternativu bez zakázaného písmena
  
Slovo 3 alternativa: "KAPR" (nemá B)
  Odpověď: KAPR ✓ +10 XP
```

## Výběr slov pro Podmínku

```csharp
public Word SelectWordForConditionRound(List<Word> availableWords, char forbiddenLetter)
{
    // Filtrovat slova která:
    // 1. Neobsahují forbiddenLetter
    // 2. Jsou z stejné kategorie obtížnosti
    
    var validWords = availableWords
        .Where(w => !w.Original.Contains(forbiddenLetter))
        .ToList();
        
    return validWords[Random.Next(validWords.Count)];
}
```

## DTOs

```csharp
public record ConditionRoundInfo(
    bool HasForbiddenLetter,
    char? ForbiddenLetter,
    string WarningMessage,
    int PenaltyForViolation
);

public record ConditionBossResult(
    bool Defeated,
    int TimesConditionViolated,
    int TotalPenalty,
    int FinalXP
);
```

## Resource klíče

```
Boss.Condition.Title
Boss.Condition.Intro
Boss.Condition.Rule
Boss.Condition.ForbiddenLetter
Boss.Condition.Warning
Boss.Condition.Violation.Penalty
Boss.Condition.Success.Avoided
Boss.Condition.Progress.Forbidden
```

## Odhad: 8h
