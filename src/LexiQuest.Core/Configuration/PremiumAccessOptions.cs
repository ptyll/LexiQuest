namespace LexiQuest.Core.Configuration;

public sealed class PremiumAccessOptions
{
    public const string SectionName = "PremiumAccess";

    public bool GrantAllFeatures { get; set; } = true;

    public int SyntheticPremiumYears { get; set; } = 100;

    public DateTime GetSyntheticExpiresAt(DateTime utcNow)
    {
        var years = Math.Clamp(SyntheticPremiumYears, 1, 100);
        return utcNow.AddYears(years);
    }
}
