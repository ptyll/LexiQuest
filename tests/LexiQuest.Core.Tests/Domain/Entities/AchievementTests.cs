using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Xunit;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class AchievementTests
{
    [Fact]
    public void Achievement_Create_SetsProperties()
    {
        // Arrange & Act
        var achievement = Achievement.Create(
            "first_word",
            AchievementCategory.Performance,
            10,
            "First Word",
            "Solve your first word",
            1);

        // Assert
        achievement.Id.Should().NotBe(Guid.Empty);
        achievement.Key.Should().Be("first_word");
        achievement.Category.Should().Be(AchievementCategory.Performance);
        achievement.XPReward.Should().Be(10);
        achievement.Name.Should().Be("First Word");
        achievement.Description.Should().Be("Solve your first word");
        achievement.RequiredValue.Should().Be(1);
    }
}

public class UserAchievementTests
{
    [Fact]
    public void UserAchievement_Create_SetsProperties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var achievementId = Guid.NewGuid();

        // Act
        var userAchievement = UserAchievement.Create(userId, achievementId);

        // Assert
        userAchievement.Id.Should().NotBe(Guid.Empty);
        userAchievement.UserId.Should().Be(userId);
        userAchievement.AchievementId.Should().Be(achievementId);
        userAchievement.Progress.Should().Be(0);
        userAchievement.IsUnlocked.Should().BeFalse();
        userAchievement.UnlockedAt.Should().BeNull();
    }

    [Fact]
    public void UserAchievement_UpdateProgress_IncreasesProgress()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        userAchievement.UpdateProgress(5);

        // Assert
        userAchievement.Progress.Should().Be(5);
        userAchievement.IsUnlocked.Should().BeFalse();
    }

    [Fact]
    public void UserAchievement_UpdateProgress_ReachesRequiredValue_Unlocks()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        userAchievement.UpdateProgress(10);

        // Assert
        userAchievement.Progress.Should().Be(10);
    }

    [Fact]
    public void UserAchievement_Unlock_SetsUnlockedAt()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        userAchievement.Unlock();

        // Assert
        userAchievement.IsUnlocked.Should().BeTrue();
        userAchievement.UnlockedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UserAchievement_Unlock_AlreadyUnlocked_DoesNotChangeUnlockedAt()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(Guid.NewGuid(), Guid.NewGuid());
        userAchievement.Unlock();
        var originalUnlockedAt = userAchievement.UnlockedAt;

        // Act
        userAchievement.Unlock();

        // Assert
        userAchievement.UnlockedAt.Should().Be(originalUnlockedAt);
    }

    [Fact]
    public void UserAchievement_GetProgressPercentage_ReturnsCorrectValue()
    {
        // Arrange
        var userAchievement = UserAchievement.Create(Guid.NewGuid(), Guid.NewGuid());
        userAchievement.UpdateProgress(5);

        // Act & Assert
        userAchievement.GetProgressPercentage(10).Should().Be(50);
        userAchievement.GetProgressPercentage(20).Should().Be(25);
        userAchievement.GetProgressPercentage(5).Should().Be(100);
    }
}
