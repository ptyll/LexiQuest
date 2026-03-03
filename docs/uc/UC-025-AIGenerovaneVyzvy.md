# UC-025: AI Generované výzvy

## Popis
Pokročilá funkce - AI generuje personalizované výzvy na základě slabých míst hráče.

## Koncept

```
Systém analyzuje:
- Která písmena hráč nejčastěji přehazuje
- Jaké délky slov dělají problém
- Jaké kategorie (slovesa, podstatná jména)
- Rychlost odpovědí

AI vygeneruje:
- Personalizovanou sadu slov
- Tipy na zlepšení
- Cvičení na slabá místa
```

## AI Challenge typy

| Typ | Popis |
|-----|-------|
| Weakness Focus | Cvičení na písmena s chybami > 30% |
| Speed Training | Krátká slova pro trénink rychlosti |
| Memory Game | Opakující se slova pro zapamatování |
| Pattern Recognition | Slova s podobnou strukturou |

## DTOs

```csharp
public record AIChallengeRequest(
    Guid UserId,
    AIChallengeType Type,
    int WordCount = 10
);

public record AIChallenge(
    Guid Id,
    string Title,
    string Description,
    AIChallengeType Type,
    List<AIChallengeWord> Words,
    string? Tip,  // AI tip pro zlepšení
    DateTime GeneratedAt
);

public record AIChallengeWord(
    Word Word,
    string? WhyIncluded,  // Proč AI vybralo toto slovo
    double? PredictedDifficulty  // 0-1
);

public enum AIChallengeType
{
    WeaknessFocus,
    SpeedTraining,
    MemoryGame,
    PatternRecognition,
    Mixed
}
```

## Resource klíče

```
AI.Title
AI.Challenge.New
AI.Challenge.Personalized
AI.Challenge.WeaknessFocus
AI.Challenge.SpeedTraining
AI.Challenge.MemoryGame
AI.Challenge.PatternRecognition
AI.Tip.Title
AI.Tip.Example
AI.Stats.Improvement
AI.Button.Generate
AI.Loading.Generating
```

## Odhad: 12h (bez skutečné AI, jen mock)

Poznámka: Skutečná AI integrace by byla výrazně složitější a nákladnější.
