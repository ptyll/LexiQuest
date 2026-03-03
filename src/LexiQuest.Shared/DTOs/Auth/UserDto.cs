namespace LexiQuest.Shared.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int CurrentStreak { get; set; }
    public int TotalXP { get; set; }
    public string League { get; set; } = string.Empty;
}
