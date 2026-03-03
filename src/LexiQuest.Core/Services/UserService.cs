using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<UserService> _localizer;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IStringLocalizer<UserService> localizer,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            return Result.Failure<AuthResponse>(new Error("Email.AlreadyExists", _localizer["Error.Email.Exists"]));
        }

        // Check if username already exists
        if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
        {
            return Result.Failure<AuthResponse>(new Error("Username.AlreadyExists", _localizer["Error.Username.Exists"]));
        }

        // Create user with default stats
        var user = User.Create(request.Email, request.Username);

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(passwordHash);

        // Add user to repository
        await _userRepository.AddAsync(user, cancellationToken);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create response
        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1), // Default expiration
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CurrentStreak = user.Streak.CurrentDays,
                TotalXP = user.Stats.TotalXP,
                League = user.Stats.League
            }
        };

        return Result.Success(response);
    }
}
