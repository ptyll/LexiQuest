# UC-020: Multiplayer 1v1

## Popis
Souboj dvou hráčů v reálném čase - kdo vyřeší více slov za daný čas.

## Pravidla 1v1

| Parametr | Hodnota |
|----------|---------|
| Délka zápasu | 3 minuty |
| Počet slov | 15 (nebo čas) |
| Výhra | Více správných odpovědí |
| Remíza | Stejný počet = rychlejší čas |

## Hlavní tok

| Krok | Akce | Popis |
|------|------|-------|
| 1 | Hráč klikne "1v1 Souboj" | - |
| 2 | Systém přidá do matchmakingu | SignalR |
| 3 | Čekání na soupeře | Zobrazení "Hledání soupeře..." |
| 4 | Soupeř nalezen | Zobrazení profilů |
| 5 | Odpočet 3, 2, 1... | - |
| 6 | Zápas začne | Oba vidí stejná slova |
| 7 | Hráči řeší současně | Real-time progress bar |
| 8 | Systém aktualizuje skóre | SignalR broadcast |
| 9 | Čas vyprší nebo všechna slova | Konec zápasu |
| 10 | Vyhodnocení | Zobrazení vítěze |
| 11 | XP odměny | Oba dostanou XP |
| 12 | Návrat do lobby | - |

## Real-time UI

```
┌────────────────────────────────────────┐
│ ⚔️ SOUBOJ - 1:45 zbývá                │
├────────────────────┬───────────────────┤
│ 👤 Ty              │ 👤 Soupeř123      │
│ 8/15 správně       │ 6/15 správně      │
│ ████████░░░░░░░░░░ │ ██████░░░░░░░░░░ │
├────────────────────┴───────────────────┤
│                                        │
│    Současné slovo:                     │
│    P Ř E S M Y Č K A                   │
│                                        │
│    [________________]                  │
│                                        │
│    Rychlost: 🔥🔥🔥 (combo x3)         │
└────────────────────────────────────────┘
```

## Stav zápasu

```csharp
public record MatchState(
    Guid MatchId,
    MatchStatus Status,  // Waiting, Countdown, Playing, Finished
    PlayerState Player1,
    PlayerState Player2,
    List<Word> Words,
    TimeSpan RemainingTime
);

public record PlayerState(
    Guid UserId,
    string Username,
    string AvatarUrl,
    int CurrentWordIndex,
    int CorrectAnswers,
    int WrongAnswers,
    int CurrentStreak,
    TimeSpan TotalTimeSpent
);
```

## Matchmaking

```csharp
public class MatchmakingService
{
    private readonly ConcurrentQueue<MatchmakingRequest> _queue = new();
    
    public async Task<Match> FindMatchAsync(Guid userId)
    {
        // Přidat do fronty
        _queue.Enqueue(new MatchmakingRequest(userId, DateTime.UtcNow));
        
        // Čekat na párování (max 30s)
        // Poté AI soupeř nebo zrušit
    }
}
```

## DTOs

```csharp
public record JoinMatchmakingRequest();
public record MatchFoundEvent(
    Guid MatchId,
    PlayerInfo Opponent,
    DateTime StartTime
);

public record SubmitAnswerRealtimeRequest(
    Guid MatchId,
    string Answer,
    int WordIndex
);

public record MatchResult(
    Guid WinnerId,
    bool IsDraw,
    PlayerResult Player1Result,
    PlayerResult Player2Result,
    int XPEarned,
    string? RankChange
);

public record PlayerResult(
    Guid UserId,
    int CorrectAnswers,
    int WrongAnswers,
    TimeSpan TotalTime,
    int MaxStreak
);
```

## SignalR Hub

```csharp
public interface IMatchHub
{
    Task JoinMatchmaking();
    Task LeaveMatchmaking();
    Task SubmitAnswer(Guid matchId, string answer, int wordIndex);
    Task Forfeit(Guid matchId);
}

public interface IMatchClient
{
    Task MatchFound(MatchInfo match);
    Task MatchStarting(int countdownSeconds);
    Task RoundStarted(int wordIndex, string scrambled);
    Task OpponentProgress(int wordIndex, bool wasCorrect);
    Task MatchEnded(MatchResult result);
    Task OpponentDisconnected();
}
```

## Resource klíče

```
Multiplayer.Title
Multiplayer.Mode.1v1
Multiplayer.Matchmaking.Searching
Multiplayer.Matchmaking.Found
Multiplayer.Match.Starting
Multiplayer.Match.TimeRemaining
Multiplayer.Match.YourProgress
Multiplayer.Match.OpponentProgress
Multiplayer.Match.Victory
Multiplayer.Match.Defeat
Multiplayer.Match.Draw
Multiplayer.Match.XPEarned
Multiplayer.Error.Disconnected
Multiplayer.Error.Timeout
```

## Odhad: 20h (SignalR + real-time je komplexní)
