using FluentAssertions;
using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class StreakProtectionTests
{
    [Fact]
    public void StreakProtection_Create_SetsDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var protection = StreakProtection.Create(userId);

        // Assert
        protection.UserId.Should().Be(userId);
        protection.ShieldsRemaining.Should().Be(0);
        protection.FreezeUsedThisWeek.Should().BeFalse();
        protection.LastShieldActivatedAt.Should().BeNull();
        protection.IsShieldActive.Should().BeFalse();
        protection.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void StreakProtection_ActivateShield_WithAvailableShield_SetsIsShieldActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(1);

        // Act
        var result = protection.ActivateShield();

        // Assert
        result.Should().BeTrue();
        protection.IsShieldActive.Should().BeTrue();
        protection.ShieldsRemaining.Should().Be(0);
        protection.LastShieldActivatedAt.Should().NotBeNull();
    }

    [Fact]
    public void StreakProtection_ActivateShield_NoShieldsRemaining_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);

        // Act
        var result = protection.ActivateShield();

        // Assert
        result.Should().BeFalse();
        protection.IsShieldActive.Should().BeFalse();
    }

    [Fact]
    public void StreakProtection_ActivateShield_AlreadyActive_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(2);
        protection.ActivateShield();

        // Act
        var result = protection.ActivateShield();

        // Assert
        result.Should().BeFalse();
        protection.ShieldsRemaining.Should().Be(1);
    }

    [Fact]
    public void StreakProtection_AddShields_IncreasesCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);

        // Act
        protection.AddShields(3);

        // Assert
        protection.ShieldsRemaining.Should().Be(3);
    }

    [Fact]
    public void StreakProtection_UseFreeze_SetsFreezeUsedThisWeek()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);

        // Act
        protection.UseFreeze();

        // Assert
        protection.FreezeUsedThisWeek.Should().BeTrue();
    }

    [Fact]
    public void StreakProtection_CanUseFreeze_NotUsedThisWeek_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);

        // Act
        var result = protection.CanUseFreeze();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void StreakProtection_CanUseFreeze_AlreadyUsedThisWeek_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.UseFreeze();

        // Act
        var result = protection.CanUseFreeze();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void StreakProtection_ResetWeeklyFreeze_ClearsFreezeUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.UseFreeze();

        // Act
        protection.ResetWeeklyFreeze();

        // Assert
        protection.FreezeUsedThisWeek.Should().BeFalse();
    }

    [Fact]
    public void StreakProtection_DeactivateShield_ClearsShieldActive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(1);
        protection.ActivateShield();

        // Act
        protection.DeactivateShield();

        // Assert
        protection.IsShieldActive.Should().BeFalse();
    }

    [Fact]
    public void StreakProtection_RemoveShields_DecreasesCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(5);

        // Act
        protection.RemoveShields(2);

        // Assert
        protection.ShieldsRemaining.Should().Be(3);
    }

    [Fact]
    public void StreakProtection_RemoveShields_CannotGoBelowZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(2);

        // Act
        protection.RemoveShields(5);

        // Assert
        protection.ShieldsRemaining.Should().Be(0);
    }
}
