using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class AIChallengeService : IAIChallengeService
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IWordRepository _wordRepository;
    private readonly IStringLocalizer<AIChallengeService> _localizer;

    public AIChallengeService(
        IGameSessionRepository gameSessionRepository,
        IWordRepository wordRepository,
        IStringLocalizer<AIChallengeService> localizer)
    {
        _gameSessionRepository = gameSessionRepository;
        _wordRepository = wordRepository;
        _localizer = localizer;
    }

    public async Task<PlayerAnalysisDto> AnalyzePlayerAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _gameSessionRepository.GetByUserIdWithRoundsAsync(
            userId, 50, cancellationToken);

        var allRounds = sessions.SelectMany(s => s.Rounds)
            .Where(r => r.IsCompleted)
            .ToList();

        var weakLetters = IdentifyWeakLetters(allRounds);
        var categoryPerformance = AnalyzeCategoryPerformance(sessions, allRounds);
        var tips = GenerateTips(weakLetters, categoryPerformance);

        return new PlayerAnalysisDto(weakLetters, categoryPerformance, tips);
    }

    public async Task<AIChallengeDto> GenerateChallengeAsync(
        Guid userId, AIChallengeRequest request, CancellationToken cancellationToken = default)
    {
        var sessions = await _gameSessionRepository.GetByUserIdWithRoundsAsync(
            userId, 50, cancellationToken);

        var allRounds = sessions.SelectMany(s => s.Rounds)
            .Where(r => r.IsCompleted)
            .ToList();

        var words = request.Type switch
        {
            AIChallengeType.WeaknessFocus => await GenerateWeaknessFocusAsync(allRounds, cancellationToken),
            AIChallengeType.SpeedTraining => await GenerateSpeedTrainingAsync(cancellationToken),
            AIChallengeType.MemoryGame => await GenerateMemoryGameAsync(allRounds, cancellationToken),
            AIChallengeType.PatternRecognition => await GeneratePatternRecognitionAsync(allRounds, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request))
        };

        var (title, description) = GetChallengeMetadata(request.Type);
        var predictedDifficulty = PredictDifficulty(allRounds, words);

        return new AIChallengeDto(request.Type, title, description, words, predictedDifficulty);
    }

    internal List<WeakLetterDto> IdentifyWeakLetters(List<GameRound> rounds)
    {
        if (rounds.Count == 0)
            return [];

        var letterStats = new Dictionary<char, (int errors, int total)>();

        foreach (var round in rounds)
        {
            if (string.IsNullOrEmpty(round.CorrectAnswer))
                continue;

            foreach (var letter in round.CorrectAnswer.ToUpperInvariant().Distinct())
            {
                if (!char.IsLetter(letter))
                    continue;

                if (!letterStats.ContainsKey(letter))
                    letterStats[letter] = (0, 0);

                var (errors, total) = letterStats[letter];
                letterStats[letter] = (errors + (round.IsCorrect ? 0 : 1), total + 1);
            }
        }

        return letterStats
            .Where(kv => kv.Value.total >= 3)
            .Select(kv => new WeakLetterDto(
                kv.Key,
                Math.Round((double)kv.Value.errors / kv.Value.total, 2)))
            .Where(w => w.ErrorRate > 0.3)
            .OrderByDescending(w => w.ErrorRate)
            .Take(5)
            .ToList();
    }

    internal List<CategoryPerformanceDto> AnalyzeCategoryPerformance(
        IReadOnlyList<GameSession> sessions, List<GameRound> rounds)
    {
        if (rounds.Count == 0)
            return [];

        // Group rounds by session difficulty as proxy for category
        var sessionMap = sessions.ToDictionary(s => s.Id, s => s);

        var categoryGroups = rounds
            .Where(r => sessionMap.ContainsKey(r.SessionId))
            .GroupBy(r => GetDifficultyLabel(sessionMap[r.SessionId].Difficulty))
            .Select(g => new CategoryPerformanceDto(
                g.Key,
                Math.Round((double)g.Count(r => r.IsCorrect) / g.Count(), 2),
                Math.Round(g.Average(r => r.TimeSpentMs) / 1000.0, 1)))
            .ToList();

        return categoryGroups;
    }

    private List<string> GenerateTips(
        List<WeakLetterDto> weakLetters,
        List<CategoryPerformanceDto> categoryPerformance)
    {
        var tips = new List<string>();

        if (weakLetters.Count == 0 && categoryPerformance.Count == 0)
        {
            tips.Add(_localizer["Tip.NoData"]);
            return tips;
        }

        if (weakLetters.Count > 0)
        {
            var letters = string.Join(", ", weakLetters.Take(3).Select(w => w.Letter));
            tips.Add(_localizer["Tip.WeakLetters", letters]);
        }

        var slowCategories = categoryPerformance
            .Where(c => c.AvgTimeSeconds > 10)
            .OrderByDescending(c => c.AvgTimeSeconds)
            .Take(2)
            .ToList();

        if (slowCategories.Count > 0)
        {
            tips.Add(_localizer["Tip.SlowCategory", slowCategories.First().Category]);
        }

        var weakCategories = categoryPerformance
            .Where(c => c.SuccessRate < 0.6)
            .OrderBy(c => c.SuccessRate)
            .Take(2)
            .ToList();

        if (weakCategories.Count > 0)
        {
            tips.Add(_localizer["Tip.WeakCategory", weakCategories.First().Category]);
        }

        if (tips.Count == 0)
        {
            tips.Add(_localizer["Tip.NoWeakness"]);
        }

        return tips;
    }

    private async Task<List<AIChallengeWordDto>> GenerateWeaknessFocusAsync(
        List<GameRound> rounds, CancellationToken cancellationToken)
    {
        var weakLetters = IdentifyWeakLetters(rounds);

        if (weakLetters.Count == 0)
        {
            var fallbackWords = await GetOrderedWordsAsync(cancellationToken);
            return fallbackWords.Take(10).Select(w => new AIChallengeWordDto(
                w.Original, PredictWordDifficulty(w), _localizer["Reason.GeneralPractice"].Value)).ToList();
        }

        var weakLetterChars = weakLetters.Select(w => w.Letter).ToHashSet();

        var allWords = await GetOrderedWordsAsync(cancellationToken);
        var matchingWords = allWords
            .Where(w => w.Original.ToUpperInvariant().Any(c => weakLetterChars.Contains(c)))
            .Take(10)
            .Select(w =>
            {
                var matchedLetters = w.Original.ToUpperInvariant()
                    .Where(c => weakLetterChars.Contains(c))
                    .Distinct();
                var reason = _localizer["Reason.WeakLetters", string.Join(", ", matchedLetters)].Value;
                return new AIChallengeWordDto(w.Original, PredictWordDifficulty(w), reason);
            })
            .ToList();

        return matchingWords.Count > 0
            ? matchingWords
            : allWords.Take(10).Select(w => new AIChallengeWordDto(
                w.Original, PredictWordDifficulty(w), _localizer["Reason.GeneralPractice"].Value)).ToList();
    }

    private async Task<List<AIChallengeWordDto>> GenerateSpeedTrainingAsync(
        CancellationToken cancellationToken)
    {
        var allWords = await GetOrderedWordsAsync(cancellationToken);
        var shortWords = allWords
            .Where(w => w.Length <= 5)
            .OrderBy(w => w.Length)
            .ThenBy(w => w.FrequencyRank)
            .ThenBy(w => w.Original)
            .Take(10)
            .Select(w => new AIChallengeWordDto(
                w.Original,
                PredictWordDifficulty(w),
                _localizer["Reason.SpeedTraining"].Value))
            .ToList();

        // If no short words at all, take the shortest available
        if (shortWords.Count == 0)
        {
            var shortestWords = allWords
                .OrderBy(w => w.Length)
                .ThenBy(w => w.FrequencyRank)
                .Take(10)
                .Select(w => new AIChallengeWordDto(
                    w.Original,
                    PredictWordDifficulty(w),
                    _localizer["Reason.SpeedTraining"].Value))
                .ToList();
            return shortestWords;
        }

        return shortWords;
    }

    private async Task<List<AIChallengeWordDto>> GenerateMemoryGameAsync(
        List<GameRound> rounds, CancellationToken cancellationToken)
    {
        // Find words the user got wrong before
        var incorrectWords = rounds
            .Where(r => !r.IsCorrect && !string.IsNullOrEmpty(r.CorrectAnswer))
            .Select(r => r.CorrectAnswer)
            .Distinct()
            .Take(10)
            .ToList();

        if (incorrectWords.Count == 0)
        {
            var fallbackWords = await GetOrderedWordsAsync(cancellationToken);
            return fallbackWords.Take(10).Select(w => new AIChallengeWordDto(
                w.Original, PredictWordDifficulty(w), _localizer["Reason.GeneralPractice"].Value)).ToList();
        }

        var existingWords = (await GetOrderedWordsAsync(cancellationToken))
            .ToDictionary(w => w.Normalized, StringComparer.OrdinalIgnoreCase);

        var words = new List<AIChallengeWordDto>();
        foreach (var wordText in incorrectWords)
        {
            existingWords.TryGetValue(wordText.ToLowerInvariant(), out var word);
            words.Add(new AIChallengeWordDto(
                word?.Original ?? wordText,
                word is null ? 0.7 : PredictWordDifficulty(word),
                _localizer["Reason.PreviousMistake"].Value));
        }

        return words;
    }

    private async Task<List<AIChallengeWordDto>> GeneratePatternRecognitionAsync(
        List<GameRound> rounds, CancellationToken cancellationToken)
    {
        // Find words the user struggled with and select similar-length words
        var difficultLengths = rounds
            .Where(r => !r.IsCorrect && !string.IsNullOrEmpty(r.CorrectAnswer))
            .Select(r => r.CorrectAnswer.Length)
            .GroupBy(l => l)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => g.Key)
            .ToHashSet();

        if (difficultLengths.Count == 0)
        {
            difficultLengths = new HashSet<int> { 5, 6, 7 };
        }

        var allWords = await GetOrderedWordsAsync(cancellationToken);
        var patternWords = allWords
            .Where(w => difficultLengths.Contains(w.Length))
            .OrderBy(w => w.Length)
            .ThenBy(w => w.FrequencyRank)
            .ThenBy(w => w.Original)
            .Take(10)
            .Select(w => new AIChallengeWordDto(
                w.Original,
                PredictWordDifficulty(w),
                _localizer["Reason.Pattern", w.Length]))
            .ToList();

        return patternWords.Count > 0
            ? patternWords
            : allWords.Take(10).Select(w => new AIChallengeWordDto(
                w.Original, PredictWordDifficulty(w), _localizer["Reason.GeneralPractice"].Value)).ToList();
    }

    internal double PredictDifficulty(List<GameRound> playerRounds, List<AIChallengeWordDto> challengeWords)
    {
        if (challengeWords.Count == 0)
            return 0.5;

        var avgWordDifficulty = challengeWords.Average(w => w.PredictedDifficulty);

        if (playerRounds.Count == 0)
            return Math.Clamp(avgWordDifficulty, 0.0, 1.0);

        var playerSuccessRate = (double)playerRounds.Count(r => r.IsCorrect) / playerRounds.Count;

        // Combine word difficulty with player performance
        // Higher success rate = lower predicted difficulty for the player
        var adjustedDifficulty = avgWordDifficulty * (1.0 - playerSuccessRate * 0.3);

        return Math.Clamp(Math.Round(adjustedDifficulty, 2), 0.0, 1.0);
    }

    private static double PredictWordDifficulty(Word word)
    {
        // Simple difficulty model based on word length and difficulty level
        var lengthFactor = Math.Min(word.Length / 15.0, 1.0);
        var difficultyFactor = (int)word.Difficulty / 4.0;

        return Math.Clamp(Math.Round((lengthFactor + difficultyFactor) / 2.0, 2), 0.0, 1.0);
    }

    private static (string title, string description) GetChallengeMetadata(AIChallengeType type)
    {
        return type switch
        {
            AIChallengeType.WeaknessFocus => (
                "Zaměření na slabiny",
                "Slova vybraná podle písmen, která vám dělají největší potíže."),
            AIChallengeType.SpeedTraining => (
                "Rychlostní trénink",
                "Krátká slova pro zrychlení reakcí."),
            AIChallengeType.MemoryGame => (
                "Paměťová hra",
                "Slova, která jste dříve zodpověděli špatně."),
            AIChallengeType.PatternRecognition => (
                "Rozpoznávání vzorů",
                "Slova s podobnou délkou nebo vzorem jako ta, se kterými jste bojovali."),
            _ => ("AI výzva", "Personalizovaná výzva připravená na míru.")
        };
    }

    private async Task<IReadOnlyList<Word>> GetOrderedWordsAsync(CancellationToken cancellationToken)
    {
        return (await _wordRepository.GetAllAsync(cancellationToken))
            .OrderBy(w => w.FrequencyRank)
            .ThenBy(w => w.Original)
            .ToList();
    }

    private string GetDifficultyLabel(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Beginner => _localizer["Difficulty.Beginner"],
            DifficultyLevel.Intermediate => _localizer["Difficulty.Intermediate"],
            DifficultyLevel.Advanced => _localizer["Difficulty.Advanced"],
            DifficultyLevel.Expert => _localizer["Difficulty.Expert"],
            _ => difficulty.ToString()
        };
    }
}
