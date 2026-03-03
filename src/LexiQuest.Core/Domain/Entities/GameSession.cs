using LexiQuest.Core.Domain.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class GameSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public GameMode Mode { get; private set; }
    public Guid? PathId { get; private set; }
    public int CurrentLevel { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public int LivesRemaining { get; private set; }
    public int TotalXP { get; private set; }
    public List<GameRound> Rounds { get; private set; } = [];
    public GameSessionStatus Status { get; private set; }

    private GameSession() { }

    public static GameSession Create(Guid userId, GameMode mode, Guid? pathId = null, int currentLevel = 1)
    {
        return new GameSession
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Mode = mode,
            PathId = pathId,
            CurrentLevel = currentLevel,
            StartedAt = DateTime.UtcNow,
            LivesRemaining = 3,
            TotalXP = 0,
            Status = GameSessionStatus.Active,
            Rounds = []
        };
    }

    public GameRound AddRound(Guid wordId, string scrambled)
    {
        var round = GameRound.Create(Id, wordId, scrambled);
        Rounds.Add(round);
        return round;
    }

    public void Complete(int totalXP)
    {
        Status = GameSessionStatus.Completed;
        TotalXP = totalXP;
        EndedAt = DateTime.UtcNow;
    }

    public void Fail()
    {
        Status = GameSessionStatus.Failed;
        EndedAt = DateTime.UtcNow;
    }

    public void Abandon()
    {
        Status = GameSessionStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
    }

    public void LoseLife()
    {
        LivesRemaining--;
        if (LivesRemaining <= 0)
        {
            Fail();
        }
    }
}
