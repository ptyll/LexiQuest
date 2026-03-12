using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class AchievementServiceTests
{
    private readonly IAchievementRepository _achievementRepository;
    private readonly IUserAchievementRepository _userAchievementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<AchievementService> _localizer;
    private readonly AchievementService _sut;

    public AchievementServiceTests()
    {
        _achievementRepository = Substitute.For<IAchievementRepository>();
        _userAchievementRepository = Substitute.For<IUserAchievementRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<AchievementService>>();
        
        _localizer[Arg.Any<string>()].Returns(ci => new LocalizedString(ci.Arg<string>(), ci.Arg<string>()));
        
        _sut = new AchievementService(
            _achievementRepository,
            _userAchievementRepository,
            _unitOfWork,
            _localizer);
    }

    [Fact]
    public async Task AchievementService_CheckWordSolved_FirstWord_UnlocksAchievement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("first_word", AchievementCategory.Performance, 1);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        
        _achievementRepository.GetByKeyAsync("first_word").Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.CheckWordSolvedAsync(userId, 1);

        // Assert
        result.Should().ContainSingle(a => a.AchievementKey == "first_word");
        userAchievement.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public async Task AchievementService_CheckWordSolved_100Words_UnlocksAchievement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("100_words", AchievementCategory.Performance, 100);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        
        _achievementRepository.GetByKeyAsync("100_words").Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.CheckWordSolvedAsync(userId, 100);

        // Assert
        result.Should().ContainSingle(a => a.AchievementKey == "100_words");
    }

    [Fact]
    public async Task AchievementService_CheckStreak_3Days_UnlocksAchievement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("streak_3", AchievementCategory.Streak, 3);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        
        _achievementRepository.GetByKeyAsync("streak_3").Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.CheckStreakAsync(userId, 3);

        // Assert
        result.Should().ContainSingle(a => a.AchievementKey == "streak_3");
    }

    [Fact]
    public async Task AchievementService_CheckStreak_7Days_UnlocksAchievementAnd50XP()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("streak_7", AchievementCategory.Streak, 7, 50);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        
        _achievementRepository.GetByKeyAsync("streak_7").Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.CheckStreakAsync(userId, 7);

        // Assert
        result.Should().ContainSingle();
        result[0].XPEarned.Should().Be(50);
    }

    [Fact]
    public async Task AchievementService_CheckPathComplete_UnlocksAchievement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("path_complete", AchievementCategory.Special, 1);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        
        _achievementRepository.GetByKeyAsync("path_complete").Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.CheckPathCompletedAsync(userId, Guid.NewGuid());

        // Assert
        result.Should().ContainSingle(a => a.AchievementKey == "path_complete");
    }

    [Fact]
    public async Task AchievementService_GetProgress_ReturnsCorrectPercentage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("100_words", AchievementCategory.Performance, 100);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        userAchievement.UpdateProgress(50);
        
        _achievementRepository.GetByIdAsync(achievement.Id).Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.GetProgressAsync(userId, achievement.Id);

        // Assert
        result.Should().Be(50);
    }

    [Fact]
    public async Task AchievementService_AlreadyUnlocked_DoesNotDuplicate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievement = CreateAchievement("first_word", AchievementCategory.Performance, 1);
        var userAchievement = UserAchievement.Create(userId, achievement.Id);
        userAchievement.Unlock();
        
        _achievementRepository.GetByKeyAsync("first_word").Returns(achievement);
        _userAchievementRepository.GetByUserAndAchievementAsync(userId, achievement.Id).Returns(userAchievement);

        // Act
        var result = await _sut.CheckWordSolvedAsync(userId, 1);

        // Assert
        result.Should().BeEmpty();
    }

    private static Achievement CreateAchievement(string key, AchievementCategory category, int requiredValue, int xpReward = 10)
    {
        return Achievement.Create(key, category, xpReward, key, $"Description for {key}", requiredValue);
    }
}
