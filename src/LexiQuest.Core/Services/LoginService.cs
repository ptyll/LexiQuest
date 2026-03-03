using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class LoginService : ILoginService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<LoginService> _localizer;
    private readonly IPasswordHasher<User> _passwordHasher;

    public LoginService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        IStringLocalizer<LoginService> localizer,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
        _passwordHasher = passwordHasher;
    }

    public virtual bool VerifyPassword(User user, string password)
    {
        // This method is virtual to allow mocking in tests
        // In real implementation, we use IPasswordHasher
        return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) 
            == PasswordVerificationResult.Success;
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        // Check if user exists
        if (user == null)
        {
            return Result.Failure<AuthResponse>(new Error(
                "Authentication.InvalidCredentials", 
                _localizer["Error.Login.InvalidCredentials"]));
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            return Result.Failure<AuthResponse>(new Error(
                "Authentication.AccountLocked", 
                _localizer["Error.Login.AccountLocked"]));
        }

        // Verify password
        if (!VerifyPassword(user, request.Password))
        {
            user.IncrementFailedLoginAttempts();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthResponse>(new Error(
                "Authentication.InvalidCredentials", 
                _localizer["Error.Login.InvalidCredentials"]));
        }

        // Record successful login
        user.RecordSuccessfulLogin();

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Check streak warning (< 6 hours remaining until midnight)
        var streakWarning = CheckStreakWarning(user);

        // Create response
        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                CurrentStreak = user.Streak.CurrentDays,
                TotalXP = user.Stats.TotalXP,
                League = user.Stats.League
            },
            StreakWarning = streakWarning
        };

        return Result.Success(response);
    }

    private StreakWarningDto? CheckStreakWarning(User user)
    {
        // If streak is 0, no warning needed
        if (user.Streak.CurrentDays == 0)
        {
            return null;
        }

        // Calculate time until end of day (midnight UTC)
        var now = DateTime.UtcNow;
        var midnight = now.Date.AddDays(1);
        var hoursRemaining = (midnight - now).TotalHours;

        // Warning if less than 6 hours remaining
        if (hoursRemaining < 6)
        {
            return new StreakWarningDto
            {
                HoursRemaining = (int)hoursRemaining,
                Message = _localizer["Warning.StreakExpiring"]
            };
        }

        return null;
    }
}
