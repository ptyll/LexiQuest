namespace LexiQuest.Shared.DTOs.Users;

/// <summary>
/// User profile information.
/// </summary>
public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserStatsDto Stats { get; set; } = new();
    public UserPreferencesDto Preferences { get; set; } = new();
    public PrivacySettingsDto Privacy { get; set; } = new();
}

/// <summary>
/// User statistics summary.
/// </summary>
public class UserStatsDto
{
    public int Level { get; set; }
    public int TotalXP { get; set; }
    public int WordsSolved { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public double Accuracy { get; set; }
}
