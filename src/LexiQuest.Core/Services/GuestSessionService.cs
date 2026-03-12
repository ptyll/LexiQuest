using System.Collections.Concurrent;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Services;

/// <summary>
/// Result of starting a guest game session.
/// </summary>
public class GuestSessionResult
{
    public Guid SessionId { get; set; }
    public List<ScrambledWordInfo> ScrambledWords { get; set; } = new();
    public bool IsGuest { get; set; } = true;
    public DateTime StartedAt { get; set; }
}

/// <summary>
/// Information about a scrambled word in guest session.
/// </summary>
public class ScrambledWordInfo
{
    public Guid WordId { get; set; }
    public string Scrambled { get; set; } = null!;
    public string Original { get; set; } = null!;
    public int Length { get; set; }
    public bool IsSolved { get; set; }
    public int? XpEarned { get; set; }
}

/// <summary>
/// Result of submitting an answer in guest session.
/// </summary>
public class GuestAnswerResult
{
    public bool IsCorrect { get; set; }
    public int XpEarned { get; set; }
    public string CorrectAnswer { get; set; } = null!;
    public string? UserAnswer { get; set; }
    public int TotalSessionXp { get; set; }
    public int WordsSolved { get; set; }
    public int WordsRemaining { get; set; }
}

/// <summary>
/// Progress information for guest session.
/// </summary>
public class GuestProgressResult
{
    public int TotalXp { get; set; }
    public int WordsSolved { get; set; }
    public int TotalWords { get; set; }
    public int WordsRemaining => TotalWords - WordsSolved;
    public bool IsComplete => WordsSolved >= TotalWords;
}

/// <summary>
/// Internal state of a guest game session.
/// </summary>
internal class GuestSession
{
    public Guid SessionId { get; set; }
    public List<ScrambledWordInfo> Words { get; set; } = new();
    public DateTime StartedAt { get; set; }
    public DateTime LastActivityAt { get; set; }

    public int TotalXp => Words.Where(w => w.IsSolved).Sum(w => w.XpEarned ?? 0);
    public int WordsSolved => Words.Count(w => w.IsSolved);
}

/// <summary>
/// Service for managing guest game sessions without user registration.
/// All data is stored in-memory only and lost after session ends.
/// Uses only beginner words (Easy difficulty) for guest sessions.
/// </summary>
public class GuestSessionService : IGuestSessionService
{
    private readonly IWordRepository _wordRepository;
    private readonly ConcurrentDictionary<Guid, GuestSession> _sessions = new();
    private readonly Random _random = new();

    // XP calculation constants for guest mode
    private const int BaseXpPerWord = 10;
    private const int StreakBonusPerWord = 2;

    public GuestSessionService(IWordRepository wordRepository)
    {
        _wordRepository = wordRepository;
    }

    /// <summary>
    /// Starts a new guest game session with 5 beginner words.
    /// </summary>
    public GuestSessionResult StartGame()
    {
        // Get 5 random beginner words
        var words = _wordRepository.GetRandomBatchAsync(5, DifficultyLevel.Beginner, null).Result;

        if (words.Count < 5)
        {
            // Fallback: create default words if not enough in database
            words = GetDefaultBeginnerWords();
        }

        var session = new GuestSession
        {
            SessionId = Guid.NewGuid(),
            StartedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow
        };

        foreach (var word in words.Take(5))
        {
            session.Words.Add(new ScrambledWordInfo
            {
                WordId = word.Id,
                Original = word.Original,
                Scrambled = word.Scramble(_random),
                Length = word.Length,
                IsSolved = false
            });
        }

        _sessions[session.SessionId] = session;

        return new GuestSessionResult
        {
            SessionId = session.SessionId,
            ScrambledWords = session.Words,
            IsGuest = true,
            StartedAt = session.StartedAt
        };
    }

    /// <summary>
    /// Submits an answer for a word in the guest session.
    /// </summary>
    public GuestAnswerResult SubmitAnswer(Guid sessionId, Guid wordId, string answer)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            throw new InvalidOperationException("Session not found or expired.");
        }

        session.LastActivityAt = DateTime.UtcNow;

        var word = session.Words.FirstOrDefault(w => w.WordId == wordId);
        if (word == null)
        {
            throw new InvalidOperationException("Word not found in session.");
        }

        // Normalize answer (case-insensitive)
        var normalizedAnswer = answer?.Trim().ToLowerInvariant() ?? "";
        var normalizedOriginal = word.Original.ToLowerInvariant();
        var isCorrect = normalizedAnswer == normalizedOriginal;

        int xpEarned = 0;

        if (isCorrect && !word.IsSolved)
        {
            // Calculate XP
            xpEarned = CalculateXp(word.Length);
            word.IsSolved = true;
            word.XpEarned = xpEarned;
        }

        return new GuestAnswerResult
        {
            IsCorrect = isCorrect,
            XpEarned = xpEarned,
            CorrectAnswer = word.Original,
            UserAnswer = answer,
            TotalSessionXp = session.TotalXp,
            WordsSolved = session.WordsSolved,
            WordsRemaining = session.Words.Count - session.WordsSolved
        };
    }

    /// <summary>
    /// Gets current progress of the guest session.
    /// </summary>
    public GuestSessionProgress GetSessionProgress(Guid sessionId)
    {
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            throw new InvalidOperationException("Session not found or expired.");
        }

        return new GuestSessionProgress(
            TotalXp: session.TotalXp,
            WordsSolved: session.WordsSolved
        );
    }

    /// <summary>
    /// Ends the guest session and returns final stats.
    /// </summary>
    public GuestSessionResult EndGame(Guid sessionId)
    {
        if (!_sessions.TryRemove(sessionId, out var session))
        {
            throw new InvalidOperationException("Session not found or expired.");
        }

        return new GuestSessionResult
        {
            SessionId = session.SessionId,
            ScrambledWords = session.Words,
            IsGuest = true,
            StartedAt = session.StartedAt
        };
    }

    /// <summary>
    /// Calculates XP based on word length and difficulty.
    /// </summary>
    private int CalculateXp(int wordLength)
    {
        // Base XP + length bonus
        return BaseXpPerWord + (wordLength * 2);
    }

    /// <summary>
    /// Default beginner words if database doesn't have enough.
    /// </summary>
    private List<Word> GetDefaultBeginnerWords()
    {
        return new List<Word>
        {
            Word.Create("pes", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("kočka", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("dům", DifficultyLevel.Beginner, WordCategory.Everyday),
            Word.Create("strom", DifficultyLevel.Beginner, WordCategory.Nature),
            Word.Create("kniha", DifficultyLevel.Beginner, WordCategory.Everyday),
            Word.Create("auto", DifficultyLevel.Beginner, WordCategory.Everyday),
            Word.Create("škola", DifficultyLevel.Beginner, WordCategory.Geography),
            Word.Create("jablko", DifficultyLevel.Beginner, WordCategory.Food),
            Word.Create("voda", DifficultyLevel.Beginner, WordCategory.Nature),
            Word.Create("slunce", DifficultyLevel.Beginner, WordCategory.Nature)
        };
    }
}
