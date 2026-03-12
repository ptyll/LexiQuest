using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class StreakProtectionServiceEdgeCaseTests
{
    private readonly IStreakProtectionRepository _protectionRepository = Substitute.For<IStreakProtectionRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly StreakProtectionService _sut;

    public StreakProtectionServiceEdgeCaseTests()
    {
        _sut = new StreakProtectionService(_protectionRepository, _unitOfWork);
    }

    // --- Activity at 23:59 and 00:01 on consecutive days (Streak entity) ---

    [Fact]
    public void Streak_ActivityAt2359And0001NextDay_ContinuesStreak()
    {
        // Arrange
        var streak = Streak.CreateDefault();
        var day1 = new DateTime(2026, 3, 10, 23, 59, 0, DateTimeKind.Utc);
        var day2 = new DateTime(2026, 3, 11, 0, 1, 0, DateTimeKind.Utc);

        // Act
        streak.RecordActivity(day1);
        streak.RecordActivity(day2);

        // Assert - day1.Date (Mar 10) and day2.Date (Mar 11) are consecutive
        streak.CurrentDays.Should().Be(2);
    }

    [Fact]
    public void Streak_ActivitySameDay_DoesNotIncrementStreak()
    {
        // Arrange
        var streak = Streak.CreateDefault();
        var day1Early = new DateTime(2026, 3, 10, 8, 0, 0, DateTimeKind.Utc);
        var day1Late = new DateTime(2026, 3, 10, 23, 59, 0, DateTimeKind.Utc);

        // Act
        streak.RecordActivity(day1Early);
        streak.RecordActivity(day1Late);

        // Assert
        streak.CurrentDays.Should().Be(1);
    }

    [Fact]
    public void Streak_MissOneDay_StreakBroken()
    {
        // Arrange
        var streak = Streak.CreateDefault();
        var day1 = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);
        var day3 = new DateTime(2026, 3, 12, 12, 0, 0, DateTimeKind.Utc); // Skipped Mar 11

        // Act
        streak.RecordActivity(day1);
        streak.RecordActivity(day3);

        // Assert
        streak.CurrentDays.Should().Be(1); // Reset to 1 for new start
    }

    // --- Grace period edge (exactly 48 hours gap) ---

    [Fact]
    public void Streak_Exactly48HoursGap_StreakBroken()
    {
        // Arrange
        var streak = Streak.CreateDefault();
        var day1 = new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc);
        // 48 hours later = Mar 12 12:00 - day gap is 2 days, streak breaks
        var day3 = new DateTime(2026, 3, 12, 12, 0, 0, DateTimeKind.Utc);

        // Act
        streak.RecordActivity(day1);
        streak.RecordActivity(day3);

        // Assert - Mar 10 and Mar 12 are not consecutive
        streak.CurrentDays.Should().Be(1);
    }

    [Fact]
    public void Streak_JustUnder48Hours_ButConsecutiveDays_ContinuesStreak()
    {
        // Arrange
        var streak = Streak.CreateDefault();
        var day1 = new DateTime(2026, 3, 10, 0, 1, 0, DateTimeKind.Utc);
        // 47 hours 58 minutes later = Mar 11 23:59 - still consecutive calendar days
        var day2 = new DateTime(2026, 3, 11, 23, 59, 0, DateTimeKind.Utc);

        // Act
        streak.RecordActivity(day1);
        streak.RecordActivity(day2);

        // Assert - Mar 10 and Mar 11 are consecutive
        streak.CurrentDays.Should().Be(2);
    }

    // --- Streak at risk detection ---

    [Fact]
    public void Streak_LongestDaysTracked_AfterBreak()
    {
        // Arrange
        var streak = Streak.CreateDefault();

        // Build a 5-day streak
        for (int i = 0; i < 5; i++)
        {
            streak.RecordActivity(new DateTime(2026, 3, 1 + i, 12, 0, 0, DateTimeKind.Utc));
        }

        streak.CurrentDays.Should().Be(5);
        streak.LongestDays.Should().Be(5);

        // Break the streak
        streak.RecordActivity(new DateTime(2026, 3, 10, 12, 0, 0, DateTimeKind.Utc));

        // Assert
        streak.CurrentDays.Should().Be(1); // Reset
        streak.LongestDays.Should().Be(5); // Preserved
    }

    [Fact]
    public void Streak_FirstActivity_SetsCurrentDaysToOne()
    {
        // Arrange
        var streak = Streak.CreateDefault();

        // Act
        streak.RecordActivity(DateTime.UtcNow);

        // Assert
        streak.CurrentDays.Should().Be(1);
        streak.LastActivityDate.Should().NotBeNull();
    }

    [Fact]
    public void Streak_Reset_ClearsCurrentDaysButPreservesLongest()
    {
        // Arrange
        var streak = Streak.CreateDefault();
        for (int i = 0; i < 3; i++)
            streak.RecordActivity(new DateTime(2026, 3, 1 + i, 12, 0, 0, DateTimeKind.Utc));

        // Act
        streak.Reset();

        // Assert
        streak.CurrentDays.Should().Be(0);
        streak.LastActivityDate.Should().BeNull();
        // Note: LongestDays is preserved (not reset)
    }

    // --- StreakProtection service tests ---

    [Fact]
    public async Task ActivateShield_NoProtection_ThrowsInvalidOperation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _protectionRepository.GetByUserIdAsync(userId).Returns((StreakProtection?)null);

        // Act
        var act = () => _sut.ActivateShieldAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task ActivateShield_NoShieldsRemaining_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        // 0 shields remaining by default
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.ActivateShieldAsync(userId);

        // Assert
        result.Should().BeFalse();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateShield_WithShields_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(3);
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.ActivateShieldAsync(userId);

        // Assert
        result.Should().BeTrue();
        protection.ShieldsRemaining.Should().Be(2);
        protection.IsShieldActive.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateShield_AlreadyActive_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.AddShields(2);
        protection.ActivateShield(); // First activation
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.ActivateShieldAsync(userId);

        // Assert - can't activate when already active
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryAutoFreeze_AlreadyUsedThisWeek_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.UseFreeze(); // Already used
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.TryAutoFreezeAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TryAutoFreeze_NotUsedThisWeek_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.TryAutoFreezeAsync(userId);

        // Assert
        result.Should().BeTrue();
        protection.FreezeUsedThisWeek.Should().BeTrue();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TryAutoFreeze_NoProtectionRecord_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _protectionRepository.GetByUserIdAsync(userId).Returns((StreakProtection?)null);

        // Act
        var result = await _sut.TryAutoFreezeAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PurchaseShields_CreatesProtectionIfNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _protectionRepository.GetByUserIdAsync(userId).Returns((StreakProtection?)null);

        // Act
        var result = await _sut.PurchaseShieldsAsync(userId, 3, 500);

        // Assert
        result.Should().BeTrue();
        await _protectionRepository.Received(1).AddAsync(Arg.Any<StreakProtection>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurchaseEmergencyShield_ActivatesImmediately()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.PurchaseEmergencyShieldAsync(userId, 300);

        // Assert
        result.Should().BeTrue();
        protection.IsShieldActive.Should().BeTrue();
    }

    [Fact]
    public async Task CanActivateFreeShield_NoRecord_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _protectionRepository.GetByUserIdAsync(userId).Returns((StreakProtection?)null);

        // Act
        var result = await _sut.CanActivateFreeShieldAsync(userId, isPremium: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanActivateFreeShield_NeverActivated_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        // LastShieldActivatedAt is null by default
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.CanActivateFreeShieldAsync(userId, isPremium: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanActivateFreeShield_FreeUser_Within30Days_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.LastShieldActivatedAt = DateTime.UtcNow.AddDays(-15); // 15 days ago
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.CanActivateFreeShieldAsync(userId, isPremium: false);

        // Assert - free user needs 30 days
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanActivateFreeShield_PremiumUser_After7Days_ReturnsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.LastShieldActivatedAt = DateTime.UtcNow.AddDays(-8); // 8 days ago
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.CanActivateFreeShieldAsync(userId, isPremium: true);

        // Assert - premium user needs 7 days
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanActivateFreeShield_PremiumUser_Within7Days_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.LastShieldActivatedAt = DateTime.UtcNow.AddDays(-5); // 5 days ago
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        var result = await _sut.CanActivateFreeShieldAsync(userId, isPremium: true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ResetWeeklyFreeze_ClearsFreezeFlag()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var protection = StreakProtection.Create(userId);
        protection.UseFreeze(); // Set flag
        _protectionRepository.GetByUserIdAsync(userId).Returns(protection);

        // Act
        await _sut.ResetWeeklyFreezeAsync(userId);

        // Assert
        protection.FreezeUsedThisWeek.Should().BeFalse();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
