using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Infrastructure.Auth;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.Extensions.Options;

namespace LexiQuest.Infrastructure.Tests.Auth;

public class TokenServiceTests
{
    private readonly TokenService _sut;
    private readonly JwtSettings _settings;

    public TokenServiceTests()
    {
        _settings = new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "SuperSecretKeyForTestingThatIsLongEnoughForHS256Algorithm!!",
            AccessTokenExpiryMinutes = 30,
            RefreshTokenExpiryDays = 7
        };
        _sut = new TokenService(Options.Create(_settings));
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        var user = User.Create("test@example.com", "testuser");

        var token = _sut.GenerateAccessToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserClaims()
    {
        var user = User.Create("test@example.com", "testuser");

        var token = _sut.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "testuser");
        jwt.Issuer.Should().Be("TestIssuer");
        jwt.Audiences.Should().Contain("TestAudience");
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueToken()
    {
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateAccessToken_ReturnsTrueForValid()
    {
        var user = User.Create("test@example.com", "testuser");
        var token = _sut.GenerateAccessToken(user);

        var userId = _sut.ValidateAccessToken(token);

        userId.Should().NotBeNull();
        userId.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateAccessToken_ReturnsNullForInvalid()
    {
        var userId = _sut.ValidateAccessToken("invalid-token");

        userId.Should().BeNull();
    }
}
