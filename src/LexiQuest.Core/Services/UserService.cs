using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IStringLocalizer<UserService> _localizer;
    private readonly ITokenService _tokenService;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher,
        IStringLocalizer<UserService> localizer,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _localizer = localizer;
        _tokenService = tokenService;
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return null;

        return MapToProfileDto(user);
    }

    public async Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        // Check if username is taken by another user
        if (!await IsUsernameAvailableAsync(request.Username, userId, cancellationToken))
        {
            throw new InvalidOperationException(_localizer["Error.UsernameTaken"]);
        }

        user.UpdateProfile(request.Username, request.Email);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        // Verify current password
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException(_localizer["Error.InvalidCurrentPassword"]);
        }

        // Hash and set new password
        var newPasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.ChangePassword(newPasswordHash);
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdatePreferencesAsync(Guid userId, UserPreferencesDto preferences, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        var userPrefs = UserPreferences.CreateDefault();
        userPrefs.Theme = preferences.Theme;
        userPrefs.Language = preferences.Language;
        userPrefs.AnimationsEnabled = preferences.AnimationsEnabled;
        userPrefs.SoundsEnabled = preferences.SoundsEnabled;
        userPrefs.StreakReminderTime = preferences.StreakReminderTime;
        userPrefs.PushNotificationsEnabled = preferences.PushNotificationsEnabled;
        userPrefs.EmailNotificationsEnabled = preferences.EmailNotificationsEnabled;
        userPrefs.LeagueUpdatesEnabled = preferences.LeagueUpdatesEnabled;
        userPrefs.AchievementNotificationsEnabled = preferences.AchievementNotificationsEnabled;
        userPrefs.DailyChallengeReminderEnabled = preferences.DailyChallengeReminderEnabled;
        user.UpdatePreferences(userPrefs);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdatePrivacySettingsAsync(Guid userId, PrivacySettingsDto privacy, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) return false;

        user.UpdatePrivacySettings(new PrivacySettings
        {
            ProfileVisibility = (LexiQuest.Core.Domain.ValueObjects.ProfileVisibility)privacy.ProfileVisibility,
            LeaderboardVisible = privacy.LeaderboardVisible,
            StatsSharingEnabled = privacy.StatsSharingEnabled
        });

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (existingUser == null) return true;
        
        if (excludeUserId.HasValue && existingUser.Id == excludeUserId.Value) return true;
        
        return false;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUserByEmail != null)
        {
            return Result.Failure<AuthResponse>(new Error("Email.AlreadyExists", _localizer["Error.EmailAlreadyExists"]));
        }

        // Check if username already exists
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUserByUsername != null)
        {
            return Result.Failure<AuthResponse>(new Error("Username.AlreadyExists", _localizer["Error.UsernameAlreadyExists"]));
        }

        // Create new user
        var user = User.Create(request.Email, request.Username);
        var passwordHash = _passwordHasher.HashPassword(user, request.Password);
        user.SetPasswordHash(passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate tokens for auto-login after registration
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return Result.Success(new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CurrentStreak = user.Streak.CurrentDays,
                TotalXP = user.Stats.TotalXP,
                League = user.Stats.League
            }
        });
    }

    private static UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            Stats = new UserStatsDto
            {
                Level = user.Stats?.Level ?? 1,
                TotalXP = user.Stats?.TotalXP ?? 0,
                WordsSolved = user.Stats?.TotalWordsSolved ?? 0,
                CurrentStreak = user.Streak?.CurrentDays ?? 0,
                LongestStreak = user.Streak?.LongestDays ?? 0,
                Accuracy = user.Stats?.Accuracy ?? 0
            },
            Preferences = new UserPreferencesDto
            {
                Theme = user.Preferences?.Theme ?? "light",
                Language = user.Preferences?.Language ?? "cs",
                AnimationsEnabled = user.Preferences?.AnimationsEnabled ?? true,
                SoundsEnabled = user.Preferences?.SoundsEnabled ?? true,
                StreakReminderTime = user.Preferences?.StreakReminderTime,
                PushNotificationsEnabled = user.Preferences?.PushNotificationsEnabled ?? true,
                EmailNotificationsEnabled = user.Preferences?.EmailNotificationsEnabled ?? true,
                LeagueUpdatesEnabled = user.Preferences?.LeagueUpdatesEnabled ?? true,
                AchievementNotificationsEnabled = user.Preferences?.AchievementNotificationsEnabled ?? true,
                DailyChallengeReminderEnabled = user.Preferences?.DailyChallengeReminderEnabled ?? true
            },
            Privacy = new PrivacySettingsDto
            {
                ProfileVisibility = (Shared.DTOs.Users.ProfileVisibility)(user.Privacy?.ProfileVisibility ?? LexiQuest.Core.Domain.ValueObjects.ProfileVisibility.Public),
                LeaderboardVisible = user.Privacy?.LeaderboardVisible ?? true,
                StatsSharingEnabled = user.Privacy?.StatsSharingEnabled ?? true
            }
        };
    }
}
