namespace LexiQuest.Shared.DTOs.Auth;

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public int Level { get; set; }
    public double Accuracy { get; set; }
    public int StreakDays { get; set; }
    public bool IsPremium { get; set; }
}
