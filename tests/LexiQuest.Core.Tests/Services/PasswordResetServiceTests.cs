using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class PasswordResetServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IStringLocalizer<PasswordResetService> _localizer;
    private readonly PasswordResetService _sut;

    public PasswordResetServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenRepository = Substitute.For<IPasswordResetTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _emailService = Substitute.For<IEmailService>();
        _passwordHasher = Substitute.For<IPasswordHasher<User>>();
        _localizer = Substitute.For<IStringLocalizer<PasswordResetService>>();
        
        _localizer["Error.InvalidToken"].Returns(new LocalizedString("Error.InvalidToken", "Neplatný token"));
        _localizer["Error.ExpiredToken"].Returns(new LocalizedString("Error.ExpiredToken", "Token vypršel"));
        _localizer["Error.UsedToken"].Returns(new LocalizedString("Error.UsedToken", "Token již byl použit"));
        _localizer["Error.SamePassword"].Returns(new LocalizedString("Error.SamePassword", "Nové heslo nesmí být stejné jako staré"));

        _sut = new PasswordResetService(
            _userRepository, 
            _tokenRepository, 
            _unitOfWork, 
            _emailService, 
            _passwordHasher,
            _localizer);
    }

    [Fact]
    public async Task PasswordResetService_RequestReset_ValidEmail_GeneratesToken()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "testuser");
        user.SetId(Guid.NewGuid());
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.RequestResetAsync(new RequestPasswordResetDto { Email = email });

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tokenRepository.Received(1).AddAsync(Arg.Any<PasswordResetToken>(), Arg.Any<CancellationToken>());
        await _emailService.Received(1).SendPasswordResetEmailAsync(email, Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PasswordResetService_RequestReset_InvalidEmail_Returns200OK()
    {
        // Arrange
        var email = "nonexistent@example.com";
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.RequestResetAsync(new RequestPasswordResetDto { Email = email });

        // Assert
        result.IsSuccess.Should().BeTrue(); // Neodhalujeme existenci emailu
        await _emailService.DidNotReceive().SendPasswordResetEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PasswordResetService_RequestReset_TokenExpires_In1Hour()
    {
        // Arrange
        var email = "test@example.com";
        var user = User.Create(email, "testuser");
        user.SetId(Guid.NewGuid());
        _userRepository.GetByEmailAsync(email, Arg.Any<CancellationToken>()).Returns(user);

        PasswordResetToken? capturedToken = null;
        await _tokenRepository.AddAsync(Arg.Do<PasswordResetToken>(t => capturedToken = t), Arg.Any<CancellationToken>());

        // Act
        await _sut.RequestResetAsync(new RequestPasswordResetDto { Email = email });

        // Assert
        capturedToken.Should().NotBeNull();
        capturedToken!.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PasswordResetService_ResetPassword_ValidToken_ChangesPassword()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "testuser");
        user.SetId(userId);
        user.SetPasswordHash("old_hash");
        
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "valid_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _tokenRepository.GetByTokenAsync("valid_token", Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.HashPassword(Arg.Any<User>(), "NewPassword1!").Returns("new_hash");

        // Act
        var result = await _sut.ResetPasswordAsync(new ResetPasswordDto 
        { 
            Token = "valid_token", 
            NewPassword = "NewPassword1!", 
            ConfirmPassword = "NewPassword1!" 
        });

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hash");
        token.UsedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PasswordResetService_ResetPassword_ExpiredToken_Returns400()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "expired_token",
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired
            UsedAt = null
        };

        _tokenRepository.GetByTokenAsync("expired_token", Arg.Any<CancellationToken>()).Returns(token);

        // Act
        var result = await _sut.ResetPasswordAsync(new ResetPasswordDto 
        { 
            Token = "expired_token", 
            NewPassword = "NewPassword1!", 
            ConfirmPassword = "NewPassword1!" 
        });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Token.Expired");
    }

    [Fact]
    public async Task PasswordResetService_ResetPassword_UsedToken_Returns400()
    {
        // Arrange
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "used_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = DateTime.UtcNow.AddHours(-1), // Already used
            IsUsed = true
        };

        _tokenRepository.GetByTokenAsync("used_token", Arg.Any<CancellationToken>()).Returns(token);

        // Act
        var result = await _sut.ResetPasswordAsync(new ResetPasswordDto 
        { 
            Token = "used_token", 
            NewPassword = "NewPassword1!", 
            ConfirmPassword = "NewPassword1!" 
        });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Token.Used");
    }

    [Fact]
    public async Task PasswordResetService_ResetPassword_InvalidToken_Returns400()
    {
        // Arrange
        _tokenRepository.GetByTokenAsync("invalid_token", Arg.Any<CancellationToken>()).Returns((PasswordResetToken?)null);

        // Act
        var result = await _sut.ResetPasswordAsync(new ResetPasswordDto 
        { 
            Token = "invalid_token", 
            NewPassword = "NewPassword1!", 
            ConfirmPassword = "NewPassword1!" 
        });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Token.Invalid");
    }

    [Fact]
    public async Task PasswordResetService_ResetPassword_SameAsOld_Returns400()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "testuser");
        user.SetId(userId);
        user.SetPasswordHash("old_hash");
        
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "valid_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            UsedAt = null
        };

        _tokenRepository.GetByTokenAsync("valid_token", Arg.Any<CancellationToken>()).Returns(token);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), "old_hash", "SamePassword1!").Returns(PasswordVerificationResult.Success);

        // Act
        var result = await _sut.ResetPasswordAsync(new ResetPasswordDto 
        { 
            Token = "valid_token", 
            NewPassword = "SamePassword1!", 
            ConfirmPassword = "SamePassword1!" 
        });

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Password.SameAsOld");
    }
}
