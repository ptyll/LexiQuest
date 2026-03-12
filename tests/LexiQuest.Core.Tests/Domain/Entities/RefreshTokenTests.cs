using FluentAssertions;
using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class RefreshTokenTests
{
    private static RefreshToken CreateToken(DateTime? expiresAt = null)
    {
        return RefreshToken.Create(
            "test-token-value",
            Guid.NewGuid(),
            expiresAt ?? DateTime.UtcNow.AddDays(7));
    }

    // --- Create ---

    [Fact]
    public void Create_SetsAllProperties()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var token = RefreshToken.Create("my-token", userId, expiresAt);

        token.Id.Should().NotBeEmpty();
        token.Token.Should().Be("my-token");
        token.UserId.Should().Be(userId);
        token.ExpiresAt.Should().Be(expiresAt);
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        token.RevokedAt.Should().BeNull();
        token.ReplacedByToken.Should().BeNull();
    }

    [Fact]
    public void Create_IsActiveByDefault()
    {
        var token = CreateToken();

        token.IsActive.Should().BeTrue();
        token.IsRevoked.Should().BeFalse();
        token.IsExpired.Should().BeFalse();
    }

    // --- Revoke ---

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        var token = CreateToken();

        token.Revoke();

        token.RevokedAt.Should().NotBeNull();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WithReplacementToken_SetsReplacedByToken()
    {
        var token = CreateToken();

        token.Revoke("new-replacement-token");

        token.ReplacedByToken.Should().Be("new-replacement-token");
        token.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public void Revoke_WithoutReplacementToken_ReplacedByTokenIsNull()
    {
        var token = CreateToken();

        token.Revoke();

        token.ReplacedByToken.Should().BeNull();
    }

    // --- IsExpired ---

    [Fact]
    public void IsExpired_FutureExpiry_ReturnsFalse()
    {
        var token = CreateToken(DateTime.UtcNow.AddDays(1));

        token.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_PastExpiry_ReturnsTrue()
    {
        var token = CreateToken(DateTime.UtcNow.AddDays(-1));

        token.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_ExactlyNow_ReturnsTrue()
    {
        // ExpiresAt == UtcNow => IsExpired uses >= so this is true
        var token = CreateToken(DateTime.UtcNow);

        // By the time we check, UtcNow >= ExpiresAt should be true
        token.IsExpired.Should().BeTrue();
    }

    // --- IsRevoked ---

    [Fact]
    public void IsRevoked_NotRevoked_ReturnsFalse()
    {
        var token = CreateToken();

        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void IsRevoked_AfterRevoke_ReturnsTrue()
    {
        var token = CreateToken();

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
    }

    // --- IsActive ---

    [Fact]
    public void IsActive_NotRevokedNotExpired_ReturnsTrue()
    {
        var token = CreateToken(DateTime.UtcNow.AddDays(7));

        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsActive_Revoked_ReturnsFalse()
    {
        var token = CreateToken(DateTime.UtcNow.AddDays(7));

        token.Revoke();

        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_Expired_ReturnsFalse()
    {
        var token = CreateToken(DateTime.UtcNow.AddDays(-1));

        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_RevokedAndExpired_ReturnsFalse()
    {
        var token = CreateToken(DateTime.UtcNow.AddDays(-1));

        token.Revoke();

        token.IsActive.Should().BeFalse();
    }
}
