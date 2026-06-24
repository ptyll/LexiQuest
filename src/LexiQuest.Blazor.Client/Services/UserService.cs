using System.Net;
using System.Net.Http.Json;
using LexiQuest.Shared.DTOs.Users;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Blazor.Services;

public class UserService : IUserService
{
    private readonly IAuthenticatedApiClient _apiClient;
    private readonly IStringLocalizer<UserService> _localizer;

    public UserService(IAuthenticatedApiClient apiClient, IStringLocalizer<UserService> localizer)
    {
        _apiClient = apiClient;
        _localizer = localizer;
    }

    public async Task<UserProfileDto?> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync("api/v1/users/me", cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return null;
            }
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PutAsJsonAsync("api/v1/users/me", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PutAsJsonAsync("api/v1/users/me/password", request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdatePreferencesAsync(UserPreferencesDto preferences, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PutAsJsonAsync("api/v1/users/me/preferences", preferences, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdatePrivacyAsync(PrivacySettingsDto privacy, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PutAsJsonAsync("api/v1/users/me/privacy", privacy, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.GetAsync($"api/v1/users/check-username?username={Uri.EscapeDataString(username)}", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var result = await response.Content.ReadFromJsonAsync<UsernameAvailabilityResponse>(cancellationToken);
            return result?.Available ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateAccountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.PostAsync("api/v1/users/me/deactivate", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _apiClient.DeleteAsync("api/v1/users/me", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private class UsernameAvailabilityResponse
    {
        public bool Available { get; set; }
    }
}
