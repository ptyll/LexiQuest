using LexiQuest.Blazor.Models;
using LexiQuest.Shared.DTOs.Auth;

namespace LexiQuest.Blazor.Services;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterModel model);
    Task<AuthResult> LoginAsync(string email, string password, bool rememberMe);
    Task<AuthResult> RefreshTokenAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task<PasswordResetRequestResult> RequestPasswordResetAsync(string email);
    Task<PasswordResetResult> ResetPasswordAsync(string token, string newPassword, string confirmPassword);
}

public class PasswordResetRequestResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PasswordResetResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthResponse? Data { get; set; }
}
