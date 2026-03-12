namespace LexiQuest.Shared.DTOs.Users;

/// <summary>
/// User privacy settings.
/// </summary>
public class PrivacySettingsDto
{
    public ProfileVisibility ProfileVisibility { get; set; } = ProfileVisibility.Public;
    public bool LeaderboardVisible { get; set; } = true;
    public bool StatsSharingEnabled { get; set; } = true;
}

public enum ProfileVisibility
{
    Public,
    Friends,
    Private
}
