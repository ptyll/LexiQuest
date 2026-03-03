namespace LexiQuest.Core.Domain.ValueObjects;

public class UserPreferences
{
    public string Theme { get; private set; } = "light";
    public string Language { get; private set; } = "cs";
    public bool AnimationsEnabled { get; private set; } = true;
    public bool SoundsEnabled { get; private set; } = true;

    private UserPreferences() { }

    public static UserPreferences CreateDefault()
    {
        return new UserPreferences
        {
            Theme = "light",
            Language = "cs",
            AnimationsEnabled = true,
            SoundsEnabled = true
        };
    }

    public void SetTheme(string theme) => Theme = theme;
    public void SetLanguage(string language) => Language = language;
    public void SetAnimationsEnabled(bool enabled) => AnimationsEnabled = enabled;
    public void SetSoundsEnabled(bool enabled) => SoundsEnabled = enabled;
}
