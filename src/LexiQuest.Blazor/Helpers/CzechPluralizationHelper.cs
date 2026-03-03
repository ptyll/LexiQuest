using Microsoft.Extensions.Localization;

namespace LexiQuest.Blazor.Helpers;

public static class CzechPluralizationHelper
{
    /// <summary>
    /// Returns the correct Czech plural form based on the count.
    /// Czech has 3 forms: singular (1), plural 2-4, plural 5+
    /// Keys should be named: baseName.Singular, baseName.Plural2to4, baseName.Plural5Plus
    /// </summary>
    public static string GetPluralized<T>(this IStringLocalizer<T> localizer, string baseName, int count)
    {
        var key = count switch
        {
            1 => $"{baseName}.Singular",
            >= 2 and <= 4 => $"{baseName}.Plural2to4",
            _ => $"{baseName}.Plural5Plus"
        };

        return string.Format(localizer[key], count);
    }
}
