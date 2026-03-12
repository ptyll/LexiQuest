using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Core.Services.BossRules;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class BossServiceTests
{
    private readonly IBossRules _marathonRules;
    private readonly IBossRules _conditionRules;
    private readonly IBossRules _twistRules;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<BossService> _localizer;
    private readonly BossService _sut;

    public BossServiceTests()
    {
        _marathonRules = Substitute.For<IBossRules>();
        _conditionRules = Substitute.For<IBossRules>();
        _twistRules = Substitute.For<IBossRules>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<BossService>>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _sut = new BossService(
            _marathonRules,
            _conditionRules,
            _twistRules,
            _unitOfWork,
            _localizer);
    }

    [Theory]
    [InlineData(BossType.Marathon, 20, 3)]
    [InlineData(BossType.Condition, 15, 3)]
    [InlineData(BossType.Twist, 12, 3)]
    public async Task StartBossGame_CorrectSettings(BossType type, int expectedRounds, int expectedLives)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.CreateBossSession(userId, type, DifficultyLevel.Intermediate);
        
        IBossRules selectedRules = type switch
        {
            BossType.Marathon => _marathonRules,
            BossType.Condition => _conditionRules,
            BossType.Twist => _twistRules,
            _ => _marathonRules
        };
        
        selectedRules.InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        var result = await _sut.StartBossGameAsync(userId, type, DifficultyLevel.Intermediate);

        // Assert
        result.TotalRounds.Should().Be(expectedRounds);
        result.LivesRemaining.Should().Be(expectedLives);
        result.BossType.Should().Be(type);
    }

    [Fact]
    public async Task StartBossGame_Marathon_UsesMarathonRules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.CreateBossSession(userId, BossType.Marathon, DifficultyLevel.Intermediate);
        _marathonRules.InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        await _sut.StartBossGameAsync(userId, BossType.Marathon, DifficultyLevel.Intermediate);

        // Assert
        await _marathonRules.Received(1).InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartBossGame_Condition_UsesConditionRules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.CreateBossSession(userId, BossType.Condition, DifficultyLevel.Intermediate);
        _conditionRules.InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        await _sut.StartBossGameAsync(userId, BossType.Condition, DifficultyLevel.Intermediate);

        // Assert
        await _conditionRules.Received(1).InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartBossGame_Twist_UsesTwistRules()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = GameSession.CreateBossSession(userId, BossType.Twist, DifficultyLevel.Intermediate);
        _twistRules.InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>())
            .Returns(session);

        // Act
        await _sut.StartBossGameAsync(userId, BossType.Twist, DifficultyLevel.Intermediate);

        // Assert
        await _twistRules.Received(1).InitializeSessionAsync(userId, DifficultyLevel.Intermediate, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBossRules_Marathon_ReturnsMarathonRules()
    {
        // Act
        var rules = _sut.GetBossRules(BossType.Marathon);

        // Assert
        rules.Should().Be(_marathonRules);
    }

    [Fact]
    public void GetBossRules_Condition_ReturnsConditionRules()
    {
        // Act
        var rules = _sut.GetBossRules(BossType.Condition);

        // Assert
        rules.Should().Be(_conditionRules);
    }

    [Fact]
    public void GetBossRules_Twist_ReturnsTwistRules()
    {
        // Act
        var rules = _sut.GetBossRules(BossType.Twist);

        // Assert
        rules.Should().Be(_twistRules);
    }
}
