namespace LexiQuest.Core.Domain.Entities;

public class GameRound
{
    public Guid Id { get; private set; }
    public Guid GameSessionId { get; private set; }
    public Guid WordId { get; private set; }
    public string Scrambled { get; private set; } = null!;
    public DateTime? StartedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public string? UserAnswer { get; private set; }
    public bool IsCorrect { get; private set; }
    public int XPEarned { get; private set; }

    private GameRound() { }

    internal static GameRound Create(Guid gameSessionId, Guid wordId, string scrambled)
    {
        return new GameRound
        {
            Id = Guid.NewGuid(),
            GameSessionId = gameSessionId,
            WordId = wordId,
            Scrambled = scrambled,
            StartedAt = DateTime.UtcNow,
            IsCorrect = false,
            XPEarned = 0
        };
    }

    public void SubmitAnswer(string answer, bool isCorrect, int xpEarned)
    {
        UserAnswer = answer;
        IsCorrect = isCorrect;
        XPEarned = xpEarned;
        AnsweredAt = DateTime.UtcNow;
    }
}
