using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.DTOs.Achievements;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Services;

/// <summary>
/// Service for managing game sessions.
/// </summary>
public class GameSessionService : IGameSessionService
{
    private readonly LexiQuestDbContext _context;
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly ILevelCalculator _levelCalculator;
    private readonly IAchievementService? _achievementService;
    private readonly IAIChallengeService? _aiChallengeService;
    private readonly Random _rng = new();

    // Default game settings
    private const int DefaultRoundCount = 10;
    private const int DefaultTimeLimitSeconds = 30;
    private const int PathLevelCoinReward = 10;

    public GameSessionService(
        LexiQuestDbContext context,
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        ILevelCalculator? levelCalculator = null,
        IAchievementService? achievementService = null,
        IAIChallengeService? aiChallengeService = null)
    {
        _context = context;
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _levelCalculator = levelCalculator ?? new LevelCalculator();
        _achievementService = achievementService;
        _aiChallengeService = aiChallengeService;
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto> StartGameAsync(Guid userId, StartGameRequest request, CancellationToken cancellationToken = default)
    {
        // Determine difficulty
        var difficulty = request.Difficulty ?? DifficultyLevel.Beginner;
        var totalRounds = DefaultRoundCount;
        var timeLimitSeconds = DefaultTimeLimitSeconds;
        LearningPath? selectedPath = null;
        PathLevel? pathLevel = null;

        if (request.Mode == GameMode.Path)
        {
            if (request.CustomDictionaryId.HasValue)
            {
                throw new InvalidOperationException("Custom dictionary cannot be used with path mode");
            }

            if (!request.PathId.HasValue || !request.LevelNumber.HasValue)
            {
                throw new InvalidOperationException("Path and level are required for path mode");
            }

            selectedPath = await _context.LearningPaths
                .Include(p => p.Levels)
                .FirstOrDefaultAsync(p => p.Id == request.PathId.Value, cancellationToken);

            if (selectedPath == null)
            {
                throw new InvalidOperationException("Path not found");
            }

            pathLevel = selectedPath.Levels.FirstOrDefault(l => l.LevelNumber == request.LevelNumber.Value);
            if (pathLevel == null)
            {
                throw new InvalidOperationException("Path level not found");
            }

            var completedLevels = await _context.UserPathLevelProgresses
                .AsNoTracking()
                .CountAsync(
                    progress => progress.UserId == userId
                        && progress.PathId == selectedPath.Id
                        && (progress.Status == LevelStatus.Completed || progress.Status == LevelStatus.Perfect),
                    cancellationToken);

            var maxPlayableLevel = Math.Min(completedLevels + 1, selectedPath.TotalLevels);
            if (request.LevelNumber.Value > maxPlayableLevel)
            {
                throw new InvalidOperationException("Path level is locked");
            }

            difficulty = selectedPath.Difficulty;
            totalRounds = GetPathWordCount(pathLevel);
            timeLimitSeconds = GetPathTimeLimitSeconds(selectedPath, pathLevel);
        }

        List<Word> words;
        if (request.Mode == GameMode.AIChallenge)
        {
            if (request.CustomDictionaryId.HasValue)
            {
                throw new InvalidOperationException("Custom dictionary cannot be used with AI challenge mode");
            }

            if (_aiChallengeService is null)
            {
                throw new InvalidOperationException("AI challenge service is not available");
            }

            var challengeType = request.AiChallengeType ?? AIChallengeType.WeaknessFocus;
            var challenge = await _aiChallengeService.GenerateChallengeAsync(
                userId,
                new AIChallengeRequest(challengeType),
                cancellationToken);

            words = await ResolveChallengeWordsAsync(challenge, cancellationToken);
            totalRounds = Math.Min(DefaultRoundCount, words.Count);
            difficulty = request.Difficulty ?? GetDifficultyForAiChallenge(challenge);
            timeLimitSeconds = challengeType == AIChallengeType.SpeedTraining ? 20 : DefaultTimeLimitSeconds;
        }
        else
        {
            words = request.CustomDictionaryId.HasValue
                ? await GetCustomDictionaryWordsAsync(userId, request.CustomDictionaryId.Value, cancellationToken)
                : (await _wordRepository.GetRandomBatchAsync(totalRounds, difficulty, null, cancellationToken)).ToList();
        }

        if (words.Count == 0)
        {
            throw new InvalidOperationException("No words available for the selected difficulty");
        }

        if (request.CustomDictionaryId.HasValue)
        {
            totalRounds = Math.Min(DefaultRoundCount, words.Count);
        }

        var maxLives = pathLevel is null
            ? GetMaxLives(request.Mode, difficulty)
            : GetPathLives(difficulty, pathLevel);
        var isInfiniteLives = request.Mode == GameMode.Training;

        // Create game session
        var session = GameSession.Create(
            userId: userId,
            mode: request.Mode,
            difficulty: difficulty,
            totalRounds: totalRounds,
            lives: isInfiniteLives ? int.MaxValue : maxLives,
            pathId: request.Mode == GameMode.Path ? request.PathId : null,
            levelNumber: request.Mode == GameMode.Path ? request.LevelNumber : null
        );

        _context.GameSessions.Add(session);

        // Store word IDs in session for later use
        session.SetWordIds(words.Select(w => w.Id).ToList());

        // Create first round
        var firstWord = words[0];
        var scrambled = firstWord.Scramble(_rng);

        var round = GameRound.Create(
            sessionId: session.Id,
            roundNumber: 1,
            wordId: firstWord.Id,
            scrambledWord: scrambled,
            correctAnswer: firstWord.Original,
            timeLimitSeconds: timeLimitSeconds
        );

        _context.GameRounds.Add(round);
        await _context.SaveChangesAsync(cancellationToken);

        return new ScrambledWordDto(
            SessionId: session.Id,
            RoundNumber: 1,
            ScrambledWord: scrambled,
            WordLength: firstWord.Length,
            Difficulty: difficulty,
            TimeLimitSeconds: timeLimitSeconds,
            TotalRounds: totalRounds,
            LivesRemaining: session.LivesRemaining,
            MaxLives: maxLives,
            IsInfiniteLives: isInfiniteLives
        );
    }

    private async Task<List<Word>> GetCustomDictionaryWordsAsync(
        Guid userId,
        Guid dictionaryId,
        CancellationToken cancellationToken)
    {
        var dictionary = await _context.CustomDictionaries
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == dictionaryId, cancellationToken);

        if (dictionary == null || !dictionary.CanBeAccessedBy(userId))
        {
            throw new InvalidOperationException("Dictionary not found");
        }

        var words = await _context.DictionaryWords
            .AsNoTracking()
            .Where(word => word.DictionaryId == dictionaryId)
            .OrderBy(word => word.CreatedAt)
            .ToListAsync(cancellationToken);

        return words
            .Select((word, index) => Word.Create(word.Word, word.Difficulty, WordCategory.Everyday, index + 1))
            .OrderBy(_ => _rng.Next())
            .Take(DefaultRoundCount)
            .ToList();
    }

    private async Task<List<Word>> ResolveChallengeWordsAsync(
        AIChallengeDto challenge,
        CancellationToken cancellationToken)
    {
        var requestedWords = challenge.Words
            .Select(w => w.Word)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (requestedWords.Count == 0)
        {
            return [];
        }

        var requestedSet = requestedWords
            .Select(word => word.ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var dictionaryWords = await _wordRepository.GetAllAsync(cancellationToken);
        var resolved = dictionaryWords
            .Where(word => requestedSet.Contains(word.Normalized)
                || requestedSet.Contains(word.Original.ToLowerInvariant()))
            .OrderBy(word =>
            {
                var index = requestedWords.FindIndex(requested =>
                    string.Equals(requested, word.Original, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(requested, word.Normalized, StringComparison.OrdinalIgnoreCase));
                return index < 0 ? int.MaxValue : index;
            })
            .Take(DefaultRoundCount)
            .ToList();

        return resolved;
    }

    /// <inheritdoc />
    public async Task<GameRoundResult> SubmitAnswerAsync(Guid userId, SubmitAnswerRequest request, CancellationToken cancellationToken = default)
    {
        // Get session and validate ownership
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == userId, cancellationToken);

        if (session == null)
        {
            throw new InvalidOperationException("Game session not found");
        }

        if (session.Status != GameSessionStatus.InProgress)
        {
            throw new InvalidOperationException("Game session is not in progress");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        // Get current round
        var currentRound = await _context.GameRounds
            .Where(r => r.SessionId == session.Id && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRound == null)
        {
            throw new InvalidOperationException("No active round found");
        }

        // Validate answer
        var userAnswer = request.Answer?.Trim() ?? string.Empty;
        var isCorrect = userAnswer.Equals(currentRound.CorrectAnswer, StringComparison.OrdinalIgnoreCase);

        // Record the attempt
        currentRound.RecordAttempt(userAnswer, isCorrect, request.TimeSpentMs);
        user?.Stats.UpdateAccuracy(isCorrect);
        user?.Stats.UpdateAverageResponseTime(TimeSpan.FromMilliseconds(request.TimeSpentMs));

        int xpEarned = 0;
        int speedBonus = 0;

        if (isCorrect)
        {
            // Update session stats
            session.RecordCorrectAnswer();

            // Calculate XP
            var xpResult = _xpCalculator.CalculateCorrectAnswer(
                request.TimeSpentMs,
                session.ComboCount,
                session.CorrectAnswers
            );

            xpEarned = xpResult.TotalXP;
            speedBonus = xpResult.SpeedBonus;
            session.AddXP(xpEarned);
            var xpEvent = ApplyUserXp(user, xpEarned);
            var unlockedAchievements = await CheckWordSolvedAchievementsAsync(user, cancellationToken);

            // Check if game is complete
            bool isLevelComplete = session.CurrentRound >= session.TotalRounds;

            if (isLevelComplete)
            {
                await CompleteSessionAsync(session, user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new GameRoundResult(
                    IsCorrect: true,
                    CorrectAnswer: currentRound.CorrectAnswer,
                    XPEarned: xpEarned,
                    SpeedBonus: speedBonus,
                    ComboCount: session.ComboCount,
                    IsLevelComplete: true,
                    LivesRemaining: session.LivesRemaining,
                    NextScrambledWord: null,
                    NextRoundNumber: null,
                    IsGameOver: false,
                    XpEvent: xpEvent,
                    UnlockedAchievements: unlockedAchievements
                );
            }

            // Generate next round
            try
            {
                var nextRoundResult = await GenerateNextRoundAsync(session, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new GameRoundResult(
                    IsCorrect: true,
                    CorrectAnswer: currentRound.CorrectAnswer,
                    XPEarned: xpEarned,
                    SpeedBonus: speedBonus,
                    ComboCount: session.ComboCount,
                    IsLevelComplete: false,
                    LivesRemaining: session.LivesRemaining,
                    NextScrambledWord: nextRoundResult.ScrambledWord,
                    NextRoundNumber: nextRoundResult.RoundNumber,
                    IsGameOver: false,
                    XpEvent: xpEvent,
                    UnlockedAchievements: unlockedAchievements
                );
            }
            catch (ArgumentOutOfRangeException)
            {
                // Ran out of words - complete the game
                await CompleteSessionAsync(session, user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new GameRoundResult(
                    IsCorrect: true,
                    CorrectAnswer: currentRound.CorrectAnswer,
                    XPEarned: xpEarned,
                    SpeedBonus: speedBonus,
                    ComboCount: session.ComboCount,
                    IsLevelComplete: true,
                    LivesRemaining: session.LivesRemaining,
                    NextScrambledWord: null,
                    NextRoundNumber: null,
                    IsGameOver: false,
                    XpEvent: xpEvent,
                    UnlockedAchievements: unlockedAchievements
                );
            }
        }
        else
        {
            // Wrong answer
            var losesLife = session.Mode != GameMode.Training;
            session.RecordWrongAnswer(loseLife: losesLife);

            DateTime? nextLifeRegenAt = null;
            if (losesLife)
            {
                var maxLives = GetMaxLives(session.Mode, session.Difficulty);
                if (user != null)
                {
                    user.ResetLives(Math.Max(session.LivesRemaining, 0), maxLives);
                    if (user.LivesRemaining < user.MaxLives && user.NextLifeRegenAt == null)
                    {
                        user.ScheduleNextRegen(GetRegenMinutes(maxLives));
                    }

                    nextLifeRegenAt = user.NextLifeRegenAt;
                }
            }
            
            bool isGameOver = session.LivesRemaining <= 0;
            await _context.SaveChangesAsync(cancellationToken);

            return new GameRoundResult(
                IsCorrect: false,
                CorrectAnswer: currentRound.CorrectAnswer,
                XPEarned: 0,
                SpeedBonus: 0,
                ComboCount: 0, // Reset combo
                IsLevelComplete: false,
                LivesRemaining: session.LivesRemaining,
                NextScrambledWord: null,
                NextRoundNumber: null,
                IsGameOver: isGameOver,
                NextLifeRegenAt: nextLifeRegenAt
            );
        }
    }

    /// <inheritdoc />
    public async Task<ScrambledWordDto?> GetSessionStateAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null || session.Status != GameSessionStatus.InProgress)
        {
            return null;
        }

        var currentRound = await _context.GameRounds
            .Where(r => r.SessionId == session.Id && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRound == null)
        {
            return null;
        }

        var isInfiniteLives = session.Mode == GameMode.Training;
        var maxLives = GetMaxLives(session.Mode, session.Difficulty);
        var nextLifeRegenAt = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.NextLifeRegenAt)
            .FirstOrDefaultAsync(cancellationToken);

        return new ScrambledWordDto(
            SessionId: session.Id,
            RoundNumber: currentRound.RoundNumber,
            ScrambledWord: currentRound.ScrambledWord,
            WordLength: currentRound.CorrectAnswer.Length,
            Difficulty: session.Difficulty,
            TimeLimitSeconds: currentRound.TimeLimitSeconds,
            TotalRounds: session.TotalRounds,
            LivesRemaining: session.LivesRemaining,
            MaxLives: maxLives,
            IsInfiniteLives: isInfiniteLives,
            NextLifeRegenAt: session.LivesRemaining < maxLives ? nextLifeRegenAt : null
        );
    }

    /// <inheritdoc />
    public async Task<OfflineTrainingSeedResponse?> GetOfflineTrainingSeedAsync(
        Guid userId,
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(
                s => s.Id == sessionId
                    && s.UserId == userId
                    && s.Mode == GameMode.Training
                    && s.Status == GameSessionStatus.InProgress,
                cancellationToken);

        if (session == null)
        {
            return null;
        }

        var activeRound = await _context.GameRounds
            .Where(r => r.SessionId == session.Id && !r.IsCompleted)
            .OrderByDescending(r => r.RoundNumber)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeRound == null)
        {
            return null;
        }

        return new OfflineTrainingSeedResponse(
            SessionId: session.Id,
            CurrentRound: activeRound.RoundNumber,
            TotalRounds: session.TotalRounds,
            LivesRemaining: session.LivesRemaining,
            Difficulty: session.Difficulty,
            Words:
            [
                new OfflineTrainingWordDto(
                    RoundNumber: activeRound.RoundNumber,
                    ScrambledWord: activeRound.ScrambledWord,
                    CorrectAnswer: activeRound.CorrectAnswer,
                    WordLength: activeRound.CorrectAnswer.Length,
                    TimeLimitSeconds: activeRound.TimeLimitSeconds)
            ],
            MaxLives: GetMaxLives(session.Mode, session.Difficulty),
            IsInfiniteLives: session.Mode == GameMode.Training);
    }

    /// <inheritdoc />
    public async Task<bool> ForfeitGameAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _context.GameSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId, cancellationToken);

        if (session == null || session.Status != GameSessionStatus.InProgress)
        {
            return false;
        }

        session.Forfeit();
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task<GameRound> GenerateNextRoundAsync(GameSession session, CancellationToken cancellationToken)
    {
        var nextRoundNumber = session.CurrentRound + 1;

        Word? word;

        try
        {
            var wordId = session.GetWordIdForRound(nextRoundNumber);
            word = await _wordRepository.GetByIdAsync(wordId, cancellationToken);
        }
        catch (ArgumentOutOfRangeException)
        {
            word = await _wordRepository.GetRandomAsync(session.Difficulty, null, cancellationToken);
        }

        if (word == null)
        {
            throw new InvalidOperationException($"Word not found for round {nextRoundNumber}");
        }

        var scrambled = word.Scramble(_rng);

        var round = GameRound.Create(
            sessionId: session.Id,
            roundNumber: nextRoundNumber,
            wordId: word.Id,
            scrambledWord: scrambled,
            correctAnswer: word.Original,
            timeLimitSeconds: GetRoundTimeLimitSeconds(session)
        );

        _context.GameRounds.Add(round);
        session.AdvanceToNextRound();

        return round;
    }

    private XPGainedEvent? ApplyUserXp(User? user, int amount)
    {
        if (user == null || amount <= 0)
        {
            return null;
        }

        var previousXp = user.Stats.TotalXP;
        var previousLevel = _levelCalculator.GetLevelFromXp(previousXp);

        user.Stats.AddXP(amount);

        var totalXp = user.Stats.TotalXP;
        var newLevel = _levelCalculator.GetLevelFromXp(totalXp);
        var hasLeveledUp = newLevel > previousLevel;

        if (hasLeveledUp)
        {
            typeof(UserStats)
                .GetProperty(nameof(UserStats.Level))
                ?.SetValue(user.Stats, newLevel);
        }

        return new XPGainedEvent(
            Amount: amount,
            Source: XpSource.Game.ToString(),
            LeveledUp: hasLeveledUp,
            NewLevel: newLevel,
            TotalXP: totalXp,
            Unlocks: hasLeveledUp ? GetUnlocksForLevels(previousLevel + 1, newLevel) : null
        );
    }

    private async Task<List<AchievementUnlockDto>?> CheckWordSolvedAchievementsAsync(
        User? user,
        CancellationToken cancellationToken)
    {
        if (user == null || _achievementService == null)
        {
            return null;
        }

        var unlocked = await _achievementService.CheckWordSolvedAsync(
            user.Id,
            user.Stats.TotalWordsSolved,
            cancellationToken);

        return unlocked.Count == 0
            ? null
            : unlocked
                .Select(achievement => new AchievementUnlockDto(
                    AchievementId: achievement.AchievementId,
                    Name: achievement.Name,
                    Description: achievement.Description,
                    XPReward: achievement.XPEarned,
                    IconName: achievement.IconName))
                .ToList();
    }

    private static List<UnlockableReward>? GetUnlocksForLevels(int firstLevel, int lastLevel)
    {
        var unlocks = new List<UnlockableReward>();

        for (var level = firstLevel; level <= lastLevel; level++)
        {
            unlocks.AddRange(GetUnlocksForLevel(level));
        }

        return unlocks.Count > 0 ? unlocks : null;
    }

    private static List<UnlockableReward> GetUnlocksForLevel(int level)
    {
        return level switch
        {
            3 =>
            [
                new UnlockableReward(
                    Type: "Path",
                    Name: "Intermediate",
                    Description: "Cesta pro pokročilé - 5-7 písmen")
            ],
            5 =>
            [
                new UnlockableReward(
                    Type: "Feature",
                    Name: "Leagues",
                    Description: "Žebříčky a ligy - soutěžte s ostatními hráči")
            ],
            7 =>
            [
                new UnlockableReward(
                    Type: "Path",
                    Name: "Advanced",
                    Description: "Pokročilá cesta - 7-10 písmen")
            ],
            10 =>
            [
                new UnlockableReward(
                    Type: "Path",
                    Name: "Expert",
                    Description: "Expertní cesta - 10+ písmen")
            ],
            15 =>
            [
                new UnlockableReward(
                    Type: "Feature",
                    Name: "Multiplayer",
                    Description: "Hrajte proti ostatním hráčům v reálném čase")
            ],
            _ => []
        };
    }

    private static int GetMaxLives(GameMode mode, DifficultyLevel difficulty)
    {
        if (mode == GameMode.Training)
        {
            return int.MaxValue;
        }

        if (mode == GameMode.Path)
        {
            return difficulty switch
            {
                DifficultyLevel.Beginner => 5,
                DifficultyLevel.Intermediate => 4,
                DifficultyLevel.Advanced => 3,
                DifficultyLevel.Expert => 3,
                _ => 5
            };
        }

        return difficulty switch
        {
            DifficultyLevel.Beginner => 5,
            DifficultyLevel.Intermediate => 4,
            DifficultyLevel.Advanced => 3,
            DifficultyLevel.Expert => 3,
            _ => 5
        };
    }

    private static DifficultyLevel GetDifficultyForAiChallenge(AIChallengeDto challenge)
    {
        return challenge.PredictedDifficulty switch
        {
            >= 0.75 => DifficultyLevel.Expert,
            >= 0.55 => DifficultyLevel.Advanced,
            >= 0.35 => DifficultyLevel.Intermediate,
            _ => DifficultyLevel.Beginner
        };
    }

    private static int GetPathWordCount(PathLevel level) => level.IsBoss ? 20 : 10;

    private static int GetPathTimeLimitSeconds(LearningPath path, PathLevel level) =>
        level.IsBoss ? Math.Min(path.TimePerWord, 15) : path.TimePerWord;

    private static int GetPathLives(DifficultyLevel difficulty, PathLevel level)
    {
        if (level.IsBoss)
        {
            return 3;
        }

        return GetMaxLives(GameMode.Path, difficulty);
    }

    private async Task CompletePathLevelAsync(GameSession session, User? user, CancellationToken cancellationToken)
    {
        if (session.Mode != GameMode.Path || !session.PathId.HasValue || !session.LevelNumber.HasValue)
        {
            return;
        }

        var pathLevel = await _context.PathLevels
            .FirstOrDefaultAsync(
                level => level.PathId == session.PathId.Value
                    && level.LevelNumber == session.LevelNumber.Value,
                cancellationToken);

        if (pathLevel == null)
        {
            return;
        }

        var expectedLives = GetPathLives(session.Difficulty, pathLevel);
        var isPerfect = session.CorrectAnswers >= session.TotalRounds
            && session.LivesRemaining >= expectedLives;

        var existingProgress = await _context.UserPathLevelProgresses
            .FirstOrDefaultAsync(
                progress => progress.UserId == session.UserId
                    && progress.PathId == session.PathId.Value
                    && progress.LevelNumber == session.LevelNumber.Value,
                cancellationToken);

        var shouldAwardCoins = user != null
            && (existingProgress == null || existingProgress.Status is not (LevelStatus.Completed or LevelStatus.Perfect));

        if (existingProgress == null)
        {
            _context.UserPathLevelProgresses.Add(UserPathLevelProgress.Complete(
                session.UserId,
                session.PathId.Value,
                pathLevel.Id,
                session.LevelNumber.Value,
                isPerfect));
        }
        else
        {
            existingProgress.MarkCompleted(isPerfect);
        }

        if (shouldAwardCoins)
        {
            user!.AddCoinTransaction(
                PathLevelCoinReward,
                CoinTransactionType.LevelComplete.ToString(),
                $"Dokončení levelu {session.LevelNumber.Value}");
        }
    }

    private async Task CompleteSessionAsync(GameSession session, User? user, CancellationToken cancellationToken)
    {
        session.Complete();
        await RecordCompletionStreakAsync(user, cancellationToken);
        await CompletePathLevelAsync(session, user, cancellationToken);
    }

    private async Task RecordCompletionStreakAsync(User? user, CancellationToken cancellationToken)
    {
        if (user == null)
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (!WouldBreakStreak(user.Streak, now))
        {
            user.Streak.RecordActivity(now);
            return;
        }

        var protection = await _context.StreakProtections
            .FirstOrDefaultAsync(item => item.UserId == user.Id, cancellationToken);

        if (protection?.IsShieldActive == true)
        {
            protection.DeactivateShield();
            user.Streak.RecordProtectedActivity(now);
            return;
        }

        if (user.Premium.IsActive(now))
        {
            if (protection == null)
            {
                protection = StreakProtection.Create(user.Id);
                _context.StreakProtections.Add(protection);
            }

            if (protection.CanUseFreeze())
            {
                protection.UseFreeze();
                user.Streak.RecordProtectedActivity(now);
                return;
            }
        }

        user.Streak.RecordActivity(now);
    }

    private static bool WouldBreakStreak(Streak streak, DateTime now)
    {
        return streak.CurrentDays > 0
            && streak.LastActivityDate.HasValue
            && streak.LastActivityDate.Value.Date != now.Date
            && now - streak.LastActivityDate.Value > TimeSpan.FromHours(48);
    }

    private static int GetRoundTimeLimitSeconds(GameSession session)
    {
        if (session.Mode != GameMode.Path)
        {
            return DefaultTimeLimitSeconds;
        }

        return session.Difficulty switch
        {
            _ when session.LevelNumber.HasValue && session.LevelNumber.Value % 5 == 0 => 15,
            DifficultyLevel.Beginner => DefaultTimeLimitSeconds,
            DifficultyLevel.Intermediate => 25,
            DifficultyLevel.Advanced => 20,
            DifficultyLevel.Expert => 18,
            _ => DefaultTimeLimitSeconds
        };
    }

    private static int GetRegenMinutes(int maxLives) => maxLives switch
    {
        3 => 60,
        4 => 30,
        5 => 20,
        _ => 30
    };
}
