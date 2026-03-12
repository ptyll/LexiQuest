namespace LexiQuest.Core.Domain.ValueObjects;

public class PrivacySettings
{
    public ProfileVisibility ProfileVisibility { get; set; }
    public bool LeaderboardVisible { get; set; }
    public bool StatsSharingEnabled { get; set; }

    public static PrivacySettings CreateDefault()
    {
        return new PrivacySettings
        {
            ProfileVisibility = ProfileVisibility.Public,
            LeaderboardVisible = true,
            StatsSharingEnabled = true
        };
    }
}

public enum ProfileVisibility
{
    Public,
    Friends,
    Private
}
