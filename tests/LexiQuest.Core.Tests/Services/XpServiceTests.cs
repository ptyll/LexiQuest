using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class XpServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly XpService _sut;

    public XpServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        // Use real LevelCalculator for integration testing
        _sut = new XpService(_userRepository, _unitOfWork, new LevelCalculator());
    }

    [Fact]
    public async Task XpService_AddXP_UpdatesUserStats()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 100);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 50, XpSource.Game);

        // Assert
        user.Stats.TotalXP.Should().Be(150);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task XpService_AddXP_DetectsLevelUp_ReturnsUnlocks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 90, currentLevel: 1);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 20, XpSource.Game);

        // Assert
        result.Should().NotBeNull();
        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(2);
        result.Amount.Should().Be(20);
        result.Source.Should().Be("Game");
    }

    [Fact]
    public async Task XpService_AddXP_MultipleLevelUps_HandlesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 50, currentLevel: 1);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act - Add enough XP to jump from Level 1 to Level 3
        // Level 1->2 needs 100 XP, Level 2->3 needs 150 XP = 250 total from start
        // Current 50 + 250 = 300 XP should be Level 3
        var result = await _sut.AddXpAsync(userId, 250, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(3);
        user.Stats.TotalXP.Should().Be(300);
    }

    [Fact]
    public async Task XpService_AddXP_UnlocksPath2_AtLevel3()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 200, currentLevel: 2);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act - Add 50 XP to reach Level 3 (cumulative 250 XP needed)
        var result = await _sut.AddXpAsync(userId, 50, XpSource.Game);

        // Assert
        result.Unlocks.Should().NotBeNull();
        result.Unlocks.Should().Contain(u => u.Type == "Path" && u.Name == "Intermediate");
    }

    [Fact]
    public async Task XpService_AddXP_UnlocksLeagues_AtLevel5()
    {
        // Arrange
        var userId = Guid.NewGuid();
        // Level 5 requires 812 cumulative XP (100+150+225+337)
        var user = CreateUserWithXp(userId, currentXp: 700, currentLevel: 4);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 150, XpSource.Game);

        // Assert
        result.Unlocks.Should().NotBeNull();
        result.Unlocks.Should().Contain(u => u.Type == "Feature" && u.Name == "Leagues");
    }

    [Fact]
    public async Task XpService_AddXP_NoLevelUp_NoUnlocks()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 100, currentLevel: 2);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 30, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeFalse();
        result.NewLevel.Should().Be(2);
        result.Unlocks.Should().BeNull();
    }

    [Fact]
    public async Task XpService_AddXP_FromDailyChallenge_ReturnsCorrectSource()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 100);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 50, XpSource.DailyChallenge);

        // Assert
        result.Source.Should().Be("DailyChallenge");
    }

    [Fact]
    public async Task XpService_AddXP_FromStreak_ReturnsCorrectSource()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 100);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 25, XpSource.Streak);

        // Assert
        result.Source.Should().Be("Streak");
    }

    [Fact]
    public async Task XpService_AddXP_UpdatesUserLevelProperty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 90, currentLevel: 1);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _sut.AddXpAsync(userId, 30, XpSource.Game);

        // Assert
        user.Stats.Level.Should().Be(2);
    }

    [Fact]
    public async Task XpService_AddXP_ReturnsCorrectTotalXP()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithXp(userId, currentXp: 500);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.AddXpAsync(userId, 75, XpSource.Game);

        // Assert
        result.TotalXP.Should().Be(575);
    }

    private User CreateUserWithXp(Guid userId, int currentXp, int currentLevel = 1)
    {
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        // Use reflection to set private fields for testing
        var stats = user.Stats;
        typeof(UserStats).GetProperty(nameof(UserStats.TotalXP))?.SetValue(stats, currentXp);
        typeof(UserStats).GetProperty(nameof(UserStats.Level))?.SetValue(stats, currentLevel);
        return user;
    }
}
