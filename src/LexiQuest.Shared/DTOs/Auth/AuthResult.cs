namespace LexiQuest.Shared.DTOs.Auth;

public class AuthResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public AuthResponse? Data { get; set; }
}
