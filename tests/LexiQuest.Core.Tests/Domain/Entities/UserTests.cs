using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class UserTests
{
    private static User CreateUser() => User.Create("test@example.com", "testuser");

    // --- Create ---

    [Fact]
    public void Create_SetsDefaultProperties()
    {
        var user = CreateUser();

        user.Email.Should().Be("test@example.com");
        user.Username.Should().Be("testuser");
        user.Id.Should().NotBeEmpty();
        user.LivesRemaining.Should().Be(5);
        user.MaxLives.Should().Be(5);
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
        user.CoinBalance.Should().Be(0);
        user.AvatarUrl.Should().BeNull();
        user.Stats.Should().NotBeNull();
        user.Preferences.Should().NotBeNull();
        user.Streak.Should().NotBeNull();
        user.Premium.Should().NotBeNull();
        user.Privacy.Should().NotBeNull();
        user.CoinTransactions.Should().BeEmpty();
    }

    // --- IncrementFailedLoginAttempts ---

    [Fact]
    public void IncrementFailedLoginAttempts_UnderThreshold_DoesNotLockOut()
    {
        var user = CreateUser();

        for (int i = 0; i < 4; i++)
            user.IncrementFailedLoginAttempts();

        user.FailedLoginAttempts.Should().Be(4);
        user.LockoutEnd.Should().BeNull();
    }

    [Fact]
    public void IncrementFailedLoginAttempts_AtThreshold_LocksAccountFor15Minutes()
    {
        var user = CreateUser();
        var before = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
            user.IncrementFailedLoginAttempts();

        user.FailedLoginAttempts.Should().Be(5);
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd.Should().BeAfter(before.AddMinutes(14));
        user.LockoutEnd.Should().BeBefore(before.AddMinutes(16));
    }

    [Fact]
    public void IncrementFailedLoginAttempts_BeyondThreshold_KeepsLocked()
    {
        var user = CreateUser();

        for (int i = 0; i < 7; i++)
            user.IncrementFailedLoginAttempts();

        user.FailedLoginAttempts.Should().Be(7);
        user.LockoutEnd.Should().NotBeNull();
    }

    // --- RecordSuccessfulLogin ---

    [Fact]
    public void RecordSuccessfulLogin_ClearsFailedAttemptsAndLockout()
    {
        var user = CreateUser();
        for (int i = 0; i < 5; i++)
            user.IncrementFailedLoginAttempts();

        user.RecordSuccessfulLogin();

        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void RecordSuccessfulLogin_SetsLastLoginAt()
    {
        var user = CreateUser();

        user.RecordSuccessfulLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // --- IsLockedOut ---

    [Fact]
    public void IsLockedOut_NoLockout_ReturnsFalse()
    {
        var user = CreateUser();

        user.IsLockedOut().Should().BeFalse();
    }

    [Fact]
    public void IsLockedOut_ActiveLockout_ReturnsTrue()
    {
        var user = CreateUser();
        user.LockAccountUntil(DateTime.UtcNow.AddMinutes(15));

        user.IsLockedOut().Should().BeTrue();
    }

    [Fact]
    public void IsLockedOut_ExpiredLockout_ReturnsFalseAndClearsLockout()
    {
        var user = CreateUser();
        user.LockAccountUntil(DateTime.UtcNow.AddMinutes(-1));

        user.IsLockedOut().Should().BeFalse();
        user.LockoutEnd.Should().BeNull();
    }

    // --- LoseLife ---

    [Fact]
    public void LoseLife_DecrementsLives()
    {
        var user = CreateUser();

        user.LoseLife();

        user.LivesRemaining.Should().Be(4);
    }

    [Fact]
    public void LoseLife_AtZeroLives_DoesNotGoNegative()
    {
        var user = CreateUser();
        user.SetLives(0, 5);

        user.LoseLife();

        user.LivesRemaining.Should().Be(0);
    }

    [Fact]
    public void LoseLife_SetsLastLifeLostAt()
    {
        var user = CreateUser();

        user.LoseLife();

        user.LastLifeLostAt.Should().NotBeNull();
        user.LastLifeLostAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void LoseLife_MultipleTimes_DecrementsCorrectly()
    {
        var user = CreateUser();

        user.LoseLife();
        user.LoseLife();
        user.LoseLife();

        user.LivesRemaining.Should().Be(2);
    }

    // --- RegenerateLife ---

    [Fact]
    public void RegenerateLife_IncrementsLives()
    {
        var user = CreateUser();
        user.SetLives(3, 5);

        user.RegenerateLife();

        user.LivesRemaining.Should().Be(4);
    }

    [Fact]
    public void RegenerateLife_AtMaxLives_DoesNotIncrement()
    {
        var user = CreateUser();

        user.RegenerateLife();

        user.LivesRemaining.Should().Be(5);
    }

    [Fact]
    public void RegenerateLife_ReachingMax_ClearsNextLifeRegenAt()
    {
        var user = CreateUser();
        user.SetLives(4, 5);
        user.SetNextLifeRegenAt(DateTime.UtcNow.AddMinutes(20));

        user.RegenerateLife();

        user.LivesRemaining.Should().Be(5);
        user.NextLifeRegenAt.Should().BeNull();
    }

    [Fact]
    public void RegenerateLife_NotYetMax_SetsNextLifeRegenAt()
    {
        var user = CreateUser();
        user.SetLives(2, 5);

        user.RegenerateLife();

        user.LivesRemaining.Should().Be(3);
        user.NextLifeRegenAt.Should().NotBeNull();
    }

    // --- RefillLives ---

    [Fact]
    public void RefillLives_RestoresToMax()
    {
        var user = CreateUser();
        user.SetLives(1, 5);
        user.SetNextLifeRegenAt(DateTime.UtcNow.AddMinutes(20));

        user.RefillLives();

        user.LivesRemaining.Should().Be(5);
        user.NextLifeRegenAt.Should().BeNull();
    }

    [Fact]
    public void RefillLives_AlreadyFull_RemainsAtMax()
    {
        var user = CreateUser();

        user.RefillLives();

        user.LivesRemaining.Should().Be(5);
        user.NextLifeRegenAt.Should().BeNull();
    }

    // --- AddCoins ---

    [Fact]
    public void AddCoins_PositiveAmount_IncreasesBalance()
    {
        var user = CreateUser();

        user.AddCoins(100);

        user.CoinBalance.Should().Be(100);
    }

    [Fact]
    public void AddCoins_MultipleAdds_Accumulates()
    {
        var user = CreateUser();

        user.AddCoins(50);
        user.AddCoins(30);

        user.CoinBalance.Should().Be(80);
    }

    [Fact]
    public void AddCoins_ZeroAmount_ThrowsArgumentException()
    {
        var user = CreateUser();

        var act = () => user.AddCoins(0);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Fact]
    public void AddCoins_NegativeAmount_ThrowsArgumentException()
    {
        var user = CreateUser();

        var act = () => user.AddCoins(-10);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    // --- SpendCoins ---

    [Fact]
    public void SpendCoins_SufficientBalance_DeductsAndReturnsTrue()
    {
        var user = CreateUser();
        user.AddCoins(100);

        var result = user.SpendCoins(40);

        result.Should().BeTrue();
        user.CoinBalance.Should().Be(60);
    }

    [Fact]
    public void SpendCoins_InsufficientBalance_ReturnsFalseNoChange()
    {
        var user = CreateUser();
        user.AddCoins(10);

        var result = user.SpendCoins(50);

        result.Should().BeFalse();
        user.CoinBalance.Should().Be(10);
    }

    [Fact]
    public void SpendCoins_ExactBalance_DeductsToZero()
    {
        var user = CreateUser();
        user.AddCoins(100);

        var result = user.SpendCoins(100);

        result.Should().BeTrue();
        user.CoinBalance.Should().Be(0);
    }

    [Fact]
    public void SpendCoins_ZeroAmount_ThrowsArgumentException()
    {
        var user = CreateUser();

        var act = () => user.SpendCoins(0);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Fact]
    public void SpendCoins_NegativeAmount_ThrowsArgumentException()
    {
        var user = CreateUser();

        var act = () => user.SpendCoins(-5);

        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    // --- AddCoinTransaction ---

    [Fact]
    public void AddCoinTransaction_PositiveAmount_AddsCoinsAndTransaction()
    {
        var user = CreateUser();

        var tx = user.AddCoinTransaction(50, "reward", "Daily reward");

        user.CoinBalance.Should().Be(50);
        user.CoinTransactions.Should().HaveCount(1);
        tx.Amount.Should().Be(50);
        tx.Type.Should().Be("reward");
        tx.Description.Should().Be("Daily reward");
        tx.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void AddCoinTransaction_NegativeAmount_SpendsCoins()
    {
        var user = CreateUser();
        user.AddCoins(100);

        var tx = user.AddCoinTransaction(-30, "purchase", "Bought item");

        user.CoinBalance.Should().Be(70);
        user.CoinTransactions.Should().HaveCount(1);
        tx.Amount.Should().Be(-30);
    }

    // --- UpdateProfile ---

    [Fact]
    public void UpdateProfile_ChangesUsernameAndEmail()
    {
        var user = CreateUser();

        user.UpdateProfile("newuser", "new@example.com");

        user.Username.Should().Be("newuser");
        user.Email.Should().Be("new@example.com");
        user.UpdatedAt.Should().NotBeNull();
    }

    // --- ChangePassword ---

    [Fact]
    public void ChangePassword_SetsNewHash()
    {
        var user = CreateUser();

        user.ChangePassword("newhash123");

        user.PasswordHash.Should().Be("newhash123");
        user.UpdatedAt.Should().NotBeNull();
    }

    // --- UpdateAvatar ---

    [Fact]
    public void UpdateAvatar_SetsUrl()
    {
        var user = CreateUser();

        user.UpdateAvatar("https://example.com/avatar.png");

        user.AvatarUrl.Should().Be("https://example.com/avatar.png");
        user.UpdatedAt.Should().NotBeNull();
    }

    // --- UpdatePreferences ---

    [Fact]
    public void UpdatePreferences_ReplacesPreferences()
    {
        var user = CreateUser();
        var prefs = UserPreferences.CreateDefault();

        user.UpdatePreferences(prefs);

        user.Preferences.Should().BeSameAs(prefs);
        user.UpdatedAt.Should().NotBeNull();
    }

    // --- UpdatePrivacySettings ---

    [Fact]
    public void UpdatePrivacySettings_ReplacesSettings()
    {
        var user = CreateUser();
        var privacy = PrivacySettings.CreateDefault();

        user.UpdatePrivacySettings(privacy);

        user.Privacy.Should().BeSameAs(privacy);
        user.UpdatedAt.Should().NotBeNull();
    }

    // --- ResetFailedLoginAttempts ---

    [Fact]
    public void ResetFailedLoginAttempts_ResetsToZero()
    {
        var user = CreateUser();
        user.IncrementFailedLoginAttempts();
        user.IncrementFailedLoginAttempts();

        user.ResetFailedLoginAttempts();

        user.FailedLoginAttempts.Should().Be(0);
    }

    // --- SetLives / ResetLives ---

    [Fact]
    public void SetLives_SetsCurrentAndMax()
    {
        var user = CreateUser();

        user.SetLives(3, 7);

        user.LivesRemaining.Should().Be(3);
        user.MaxLives.Should().Be(7);
    }

    // --- UpdateEmail / UpdateUsername ---

    [Fact]
    public void UpdateEmail_SetsEmailAndUpdatedAt()
    {
        var user = CreateUser();

        user.UpdateEmail("changed@example.com");

        user.Email.Should().Be("changed@example.com");
        user.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateUsername_SetsUsernameAndUpdatedAt()
    {
        var user = CreateUser();

        user.UpdateUsername("changed");

        user.Username.Should().Be("changed");
        user.UpdatedAt.Should().NotBeNull();
    }

    // --- SetStripeCustomerId ---

    [Fact]
    public void SetStripeCustomerId_SetsValue()
    {
        var user = CreateUser();

        user.SetStripeCustomerId("cus_abc123");

        user.StripeCustomerId.Should().Be("cus_abc123");
    }

    // --- ScheduleNextRegen ---

    [Fact]
    public void ScheduleNextRegen_BelowMax_SetsTime()
    {
        var user = CreateUser();
        user.SetLives(3, 5);

        user.ScheduleNextRegen(20);

        user.NextLifeRegenAt.Should().NotBeNull();
        user.NextLifeRegenAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(20), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void ScheduleNextRegen_AtMax_DoesNotSet()
    {
        var user = CreateUser();

        user.ScheduleNextRegen(20);

        user.NextLifeRegenAt.Should().BeNull();
    }
}
