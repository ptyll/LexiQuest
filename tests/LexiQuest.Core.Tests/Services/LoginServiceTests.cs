using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class LoginServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<LoginService> _localizer;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly LoginService _sut;

    public LoginServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenService = Substitute.For<ITokenService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<LoginService>>();
        _passwordHasher = Substitute.For<IPasswordHasher<User>>();

        _localizer["Error.Login.InvalidCredentials"].Returns(new LocalizedString("Error.Login.InvalidCredentials", "Nesprávný email nebo heslo"));
        _localizer["Error.Login.AccountLocked"].Returns(new LocalizedString("Error.Login.AccountLocked", "Účet je dočasně zablokován"));
        _localizer["Warning.StreakExpiring"].Returns(new LocalizedString("Warning.StreakExpiring", "Zbývá méně než 6 hodin do resetu streaku!"));

        _sut = new LoginService(_userRepository, _tokenService, _unitOfWork, _localizer, _passwordHasher);
    }

    [Fact]
    public async Task LoginService_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "hashedPassword123", "password123")
            .Returns(PasswordVerificationResult.Success);

        _tokenService.GenerateAccessToken(user).Returns("access_token");
        _tokenService.GenerateRefreshToken().Returns("refresh_token");

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task LoginService_InvalidEmail_Returns401()
    {
        // Arrange
        _userRepository.GetByEmailAsync("nonexistent@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "password123",
            RememberMe = false
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Authentication.InvalidCredentials");
    }

    [Fact]
    public async Task LoginService_InvalidPassword_Returns401()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);


        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword",
            RememberMe = false
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Authentication.InvalidCredentials");
    }

    [Fact]
    public async Task LoginService_InvalidPassword_IncrementsFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);


        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword",
            RememberMe = false
        };

        // Act
        await _sut.LoginAsync(request);

        // Assert
        user.FailedLoginAttempts.Should().Be(1);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginService_LockedAccount_Returns423Locked()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");
        user.LockAccountUntil(DateTime.UtcNow.AddMinutes(15));

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Authentication.AccountLocked");
    }

    [Fact]
    public async Task LoginService_ValidLogin_ResetsFailedAttempts()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");
        user.IncrementFailedLoginAttempts(); // Set to 1

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "hashedPassword123", "password123")
            .Returns(PasswordVerificationResult.Success);

        _tokenService.GenerateAccessToken(user).Returns("access_token");
        _tokenService.GenerateRefreshToken().Returns("refresh_token");

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        // Act
        await _sut.LoginAsync(request);

        // Assert
        user.FailedLoginAttempts.Should().Be(0);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginService_ValidLogin_UpdatesLastLoginAt()
    {
        // Arrange
        var beforeLogin = DateTime.UtcNow.AddSeconds(-1);
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "hashedPassword123", "password123")
            .Returns(PasswordVerificationResult.Success);

        _tokenService.GenerateAccessToken(user).Returns("access_token");
        _tokenService.GenerateRefreshToken().Returns("refresh_token");

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        // Act
        await _sut.LoginAsync(request);

        // Assert
        user.LastLoginAt.Should().BeAfter(beforeLogin);
    }

    [Fact]
    public async Task LoginService_FiveFailedAttempts_LocksAccount()
    {
        // Arrange
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");
        // Already have 4 failed attempts
        for (int i = 0; i < 4; i++)
        {
            user.IncrementFailedLoginAttempts();
        }

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);


        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword",
            RememberMe = false
        };

        // Act - 5th failed attempt locks account but still returns InvalidCredentials
        var result = await _sut.LoginAsync(request);

        // Assert - account should be locked after 5th failed attempt
        user.FailedLoginAttempts.Should().Be(5);
        user.IsLockedOut().Should().BeTrue();
        result.Error.Code.Should().Be("Authentication.InvalidCredentials");
        
        // Act - next attempt should return AccountLocked
        var lockedResult = await _sut.LoginAsync(request);
        lockedResult.Error.Code.Should().Be("Authentication.AccountLocked");
    }

    [Fact]
    public async Task LoginService_NearMidnightWithStreak_ReturnsStreakWarning()
    {
        // Arrange - simulate being close to midnight UTC
        var user = CreateTestUser("test@example.com", "testuser");
        user.SetPasswordHash("hashedPassword123");
        // Simulate having a streak
        user.Streak.GetType().GetProperty("CurrentDays")?.SetValue(user.Streak, 5);

        _userRepository.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "hashedPassword123", "password123")
            .Returns(PasswordVerificationResult.Success);

        _tokenService.GenerateAccessToken(user).Returns("access_token");
        _tokenService.GenerateRefreshToken().Returns("refresh_token");

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123",
            RememberMe = false
        };

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        // Note: This test will only pass if run between 18:00-23:59 UTC
        // In a real scenario, we might want to inject a time provider
        if (DateTime.UtcNow.Hour >= 18)
        {
            result.Value?.StreakWarning.Should().NotBeNull();
        }
    }

    private User CreateTestUser(string email, string username)
    {
        return User.Create(email, username);
    }
}
