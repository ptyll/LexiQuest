using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class GameSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public GameMode Mode { get; private set; }
    public Guid? PathId { get; private set; }
    public int? LevelNumber { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public int CurrentRound { get; private set; }
    public int TotalRounds { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int LivesRemaining { get; private set; }
    public int TotalXP { get; private set; }
    public int ComboCount { get; private set; }
    public int CorrectAnswers { get; private set; }
    public List<GameRound> Rounds { get; private set; } = [];
    public GameSessionStatus Status { get; private set; }

    // Stored word IDs for the game
    private List<Guid> _wordIds = [];

    private GameSession() { }

    public static GameSession Create(
        Guid userId,
        GameMode mode,
        DifficultyLevel difficulty,
        int totalRounds,
        int lives,
        Guid? pathId = null,
        int? levelNumber = null)
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Mode = mode,
            Difficulty = difficulty,
            TotalRounds = totalRounds,
            LivesRemaining = lives,
            PathId = pathId,
            LevelNumber = levelNumber,
            CurrentRound = 1,
            StartedAt = DateTime.UtcNow,
            TotalXP = 0,
            ComboCount = 0,
            CorrectAnswers = 0,
            Status = GameSessionStatus.InProgress,
            Rounds = []
        };
    }

    public void SetWordIds(List<Guid> wordIds)
    {
        _wordIds = wordIds;
    }

    public Guid GetWordIdForRound(int roundNumber)
    {
        if (roundNumber < 1 || roundNumber > _wordIds.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(roundNumber));
        }
        return _wordIds[roundNumber - 1];
    }

    public void RecordCorrectAnswer()
    {
        ComboCount++;
        CorrectAnswers++;
    }

    public void RecordWrongAnswer()
    {
        ComboCount = 0;
        LivesRemaining--;
        
        if (LivesRemaining <= 0)
        {
            Fail();
        }
    }

    public void AddXP(int xp)
    {
        TotalXP += xp;
    }

    public void AdvanceToNextRound()
    {
        CurrentRound++;
    }

    public void Complete()
    {
        Status = GameSessionStatus.Completed;
        EndedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = GameSessionStatus.Failed;
        EndedAt = DateTime.UtcNow;
    }

    public void Forfeit()
    {
        Status = GameSessionStatus.Forfeited;
        EndedAt = DateTime.UtcNow;
    }

    public GameRound AddRound(Guid wordId, string scrambled)
    {
        var round = GameRound.Create(Id, wordId, scrambled);
        Rounds.Add(round);
        return round;
    }

    // Legacy method for backward compatibility
    public void LoseLife()
    {
        LivesRemaining--;
        if (LivesRemaining <= 0)
        {
            Fail();
        }
    }
}
