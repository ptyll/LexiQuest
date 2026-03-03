# UC-004: Základní herní smyčka (přesmyčka)

## Popis
Core herní mechanika - uživatel dostane zamíchané slovo a musí ho rozluštit.

## Aktéři
- **Primary Actor:** Přihlášený hráč

## Herní režimy
1. **Trénink** - nekonečný, žádný tlak
2. **Časovka** - limit 1-3 minuty
3. **Cesta** - postup levely ve výuce
4. **Denní výzva** - jedno slovo denně pro všechny

## Post-conditions
**Úspěch:**
- XP přičteno
- Statistiky aktualizovány
- Případně postup na další level

**Neúspěch:**
- Ztráta života (pokud režim používá životy)
- Konec hry při 0 životech

## Hlavní tok

| Krok | Akce | Data | Poznámka |
|------|------|------|----------|
| 1 | Uživatel vybere režim | mode: GameMode | - |
| 2 | Systém vytvoří GameSession | sessionId: GUID | Uloží do DB |
| 3 | Systém vybere slovo | word: Word | Podle obtížnosti |
| 4 | Systém zamíchá písmena | scrambled: string | Fisher-Yates |
| 5 | Systém zobrazí zamíchané slovo | - | Animace shuffle |
| 6 | Systém spustí časovač | timer: Timer | Podle obtížnosti |
| 7 | Uživatel píše odpověď | answer: string | Real-time validace |
| 8 | Uživatel potvrdí (Enter/tlačítko) | - | - |
| 9 | Systém validuje odpověď | - | Case-insensitive, trim |
| 10 | Systém vypočítá XP | baseXP + bonusy | Rychlost, streak, combo |
| 11 | Zobrazí se feedback | - | Zelená/červená animace |
| 12 | Systém aktualizuje statistiky | - | - |
| 13 | Přechod na další slovo nebo konec | - | - |

## Výpočet XP

```csharp
public int CalculateXP(GameRound round, UserStats stats)
{
    int baseXP = 10;
    
    // Rychlostní bonus
    double timeBonus = round.TimeSpentMs switch
    {
        < 3000 => 5,    // < 3s = +5 XP
        < 5000 => 3,    // < 5s = +3 XP
        < 10000 => 1,   // < 10s = +1 XP
        _ => 0
    };
    
    // Combo multiplikátor
    double comboMultiplier = stats.CurrentCombo switch
    {
        >= 10 => 2.0,
        >= 5 => 1.5,
        >= 3 => 1.2,
        _ => 1.0
    };
    
    // Bezchybný streak bonus
    int streakBonus = stats.CorrectStreak >= 5 ? 2 : 0;
    
    return (int)((baseXP + timeBonus + streakBonus) * comboMultiplier);
}
```

## Časové limity podle obtížnosti

| Obtížnost | Čas na slovo | Nápověda |
|-----------|--------------|----------|
| Začátečník | 30s | Zdarma, vždy dostupná |
| Mírně pokročilý | 25s | Zdarma, max 3x |
| Pokročilý | 15s | Stojí 2 XP |
| Expert | 10s | Stojí 5 XP (gamble) |

## Business pravidla

| ID | Pravidlo |
|----|----------|
| BR-101 | Odpověď je case-insensitive |
| BR-102 | Odpověď se trimuje (mezery na začátku/konci) |
| BR-103 | Diakritika musí odpovídat ("řeka" ≠ "reka") |
| BR-104 | Zamíchané slovo nesmí být stejné jako originál |
| BR-105 | Combo se resetuje při chybě |
| BR-106 | Časovač běží jen když je okno aktivní (focus) |

## DTOs

```csharp
public record StartGameRequest(
    GameMode Mode,
    Guid? PathId,       // pro režim Cesta
    Difficulty? Difficulty  // pro Trénink
);

public record SubmitAnswerRequest(
    Guid SessionId,
    string Answer,
    int TimeSpentMs
);

public record GameRoundResult(
    bool IsCorrect,
    int XPEarned,
    int TotalXP,
    string CorrectAnswer,  // zobrazeno jen při špatné odpovědi
    int LivesRemaining,
    int CurrentCombo,
    TimeSpan? NextWordDelay
);

public record ScrambledWordDto(
    string Scrambled,
    int Length,
    int? RevealedLettersCount,  // pro Twist mód
    List<char>? ForbiddenLetters  // pro Podmínka mód
);
```

## Validátory

```csharp
public class SubmitAnswerRequestValidator : AbstractValidator<SubmitAnswerRequest>
{
    public SubmitAnswerRequestValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TimeSpentMs).GreaterThanOrEqualTo(0);
    }
}
```

## Resource klíče

```
Game.Answer.Placeholder
Game.Answer.Submit
Game.Answer.Skip
Game.Timer.Remaining
Game.XP.Earned
Game.XP.TimeBonus
Game.XP.Combo
Game.Feedback.Correct
Game.Feedback.Wrong
Game.Feedback.TimeUp
Game.Hint.Button
Game.Hint.Cost
Game.Hint.NotAvailable
Game.Hint.RevealLetter
Game.Lives.Remaining
Game.Combo.Multiplier
Game.Round.Progress
```

## Test Cases

```csharp
[Fact]
public void SubmitAnswer_CorrectAnswer_IncreasesXP()
{
    // Arrange
    var session = CreateGameSession();
    var word = new Word("TEST", "TSET");
    
    // Act
    var result = _gameService.SubmitAnswer(session.Id, "test", 2000);
    
    // Assert
    result.IsCorrect.Should().BeTrue();
    result.XPEarned.Should().BeGreaterThan(10); // base + speed bonus
}

[Fact]
public void SubmitAnswer_CaseInsensitive_AcceptsLowercase()
{
    var result = _gameService.SubmitAnswer(session.Id, "test", 5000);
    result.IsCorrect.Should().BeTrue();
}

[Fact]
public void SubmitAnswer_WrongAnswer_DecreasesLife()
{
    var result = _gameService.SubmitAnswer(session.Id, "wrong", 5000);
    result.IsCorrect.Should().BeFalse();
    result.LivesRemaining.Should().Be(2);
}
```

## Odhad: 20h (core herní logika je nejsložitější část)
