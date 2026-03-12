using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using Xunit;

namespace LexiQuest.Core.Tests.Domain;

public class StreakProtectionTests
{
    [Fact]
    public void Create_SetsDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var protection = StreakProtection.Create(userId);

        // Assert
        protection.UserId.Should().Be(userId);
        protection.ShieldsRemaining.Should().Be(0);
        protection.IsShieldActive.Should().BeFalse();
        protection.FreezeUsedThisWeek.Should().BeFalse();
        protection.LastShieldActivatedAt.Should().BeNull();
    }

    [Fact]
    public void ActivateShield_WithShieldsRemaining_ActivatesAndReturnsTrue()
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
    public void ActivateShield_NoShieldsRemaining_ReturnsFalse()
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
    public void ActivateShield_AlreadyActive_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(2);
        protection.ActivateShield(); // První aktivace

        // Act
        var result = protection.ActivateShield(); // Druhá pokus - měl by selhat

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DeactivateShield_SetsIsShieldActiveToFalse()
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
    public void AddShields_IncreasesShieldCount()
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
    public void RemoveShields_DecreasesShieldCount()
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
    public void RemoveShields_CannotGoBelowZero()
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

    [Fact]
    public void UseFreeze_SetsFreezeUsedThisWeek()
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
    public void CanUseFreeze_WhenNotUsed_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);

        // Act & Assert
        protection.CanUseFreeze().Should().BeTrue();
    }

    [Fact]
    public void CanUseFreeze_WhenAlreadyUsed_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.UseFreeze();

        // Act & Assert
        protection.CanUseFreeze().Should().BeFalse();
    }

    [Fact]
    public void ResetWeeklyFreeze_ClearsFreezeFlag()
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
}
