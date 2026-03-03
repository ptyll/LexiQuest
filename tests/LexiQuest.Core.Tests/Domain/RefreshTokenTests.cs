using FluentAssertions;
using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Tests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void Create_StoresCorrectData()
    {
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var token = RefreshToken.Create("test-token-value", userId, expiresAt);

        token.Id.Should().NotBe(Guid.Empty);
        token.Token.Should().Be("test-token-value");
        token.UserId.Should().Be(userId);
        token.ExpiresAt.Should().Be(expiresAt);
        token.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        token.RevokedAt.Should().BeNull();
        token.ReplacedByToken.Should().BeNull();
        token.IsActive.Should().BeTrue();
        token.IsExpired.Should().BeFalse();
        token.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Revoke_SetsRevokedAt()
    {
        var token = RefreshToken.Create("token", Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        token.Revoke("new-token");

        token.IsRevoked.Should().BeTrue();
        token.IsActive.Should().BeFalse();
        token.RevokedAt.Should().NotBeNull();
        token.ReplacedByToken.Should().Be("new-token");
    }

    [Fact]
    public void IsExpired_ReturnsTrueForExpiredToken()
    {
        var token = RefreshToken.Create("token", Guid.NewGuid(), DateTime.UtcNow.AddSeconds(-1));

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }
}
