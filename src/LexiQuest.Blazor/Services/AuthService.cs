using System.Net.Http.Json;
using LexiQuest.Blazor.Models;
using LexiQuest.Shared.DTOs.Auth;
using Microsoft.JSInterop;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Blazor.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly IStringLocalizer<AuthService> _localizer;
    private const string TokenKey = "access_token";
    private const string RefreshTokenKey = "refresh_token";

    public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime, IStringLocalizer<AuthService> localizer)
    {
        _httpClient = httpClientFactory.CreateClient("ApiClient");
        _jsRuntime = jsRuntime;
        _localizer = localizer;
    }

    public async Task<AuthResult> RegisterAsync(RegisterModel model)
    {
        try
        {
            var request = new RegisterRequest
            {
                Email = model.Email,
                Username = model.Username,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                AcceptTerms = model.AcceptTerms
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/users/register", request);

            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Register.Duplicate"] };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Register.Failed"] };
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (result != null)
            {
                await StoreTokensAsync(result.AccessToken, result.RefreshToken);
                return new AuthResult { Success = true, Data = result };
            }

            return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Register.InvalidResponse"] };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = string.Format(_localizer["Error.Register.Exception"], ex.Message) };
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe)
    {
        try
        {
            var request = new LoginRequest
            {
                Email = email,
                Password = password,
                RememberMe = rememberMe
            };

            var response = await _httpClient.PostAsJsonAsync("api/v1/users/login", request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Login.InvalidCredentials"] };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Login.Failed"] };
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (result != null)
            {
                await StoreTokensAsync(result.AccessToken, result.RefreshToken);
                return new AuthResult { Success = true, Data = result };
            }

            return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Login.InvalidResponse"] };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = string.Format(_localizer["Error.Login.Exception"], ex.Message) };
        }
    }

    public async Task<AuthResult> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
            
            if (string.IsNullOrEmpty(refreshToken))
            {
                return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Refresh.NoToken"] };
            }

            // Call refresh token endpoint
            var response = await _httpClient.PostAsJsonAsync("api/v1/users/refresh", new { RefreshToken = refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                // Clear tokens on refresh failure
                await LogoutAsync();
                return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Refresh.Failed"] };
            }

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            
            if (result != null)
            {
                await StoreTokensAsync(result.AccessToken, result.RefreshToken);
                return new AuthResult { Success = true, Data = result };
            }

            return new AuthResult { Success = false, ErrorMessage = _localizer["Error.Refresh.InvalidResponse"] };
        }
        catch (Exception ex)
        {
            return new AuthResult { Success = false, ErrorMessage = string.Format(_localizer["Error.Refresh.Exception"], ex.Message) };
        }
    }

    public async Task LogoutAsync()
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }

    private async Task StoreTokensAsync(string accessToken, string refreshToken)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, accessToken);
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, refreshToken);
    }
}
