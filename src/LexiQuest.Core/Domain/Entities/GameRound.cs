namespace LexiQuest.Core.Domain.Entities;

public class GameRound
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public Guid WordId { get; private set; }
    public int RoundNumber { get; private set; }
    public string ScrambledWord { get; private set; } = null!;
    public string CorrectAnswer { get; private set; } = null!;
    public int TimeLimitSeconds { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? AnsweredAt { get; private set; }
    public string? UserAnswer { get; private set; }
    public bool IsCorrect { get; private set; }
    public bool IsCompleted { get; private set; }
    public int XPEarned { get; private set; }
    public int TimeSpentMs { get; private set; }

    // Boss Level properties
    public string? ForbiddenLetters { get; private set; }
    public int RevealedLettersCount { get; private set; }
    public string? RevealedPositions { get; private set; }

    private GameRound() { }

    public static GameRound Create(
        Guid sessionId,
        int roundNumber,
        Guid wordId,
        string scrambledWord,
        string correctAnswer,
        int timeLimitSeconds)
    {
        return new GameRound
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            RoundNumber = roundNumber,
            WordId = wordId,
            ScrambledWord = scrambledWord,
            CorrectAnswer = correctAnswer,
            TimeLimitSeconds = timeLimitSeconds,
            StartedAt = DateTime.UtcNow,
            IsCorrect = false,
            IsCompleted = false,
            XPEarned = 0
        };
    }

    // Legacy method for backward compatibility
    internal static GameRound Create(Guid gameSessionId, Guid wordId, string scrambled)
    {
        return new GameRound
        {
            Id = Guid.NewGuid(),
            SessionId = gameSessionId,
            RoundNumber = 0,
            WordId = wordId,
            ScrambledWord = scrambled,
            CorrectAnswer = string.Empty,
            TimeLimitSeconds = 30,
            StartedAt = DateTime.UtcNow,
            IsCorrect = false,
            IsCompleted = false,
            XPEarned = 0
        };
    }

    public void RecordAttempt(string answer, bool isCorrect, int timeSpentMs)
    {
        UserAnswer = answer;
        IsCorrect = isCorrect;
        TimeSpentMs = timeSpentMs;
        IsCompleted = true;
        AnsweredAt = DateTime.UtcNow;
    }

    public void SubmitAnswer(string answer, bool isCorrect, int xpEarned)
    {
        UserAnswer = answer;
        IsCorrect = isCorrect;
        XPEarned = xpEarned;
        IsCompleted = true;
        AnsweredAt = DateTime.UtcNow;
    }

    public void SetXPEarned(int xp)
    {
        XPEarned = xp;
    }

    public void SetForbiddenLetters(string letters)
    {
        ForbiddenLetters = letters;
    }

    public void SetRevealedPositions(int[] positions)
    {
        RevealedPositions = string.Join(",", positions);
        RevealedLettersCount = positions.Length;
    }

    public bool ContainsForbiddenLetter(string answer)
    {
        if (string.IsNullOrEmpty(ForbiddenLetters) || string.IsNullOrEmpty(answer))
            return false;

        return ForbiddenLetters.Any(forbidden => 
            answer.Contains(forbidden, StringComparison.OrdinalIgnoreCase));
    }

    public void RevealLetter()
    {
        RevealedLettersCount++;
    }
}
