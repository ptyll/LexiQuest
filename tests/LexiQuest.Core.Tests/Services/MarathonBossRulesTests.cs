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

public class MarathonBossRulesTests
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<MarathonBossRules> _localizer;
    private readonly MarathonBossRules _sut;

    public MarathonBossRulesTests()
    {
        _wordRepository = Substitute.For<IWordRepository>();
        _xpCalculator = Substitute.For<IXpCalculator>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<MarathonBossRules>>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _sut = new MarathonBossRules(
            _wordRepository,
            _xpCalculator,
            _unitOfWork,
            _localizer);
    }

    [Fact]
    public void MarathonBoss_Start_20Words3Lives()
    {
        // Arrange & Act
        var (totalRounds, lives) = MarathonBossRules.GetSettings();

        // Assert
        totalRounds.Should().Be(20);
        lives.Should().Be(3);
    }

    [Fact]
    public async Task MarathonBoss_WrongAnswer_DecreasesLife_NoRegen()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);
        var initialLives = session.LivesRemaining;

        // Act
        await _sut.ProcessWrongAnswerAsync(session);

        // Assert
        session.LivesRemaining.Should().Be(initialLives - 1);
    }

    [Fact]
    public async Task MarathonBoss_0Lives_GameOver()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);
        
        // Act
        await _sut.ProcessWrongAnswerAsync(session);
        await _sut.ProcessWrongAnswerAsync(session);
        await _sut.ProcessWrongAnswerAsync(session);

        // Assert
        session.LivesRemaining.Should().Be(0);
        session.IsGameOver.Should().BeTrue();
    }

    [Fact]
    public void MarathonBoss_AllCorrect_PerfectBonus200XP()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);
        session.Complete(); // Simulate perfect completion

        // Act
        var bonus = _sut.CalculateCompletionBonus(session, true);

        // Assert
        bonus.Should().Be(200);
    }

    [Fact]
    public void MarathonBoss_Completed_WithLosses_100XP()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);
        session.Complete(); // Simulate completion with some losses

        // Act
        var bonus = _sut.CalculateCompletionBonus(session, false);

        // Assert
        bonus.Should().Be(100);
    }

    [Fact]
    public void MarathonBoss_Under5Min_SpeedBonus50XP()
    {
        // Arrange
        var session = GameSession.CreateBossSession(Guid.NewGuid(), BossType.Marathon, DifficultyLevel.Intermediate);
        // Simulate 4 minutes duration
        var duration = TimeSpan.FromMinutes(4);

        // Act
        var bonus = _sut.CalculateSpeedBonus(duration);

        // Assert
        bonus.Should().Be(50);
    }
}

public class MarathonBossRules : IBossRules
{
    private readonly IWordRepository _wordRepository;
    private readonly IXpCalculator _xpCalculator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<MarathonBossRules> _localizer;

    public MarathonBossRules(
        IWordRepository wordRepository,
        IXpCalculator xpCalculator,
        IUnitOfWork unitOfWork,
        IStringLocalizer<MarathonBossRules> localizer)
    {
        _wordRepository = wordRepository;
        _xpCalculator = xpCalculator;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public static (int TotalRounds, int Lives) GetSettings() => (20, 3);

    public async Task ProcessWrongAnswerAsync(GameSession session)
    {
        session.LoseLife();
        await Task.CompletedTask;
    }

    public int CalculateCompletionBonus(GameSession session, bool perfectRun)
    {
        return perfectRun ? 200 : 100;
    }

    public int CalculateSpeedBonus(TimeSpan duration)
    {
        return duration.TotalMinutes < 5 ? 50 : 0;
    }

    public Task<GameSession> InitializeSessionAsync(Guid userId, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var session = GameSession.CreateBossSession(userId, BossType.Marathon, difficulty);
        return Task.FromResult(session);
    }
}
