using LexiQuest.Shared.DTOs.Users;

namespace LexiQuest.Blazor.Services;

/// <summary>
/// Service for user profile and settings management on the frontend.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets current user profile.
    /// </summary>
    Task<UserProfileDto?> GetProfileAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates user profile.
    /// </summary>
    Task<bool> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Changes user password.
    /// </summary>
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates user preferences.
    /// </summary>
    Task<bool> UpdatePreferencesAsync(UserPreferencesDto preferences, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates privacy settings.
    /// </summary>
    Task<bool> UpdatePrivacyAsync(PrivacySettingsDto privacy, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if username is available.
    /// </summary>
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);
}
