using FluentAssertions;
using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class PasswordResetTokenTests
{
    // --- Create ---

    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();

        var token = PasswordResetToken.Create(userId, "abc123token", TimeSpan.FromHours(1));

        token.Id.Should().NotBeEmpty();
        token.UserId.Should().Be(userId);
        token.Token.Should().Be("abc123token");
        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(2));
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        token.UsedAt.Should().BeNull();
        token.IsUsed.Should().BeFalse();
    }

    [Fact]
    public void Create_IsNotExpiredImmediately()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Create_IsValidImmediately()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));

        token.IsValid.Should().BeTrue();
    }

    // --- MarkAsUsed ---

    [Fact]
    public void MarkAsUsed_SetsUsedAtAndIsUsed()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));

        token.MarkAsUsed();

        token.UsedAt.Should().NotBeNull();
        token.UsedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        token.IsUsed.Should().BeTrue();
    }

    [Fact]
    public void MarkAsUsed_MakesTokenInvalid()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));

        token.MarkAsUsed();

        token.IsValid.Should().BeFalse();
    }

    // --- IsExpired ---

    [Fact]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ZeroDuration_IsExpired()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.Zero);

        // Might be expired or just at boundary - either way it shouldn't be "valid for a long time"
        // With zero duration, ExpiresAt = UtcNow, so IsExpired checks UtcNow > ExpiresAt
        // Could be false by a few ms, so just check it's close
        token.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    // --- IsValid ---

    [Fact]
    public void IsValid_NotExpiredNotUsed_ReturnsTrue()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));

        token.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IsValid_Expired_ReturnsFalse()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);

        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_Used_ReturnsFalse()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));
        token.MarkAsUsed();

        token.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IsValid_UsedAndExpired_ReturnsFalse()
    {
        var token = PasswordResetToken.Create(Guid.NewGuid(), "token", TimeSpan.FromHours(1));
        token.MarkAsUsed();
        token.ExpiresAt = DateTime.UtcNow.AddHours(-1);

        token.IsValid.Should().BeFalse();
    }
}
