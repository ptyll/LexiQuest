using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Models;
using LexiQuest.Shared.DTOs.Auth;
using LexiQuest.Shared.DTOs.Users;

namespace LexiQuest.Core.Interfaces.Services;

/// <summary>
/// Service for user profile and settings management.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets user profile by ID.
    /// </summary>
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates user profile.
    /// </summary>
    Task<bool> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes user password.
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates user preferences.
    /// </summary>
    Task<bool> UpdatePreferencesAsync(Guid userId, UserPreferencesDto preferences, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates privacy settings.
    /// </summary>
    Task<bool> UpdatePrivacySettingsAsync(Guid userId, PrivacySettingsDto privacy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if username is available.
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}
