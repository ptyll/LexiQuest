using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class AIChallengeServiceTests
{
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IWordRepository _wordRepository;
    private readonly AIChallengeService _sut;

    public AIChallengeServiceTests()
    {
        _gameSessionRepository = Substitute.For<IGameSessionRepository>();
        _wordRepository = Substitute.For<IWordRepository>();
        _sut = new AIChallengeService(_gameSessionRepository, _wordRepository);
    }

    [Fact]
    public async Task AIChallengeService_Analyze_IdentifiesWeakLetters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Beginner, 10, 3);

        // Create rounds where the user fails on words containing 'X' and 'Z'
        var rounds = new List<GameRound>();
        // Rounds with 'X' - mostly wrong
        for (int i = 0; i < 5; i++)
        {
            var round = GameRound.Create(session.Id, i + 1, Guid.NewGuid(), "XOEB", "BOXE", 30);
            round.RecordAttempt(i < 4 ? "WRONG" : "BOXE", i >= 4, 5000); // 4 out of 5 wrong = 80% error
            rounds.Add(round);
        }
        // Rounds with 'Z' - mostly wrong
        for (int i = 0; i < 4; i++)
        {
            var round = GameRound.Create(session.Id, i + 6, Guid.NewGuid(), "ZEOR", "ZERO", 30);
            round.RecordAttempt(i < 3 ? "WRONG" : "ZERO", i >= 3, 5000); // 3 out of 4 wrong = 75% error
            rounds.Add(round);
        }
        // Rounds with 'A' - mostly correct
        for (int i = 0; i < 5; i++)
        {
            var round = GameRound.Create(session.Id, i + 10, Guid.NewGuid(), "TAC", "CAT", 30);
            round.RecordAttempt("CAT", true, 3000);
            rounds.Add(round);
        }

        SetupSessionRounds(session, rounds);

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession> { session }.AsReadOnly());

        // Act
        var result = await _sut.AnalyzePlayerAsync(userId);

        // Assert
        result.WeakLetters.Should().NotBeEmpty();
        result.WeakLetters.Should().Contain(w => w.Letter == 'X');
        result.WeakLetters.Should().Contain(w => w.Letter == 'Z');
        result.WeakLetters.Should().NotContain(w => w.Letter == 'A');
    }

    [Fact]
    public async Task AIChallengeService_Analyze_IdentifiesSlowCategories()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var easySession = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Beginner, 5, 3);
        var hardSession = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Advanced, 5, 3);

        // Easy rounds - fast
        var easyRounds = new List<GameRound>();
        for (int i = 0; i < 5; i++)
        {
            var round = GameRound.Create(easySession.Id, i + 1, Guid.NewGuid(), "TAC", "CAT", 30);
            round.RecordAttempt("CAT", true, 2000); // 2 seconds
            easyRounds.Add(round);
        }
        SetupSessionRounds(easySession, easyRounds);

        // Hard rounds - slow
        var hardRounds = new List<GameRound>();
        for (int i = 0; i < 5; i++)
        {
            var round = GameRound.Create(hardSession.Id, i + 1, Guid.NewGuid(), "MTHYALORG", "ALGORITHM", 30);
            round.RecordAttempt("ALGORITHM", true, 15000); // 15 seconds
            hardRounds.Add(round);
        }
        SetupSessionRounds(hardSession, hardRounds);

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession> { easySession, hardSession }.AsReadOnly());

        // Act
        var result = await _sut.AnalyzePlayerAsync(userId);

        // Assert
        result.CategoryPerformance.Should().NotBeEmpty();
        var hardCategory = result.CategoryPerformance.FirstOrDefault(c => c.Category == "Advanced");
        hardCategory.Should().NotBeNull();
        hardCategory!.AvgTimeSeconds.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task AIChallengeService_Generate_WeaknessFocus_SelectsProblematicLetters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Beginner, 10, 3);

        var rounds = new List<GameRound>();
        // Fail on words with 'Q'
        for (int i = 0; i < 5; i++)
        {
            var round = GameRound.Create(session.Id, i + 1, Guid.NewGuid(), "UEQNE", "QUEUE", 30);
            round.RecordAttempt("WRONG", false, 5000);
            rounds.Add(round);
        }
        SetupSessionRounds(session, rounds);

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession> { session }.AsReadOnly());

        // Return words containing 'Q'
        var wordsWithQ = new List<Word>
        {
            Word.Create("QUEEN", DifficultyLevel.Intermediate, WordCategory.Everyday),
            Word.Create("QUIET", DifficultyLevel.Intermediate, WordCategory.Everyday),
            Word.Create("QUEST", DifficultyLevel.Intermediate, WordCategory.Everyday),
            Word.Create("APPLE", DifficultyLevel.Beginner, WordCategory.Food),
            Word.Create("TABLE", DifficultyLevel.Beginner, WordCategory.Household)
        };

        _wordRepository.GetRandomBatchAsync(Arg.Any<int>(), Arg.Any<DifficultyLevel?>(), Arg.Any<WordCategory?>(), Arg.Any<CancellationToken>())
            .Returns(wordsWithQ.AsReadOnly());

        var request = new AIChallengeRequest(AIChallengeType.WeaknessFocus);

        // Act
        var result = await _sut.GenerateChallengeAsync(userId, request);

        // Assert
        result.Type.Should().Be(AIChallengeType.WeaknessFocus);
        result.Words.Should().NotBeEmpty();
        // Should contain words with Q (the weak letter)
        result.Words.Should().Contain(w => w.Word.Contains('Q', StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task AIChallengeService_Generate_SpeedTraining_SelectsShortWords()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession>().AsReadOnly());

        var words = new List<Word>
        {
            Word.Create("CAT", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("DOG", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("SUN", DifficultyLevel.Beginner, WordCategory.Nature),
            Word.Create("EXTRAORDINARILY", DifficultyLevel.Advanced, WordCategory.Abstract),
            Word.Create("HIPPOPOTAMUS", DifficultyLevel.Advanced, WordCategory.Animals)
        };

        _wordRepository.GetRandomBatchAsync(Arg.Any<int>(), Arg.Any<DifficultyLevel?>(), Arg.Any<WordCategory?>(), Arg.Any<CancellationToken>())
            .Returns(words.AsReadOnly());

        var request = new AIChallengeRequest(AIChallengeType.SpeedTraining);

        // Act
        var result = await _sut.GenerateChallengeAsync(userId, request);

        // Assert
        result.Type.Should().Be(AIChallengeType.SpeedTraining);
        result.Words.Should().NotBeEmpty();
        result.Words.Should().OnlyContain(w => w.Word.Length <= 5);
    }

    [Fact]
    public async Task AIChallengeService_Generate_MemoryGame_SelectsRepeatedWords()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Intermediate, 5, 3);

        var rounds = new List<GameRound>();
        // User got these words wrong
        var wrongWord1 = GameRound.Create(session.Id, 1, Guid.NewGuid(), "CNEESI", "SCIENCE", 30);
        wrongWord1.RecordAttempt("SCIENEC", false, 8000);
        rounds.Add(wrongWord1);

        var wrongWord2 = GameRound.Create(session.Id, 2, Guid.NewGuid(), "LCUTRUE", "CULTURE", 30);
        wrongWord2.RecordAttempt("CULTRRE", false, 9000);
        rounds.Add(wrongWord2);

        // User got this word right
        var rightWord = GameRound.Create(session.Id, 3, Guid.NewGuid(), "TAC", "CAT", 30);
        rightWord.RecordAttempt("CAT", true, 2000);
        rounds.Add(rightWord);

        SetupSessionRounds(session, rounds);

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession> { session }.AsReadOnly());

        var request = new AIChallengeRequest(AIChallengeType.MemoryGame);

        // Act
        var result = await _sut.GenerateChallengeAsync(userId, request);

        // Assert
        result.Type.Should().Be(AIChallengeType.MemoryGame);
        result.Words.Should().NotBeEmpty();
        result.Words.Should().Contain(w => w.Word == "SCIENCE");
        result.Words.Should().Contain(w => w.Word == "CULTURE");
        result.Words.Should().NotContain(w => w.Word == "CAT");
    }

    [Fact]
    public async Task AIChallengeService_Generate_PatternRecognition_SelectsSimilarWords()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Intermediate, 5, 3);

        var rounds = new List<GameRound>();
        // User struggles with 7-letter words
        for (int i = 0; i < 4; i++)
        {
            var round = GameRound.Create(session.Id, i + 1, Guid.NewGuid(), "CNEEIS", "SCIENCE", 30);
            round.RecordAttempt("WRONG", false, 8000);
            rounds.Add(round);
        }
        SetupSessionRounds(session, rounds);

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession> { session }.AsReadOnly());

        var words = new List<Word>
        {
            Word.Create("CULTURE", DifficultyLevel.Intermediate, WordCategory.Abstract),     // 7 letters
            Word.Create("FREEDOM", DifficultyLevel.Intermediate, WordCategory.Abstract),     // 7 letters
            Word.Create("HISTORY", DifficultyLevel.Intermediate, WordCategory.History),      // 7 letters
            Word.Create("CAT", DifficultyLevel.Beginner, WordCategory.Animals),            // 3 letters
            Word.Create("EXTRAORDINARILY", DifficultyLevel.Advanced, WordCategory.Abstract) // 15 letters
        };

        _wordRepository.GetRandomBatchAsync(Arg.Any<int>(), Arg.Any<DifficultyLevel?>(), Arg.Any<WordCategory?>(), Arg.Any<CancellationToken>())
            .Returns(words.AsReadOnly());

        var request = new AIChallengeRequest(AIChallengeType.PatternRecognition);

        // Act
        var result = await _sut.GenerateChallengeAsync(userId, request);

        // Assert
        result.Type.Should().Be(AIChallengeType.PatternRecognition);
        result.Words.Should().NotBeEmpty();
        // Should select words with similar length (7 letters)
        result.Words.Should().OnlyContain(w => w.Word.Length == 7);
    }

    [Fact]
    public async Task AIChallengeService_PredictDifficulty_ReturnsScore0to1()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.Create(userId, GameMode.Training, DifficultyLevel.Intermediate, 10, 3);

        var rounds = new List<GameRound>();
        for (int i = 0; i < 10; i++)
        {
            var round = GameRound.Create(session.Id, i + 1, Guid.NewGuid(), "TAC", "CAT", 30);
            round.RecordAttempt("CAT", i % 2 == 0, 5000); // 50% success rate
            rounds.Add(round);
        }
        SetupSessionRounds(session, rounds);

        _gameSessionRepository.GetByUserIdWithRoundsAsync(userId, Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameSession> { session }.AsReadOnly());

        var words = new List<Word>
        {
            Word.Create("TEST", DifficultyLevel.Beginner, WordCategory.Everyday),
            Word.Create("EXAM", DifficultyLevel.Beginner, WordCategory.Everyday)
        };

        _wordRepository.GetRandomBatchAsync(Arg.Any<int>(), Arg.Any<DifficultyLevel?>(), Arg.Any<WordCategory?>(), Arg.Any<CancellationToken>())
            .Returns(words.AsReadOnly());

        var request = new AIChallengeRequest(AIChallengeType.SpeedTraining);

        // Act
        var result = await _sut.GenerateChallengeAsync(userId, request);

        // Assert
        result.PredictedDifficulty.Should().BeGreaterThanOrEqualTo(0.0);
        result.PredictedDifficulty.Should().BeLessThanOrEqualTo(1.0);
    }

    /// <summary>
    /// Uses reflection to set the Rounds property since it has a private setter.
    /// </summary>
    private static void SetupSessionRounds(GameSession session, List<GameRound> rounds)
    {
        var roundsProperty = typeof(GameSession).GetProperty("Rounds")!;
        roundsProperty.SetValue(session, rounds);
    }
}
