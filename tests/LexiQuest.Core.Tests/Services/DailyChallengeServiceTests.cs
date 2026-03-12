using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class DailyChallengeServiceTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IDailyChallengeRepository _challengeRepository;
    private readonly IGameSessionService _gameSessionService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<DailyChallengeService> _localizer;
    private readonly DailyChallengeService _sut;

    public DailyChallengeServiceTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        _challengeRepository = Substitute.For<IDailyChallengeRepository>();
        _gameSessionService = Substitute.For<IGameSessionService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<DailyChallengeService>>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), $"Localized:{ci.Arg<string>()}"));
        
        _sut = new DailyChallengeService(
            _wordRepository,
            _challengeRepository,
            _gameSessionService,
            _unitOfWork,
            _localizer);
    }

    [Fact]
    public async Task DailyChallengeService_GetToday_ReturnsChallengeForCurrentDate()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var word = Word.Create("test", DifficultyLevel.Beginner, WordCategory.Animals);
        var challenge = DailyChallenge.Create(today, word.Id, DailyModifier.Category);
        
        _challengeRepository.GetByDateAsync(today).Returns(challenge);
        _wordRepository.GetByIdAsync(word.Id).Returns(word);

        // Act
        var result = await _sut.GetTodayAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Date.Should().Be(today);
    }

    [Fact]
    public async Task DailyChallengeService_GetToday_SameWordForAllUsers()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var word = Word.Create("puzzle", DifficultyLevel.Intermediate, WordCategory.Science);
        var challenge = DailyChallenge.Create(today, word.Id, DailyModifier.Speed);
        
        _challengeRepository.GetByDateAsync(today).Returns(challenge);
        _wordRepository.GetByIdAsync(word.Id).Returns(word);

        // Act
        var result1 = await _sut.GetTodayAsync();
        var result2 = await _sut.GetTodayAsync();

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.WordId.Should().Be(result2!.WordId);
    }

    [Theory]
    [InlineData(DayOfWeek.Monday, DailyModifier.Category)]
    [InlineData(DayOfWeek.Tuesday, DailyModifier.Speed)]
    [InlineData(DayOfWeek.Wednesday, DailyModifier.NoHints)]
    [InlineData(DayOfWeek.Thursday, DailyModifier.DoubleLetters)]
    [InlineData(DayOfWeek.Friday, DailyModifier.Team)]
    [InlineData(DayOfWeek.Saturday, DailyModifier.Hard)]
    [InlineData(DayOfWeek.Sunday, DailyModifier.Easy)]
    public void DailyChallengeService_GetModifier_ReturnsCorrectModifier(DayOfWeek day, DailyModifier expectedModifier)
    {
        // Act
        var result = DailyChallengeService.GetModifierForDay(day);

        // Assert
        result.Should().Be(expectedModifier);
    }

    [Fact]
    public async Task DailyChallengeService_SubmitChallenge_CalculatesXPWithModifier()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;
        var word = Word.Create("bonus", DifficultyLevel.Intermediate, WordCategory.Food);
        var challenge = DailyChallenge.Create(today, word.Id, DailyModifier.Category);
        
        _challengeRepository.GetByDateAsync(today).Returns(challenge);
        _wordRepository.GetByIdAsync(word.Id).Returns(word);

        // Act
        var result = await _sut.SubmitAnswerAsync(userId, today, "bonus", TimeSpan.FromSeconds(5));

        // Assert
        result.Should().NotBeNull();
        result.XPEarned.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task DailyChallengeService_AlreadyCompleted_ReturnsForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;
        var word = Word.Create("done", DifficultyLevel.Beginner, WordCategory.Animals);
        var challenge = DailyChallenge.Create(today, word.Id, DailyModifier.Easy);
        
        _challengeRepository.GetByDateAsync(today).Returns(challenge);
        _challengeRepository.HasUserCompletedAsync(userId, today).Returns(true);

        // Act
        var act = async () => await _sut.SubmitAnswerAsync(userId, today, "done", TimeSpan.FromSeconds(3));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Localized:Error.AlreadyCompleted*");
    }

    [Fact]
    public async Task DailyChallengeService_GetLeaderboard_ReturnsSortedByTime()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var leaderboard = new List<DailyLeaderboardEntry>
        {
            new(Guid.NewGuid(), "User1", TimeSpan.FromSeconds(10), 100),
            new(Guid.NewGuid(), "User2", TimeSpan.FromSeconds(5), 100),
            new(Guid.NewGuid(), "User3", TimeSpan.FromSeconds(15), 95)
        };
        
        _challengeRepository.GetLeaderboardAsync(today).Returns(leaderboard);

        // Act
        var result = await _sut.GetLeaderboardAsync(today);

        // Assert
        result.Should().HaveCount(3);
        result[0].Username.Should().Be("User2"); // Fastest
        result[1].Username.Should().Be("User1");
        result[2].Username.Should().Be("User3");
    }

    [Fact]
    public async Task DailyChallengeService_CreateNew_GeneratesDeterministicWord()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var words = new List<Word>
        {
            Word.Create("word1", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("word2", DifficultyLevel.Intermediate, WordCategory.Science),
            Word.Create("word3", DifficultyLevel.Expert, WordCategory.Food)
        };
        
        _challengeRepository.GetByDateAsync(today).Returns((DailyChallenge?)null);
        _wordRepository.GetRandomAsync(Arg.Any<DifficultyLevel>()).Returns(words[0]);

        // Act
        var result = await _sut.GetOrCreateTodayAsync();

        // Assert
        await _challengeRepository.Received(1).AddAsync(Arg.Is<DailyChallenge>(c => c.Date == today));
        await _unitOfWork.Received(1).SaveChangesAsync();
    }
}
