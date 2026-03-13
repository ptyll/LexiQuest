using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Caching.Memory;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class GuestSessionServiceTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IGuestSessionService _service;

    public GuestSessionServiceTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        _service = new GuestSessionService(_wordRepository, new MemoryCache(new MemoryCacheOptions()));

        // Setup default mock for any call
        var beginnerWords = new List<Word>
        {
            Word.Create("pes", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("kočka", DifficultyLevel.Beginner, WordCategory.Animals),
            Word.Create("dům", DifficultyLevel.Beginner, WordCategory.Everyday),
            Word.Create("strom", DifficultyLevel.Beginner, WordCategory.Nature),
            Word.Create("kniha", DifficultyLevel.Beginner, WordCategory.Everyday)
        };
        _wordRepository.GetRandomBatchAsync(5, DifficultyLevel.Beginner, null, Arg.Any<CancellationToken>())
            .Returns(beginnerWords);
    }

    [Fact]
    public void StartGame_CreatesAnonymousSession()
    {
        // Act
        var result = _service.StartGame();

        // Assert
        result.Should().NotBeNull();
        result.SessionId.Should().NotBeEmpty();
        result.ScrambledWords.Should().HaveCount(5);
        result.IsGuest.Should().BeTrue();
    }

    [Fact]
    public void StartGame_UsesBeginnerWords()
    {
        // Act
        _service.StartGame();

        // Assert
        _wordRepository.Received(1).GetRandomBatchAsync(5, DifficultyLevel.Beginner, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void StartGame_5WordsPerGame()
    {
        // Act
        var result = _service.StartGame();

        // Assert
        result.ScrambledWords.Should().HaveCount(5);
    }

    [Fact]
    public void SubmitAnswer_Correct_CalculatesXP()
    {
        // Arrange
        var session = _service.StartGame();
        var firstWord = session.ScrambledWords.First();

        // Act
        var result = _service.SubmitAnswer(session.SessionId, firstWord.WordId, "pes");

        // Assert
        result.IsCorrect.Should().BeTrue();
        result.XpEarned.Should().BeGreaterThan(0);
        result.CorrectAnswer.Should().Be("pes");
    }

    [Fact]
    public void SubmitAnswer_Wrong_ShowsCorrectAnswer()
    {
        // Arrange
        var session = _service.StartGame();
        var firstWord = session.ScrambledWords.First();

        // Act
        var result = _service.SubmitAnswer(session.SessionId, firstWord.WordId, "špatná odpověď");

        // Assert
        result.IsCorrect.Should().BeFalse();
        result.XpEarned.Should().Be(0);
        result.CorrectAnswer.Should().Be("pes");
    }

    [Fact]
    public void GetSessionProgress_ReturnsAccumulatedXP()
    {
        // Arrange
        var session = _service.StartGame();
        var firstWord = session.ScrambledWords.First();
        _service.SubmitAnswer(session.SessionId, firstWord.WordId, "pes");

        // Act
        var progress = _service.GetSessionProgress(session.SessionId);

        // Assert
        progress.Should().NotBeNull();
        progress.TotalXp.Should().BeGreaterThan(0);
        progress.WordsSolved.Should().Be(1);
    }
}
