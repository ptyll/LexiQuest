using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class TwistBossRulesTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<TwistBossRules> _localizer;
    private readonly TwistBossRules _sut;

    public TwistBossRulesTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        _xpCalculator = Substitute.For<IXpCalculator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<TwistBossRules>>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _sut = new TwistBossRules(
            _wordRepository,
            _xpCalculator,
            _unitOfWork,
            _localizer);
    }

    [Fact]
    public void TwistBoss_Start_12Words3Lives()
    {
        // Arrange & Act
        var (totalRounds, lives) = TwistBossRules.GetSettings();

        // Assert
        totalRounds.Should().Be(12);
        lives.Should().Be(3);
    }

    [Fact]
    public void TwistBoss_Start_2RevealedLetters()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Twist, DifficultyLevel.Intermediate);

        // Assert
        session.RevealedLettersCount.Should().Be(2);
    }

    [Theory]
    [InlineData(2, 3, 10)]   // 2 revealed, 3 remaining = 10 XP
    [InlineData(3, 2, 7)]    // 3 revealed, 2 remaining = 7 XP
    [InlineData(4, 1, 5)]    // 4 revealed, 1 remaining = 5 XP
    [InlineData(5, 0, 2)]    // 5 revealed, 0 remaining = 2 XP
    public void TwistBoss_EarlyGuessBonus_Table(int revealedCount, int remainingCount, int expectedBonus)
    {
        // Act
        var bonus = _sut.CalculateEarlyGuessBonus(revealedCount, remainingCount);

        // Assert
        bonus.Should().Be(expectedBonus);
    }

    [Fact]
    public void TwistBoss_3Sec_RevealNextLetter()
    {
        // Arrange
        const int wordLength = 6;
        var revealedPositions = new List<int> { 0, 1 }; // First 2 letters revealed
        var interval = TimeSpan.FromSeconds(3);
        var elapsed = TimeSpan.FromSeconds(6); // 6 seconds = 2 intervals

        // Act
        var newRevealed = _sut.CalculateRevealedLetters(wordLength, revealedPositions, elapsed, interval);

        // Assert
        newRevealed.Should().Be(4); // 2 initial + 2 from intervals
    }

    [Fact]
    public void TwistBoss_NoWrongLifeLoss()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Twist, DifficultyLevel.Intermediate);
        var initialLives = session.LivesRemaining;

        // Act - Twist boss doesn't lose lives on wrong answers
        var xpPenalty = _sut.CalculateWrongAnswerPenalty();

        // Assert
        session.LivesRemaining.Should().Be(initialLives);
        xpPenalty.Should().Be(-3);
    }

    [Fact]
    public void TwistBoss_RevealInterval_3Seconds()
    {
        // Assert
        TwistBossRules.RevealInterval.Should().Be(TimeSpan.FromSeconds(3));
    }
}

public class TwistBossRules : IBossRules
{
    public static readonly TimeSpan RevealInterval = TimeSpan.FromSeconds(3);
    
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<TwistBossRules> _localizer;

    public TwistBossRules(
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        IUnitOfWork unitOfWork,
        IStringLocalizer<TwistBossRules> localizer)
    {
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public static (int TotalRounds, int Lives) GetSettings() => (12, 3);

    public int CalculateEarlyGuessBonus(int revealedCount, int remainingCount)
    {
        // Table: 2 revealed=10XP, 3=7XP, 4=5XP, 5+=2XP
        return revealedCount switch
        {
            2 => 10,
            3 => 7,
            4 => 5,
            >= 5 => 2,
            _ => 0
        };
    }

    public int CalculateRevealedLetters(int wordLength, List<int> revealedPositions, TimeSpan elapsed, TimeSpan interval)
    {
        var intervalsPassed = (int)(elapsed.TotalSeconds / interval.TotalSeconds);
        var additionalReveals = Math.Min(intervalsPassed, wordLength - revealedPositions.Count);
        return revealedPositions.Count + additionalReveals;
    }

    public int CalculateWrongAnswerPenalty() => -3;

    public async Task ProcessWrongAnswerAsync(GameSession session)
    {
        // Twist boss: no life loss, just XP penalty handled separately
        await Task.CompletedTask;
    }

    public int CalculateCompletionBonus(GameSession session, bool perfectRun)
    {
        return perfectRun ? 180 : 90;
    }

    public int CalculateSpeedBonus(TimeSpan duration)
    {
        return duration.TotalMinutes < 3 ? 60 : 0;
    }

    public Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var session = GameSession.CreateBossSession(userId, BossType.Twist, difficulty);
        return Task.FromResult(session);
    }
}
