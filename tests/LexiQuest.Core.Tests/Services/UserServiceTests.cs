using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<UserService> _localizer;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _localizer = Substitute.For<IStringLocalizer<UserService>>();
        _passwordHasher = Substitute.For<IPasswordHasher<User>>();
        _tokenService = Substitute.For<ITokenService>();
        
        _localizer["Error.EmailAlreadyExists"].Returns(new LocalizedString("Error.EmailAlreadyExists", "Tento email je již registrován"));
        _localizer["Error.UsernameAlreadyExists"].Returns(new LocalizedString("Error.UsernameAlreadyExists", "Toto uživatelské jméno je již obsazeno"));

        _passwordHasher.HashPassword(Arg.Any<User>(), Arg.Any<string>())
            .Returns((callInfo) => "hashed_" + callInfo.ArgAt<string>(1));

        _tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("test-access-token");
        _tokenService.GenerateRefreshToken().Returns("test-refresh-token");

        _sut = new UserService(_userRepository, _unitOfWork, _passwordHasher, _localizer, _tokenService);
    }

    [Fact]
    public async Task UserService_Register_ValidData_CreatesUser()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Username, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.User.Email.Should().Be(request.Email);
        result.Value.User.Username.Should().Be(request.Username);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserService_Register_DuplicateEmail_Returns409Conflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "exists@example.com",
            Username = "newuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns(User.Create(request.Email, "existing"));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Email.AlreadyExists");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserService_Register_DuplicateUsername_Returns409Conflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "new@example.com",
            Username = "existsuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Username, Arg.Any<CancellationToken>()).Returns(User.Create("existing@test.com", request.Username));

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Username.AlreadyExists");
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserService_Register_InitializesDefaultStats()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Username, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.User.Should().NotBeNull();
        // Ověříme že User byl vytvořen s defaultními hodnotami
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => 
                u.Email == request.Email && 
                u.Username == request.Username &&
                u.Stats != null &&
                u.Streak != null &&
                u.Preferences != null &&
                u.Premium != null), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UserService_Register_InitializesDefaultStreak()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Username, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Streak.CurrentDays == 0), 
            Arg.Any<CancellationToken>());
    }

    // Note: Token generation is handled by LoginService, not UserService

    [Fact]
    public async Task UserService_Register_HashesPassword()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Username = "testuser",
            Password = "Strong1!Pass",
            ConfirmPassword = "Strong1!Pass",
            AcceptTerms = true
        };

        _userRepository.GetByEmailAsync(request.Email, Arg.Any<CancellationToken>()).Returns((User?)null);
        _userRepository.GetByUsernameAsync(request.Username, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _passwordHasher.Received(1).HashPassword(Arg.Any<User>(), "Strong1!Pass");
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.PasswordHash == "hashed_Strong1!Pass"), 
            Arg.Any<CancellationToken>());
    }
}
