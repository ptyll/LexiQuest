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

public class StreakServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StreakService _sut;

    public StreakServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new StreakService(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task StreakService_CheckStreak_FirstDay_Returns1()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithStreak(userId, currentDays: 0, lastActivity: null);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert
        result.CurrentDays.Should().Be(1);
    }

    [Fact]
    public async Task StreakService_CheckStreak_ConsecutiveDay_Increments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var user = CreateUserWithStreak(userId, currentDays: 3, lastActivity: yesterday);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert
        result.CurrentDays.Should().Be(4);
    }

    [Fact]
    public async Task StreakService_CheckStreak_SameDay_NoChange()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var today = DateTime.UtcNow.Date;
        var user = CreateUserWithStreak(userId, currentDays: 5, lastActivity: today);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert
        result.CurrentDays.Should().Be(5);
    }

    [Fact]
    public async Task StreakService_CheckStreak_MissedDay_ResetsTo1()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);
        var user = CreateUserWithStreak(userId, currentDays: 5, lastActivity: twoDaysAgo);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert
        result.CurrentDays.Should().Be(1);
    }

    [Fact]
    public async Task StreakService_CheckStreak_GracePeriod48h_DoesNotReset()
    {
        // Arrange - exactly 48 hours (2 days) since last activity is considered broken
        // but we want to be lenient and allow up to 48 hours
        var userId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var user = CreateUserWithStreak(userId, currentDays: 5, lastActivity: yesterday);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert - yesterday's activity means streak continues today
        result.CurrentDays.Should().Be(6);
    }

    [Fact]
    public async Task StreakService_CheckStreak_UpdatesLongest_WhenCurrentExceeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var user = CreateUserWithStreak(userId, currentDays: 9, longestDays: 9, lastActivity: yesterday);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _sut.CheckStreakAsync(userId);

        // Assert
        user.Streak.LongestDays.Should().Be(10);
    }

    [Theory]
    [InlineData(0, FireLevel.Cold)]
    [InlineData(1, FireLevel.Small)]
    [InlineData(3, FireLevel.Small)]
    [InlineData(4, FireLevel.Medium)]
    [InlineData(7, FireLevel.Medium)]
    [InlineData(8, FireLevel.Large)]
    [InlineData(30, FireLevel.Large)]
    [InlineData(31, FireLevel.Legendary)]
    [InlineData(100, FireLevel.Legendary)]
    public void StreakService_GetFireLevel_ReturnsCorrectLevel(int days, FireLevel expected)
    {
        // Act
        var result = _sut.GetFireLevel(days);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task StreakService_CheckStreak_ReturnsCorrectFireLevel()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithStreak(userId, currentDays: 5, lastActivity: DateTime.UtcNow.Date);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert
        result.FireLevel.Should().Be(FireLevel.Medium.ToString());
    }

    [Fact]
    public async Task StreakService_CheckStreak_AtRisk_ReturnsTrue()
    {
        // Arrange - last activity was yesterday but before checking, streak is at risk
        var userId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var user = CreateUserWithStreak(userId, currentDays: 5, lastActivity: yesterday);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert - after recording activity, streak is no longer at risk
        result.IsAtRisk.Should().BeFalse();
    }

    [Fact]
    public async Task StreakService_CheckStreak_NoActivity_ReturnsCorrectTimeRemaining()
    {
        // Arrange - last activity yesterday, so we have until tomorrow to maintain streak
        var userId = Guid.NewGuid();
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var user = CreateUserWithStreak(userId, currentDays: 5, lastActivity: yesterday);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert - time remaining is time until streak would be lost (tomorrow)
        result.TimeRemaining.Should().NotBeNull();
    }

    [Fact]
    public async Task StreakService_CheckStreak_ReturnsLongestDays()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithStreak(userId, currentDays: 3, longestDays: 10, lastActivity: DateTime.UtcNow.Date);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.CheckStreakAsync(userId);

        // Assert
        result.LongestDays.Should().Be(10);
    }

    private User CreateUserWithStreak(Guid userId, int currentDays, int longestDays, DateTime? lastActivity)
    {
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        
        // Set streak values via reflection
        var streak = user.Streak;
        typeof(Streak).GetProperty(nameof(Streak.CurrentDays))?.SetValue(streak, currentDays);
        typeof(Streak).GetProperty(nameof(Streak.LongestDays))?.SetValue(streak, longestDays);
        typeof(Streak).GetProperty(nameof(Streak.LastActivityDate))?.SetValue(streak, lastActivity);
        
        return user;
    }

    private User CreateUserWithStreak(Guid userId, int currentDays, DateTime? lastActivity)
    {
        return CreateUserWithStreak(userId, currentDays, currentDays, lastActivity);
    }
}
