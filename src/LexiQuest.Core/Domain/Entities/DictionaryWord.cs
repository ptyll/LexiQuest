using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

/// <summary>
/// Represents a word in a custom dictionary.
/// </summary>
public class DictionaryWord
{
    public Guid Id { get; private set; }
    public Guid DictionaryId { get; private set; }
    public string Word { get; private set; } = null!;
    public DifficultyLevel Difficulty { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private DictionaryWord() { }

    public static DictionaryWord Create(Guid dictionaryId, string word, DifficultyLevel difficulty)
    {
        ValidateWordOrThrow(word);

        return new DictionaryWord
        {
            Id = Guid.NewGuid(),
            DictionaryId = dictionaryId,
            Word = word.ToLowerInvariant(),
            Difficulty = difficulty,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateWord(string word)
    {
        ValidateWordOrThrow(word);
        Word = word.ToLowerInvariant();
    }

    public void UpdateDifficulty(DifficultyLevel difficulty)
    {
        Difficulty = difficulty;
    }

    public string GetNormalizedWord()
    {
        // Remove diacritics and convert to lowercase
        var normalized = Word.Normalize(NormalizationForm.FormD);
        var chars = normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
        return new string(chars).ToLowerInvariant();
    }

    public static DifficultyLevel AutoDetectDifficulty(string word)
    {
        var length = word?.Length ?? 0;

        return length switch
        {
            <= 4 => DifficultyLevel.Beginner,
            <= 7 => DifficultyLevel.Intermediate,
            <= 10 => DifficultyLevel.Advanced,
            _ => DifficultyLevel.Expert
        };
    }

    public static ValidationResult Validate(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return new ValidationResult(false, "Word cannot be null or empty.");

        if (word.Length < 3)
            return new ValidationResult(false, "Word must be at least 3 characters long.");

        if (word.Length > 50)
            return new ValidationResult(false, "Word cannot exceed 50 characters.");

        if (word.Contains(' '))
            return new ValidationResult(false, "Word cannot contain spaces.");

        // Allow only letters (including Czech characters) and hyphen
        if (!Regex.IsMatch(word, @"^[a-zA-ZáčďéěíňóřšťúůýžÁČĎÉĚÍŇÓŘŠŤÚŮÝŽ\-]+$"))
            return new ValidationResult(false, "Word can only contain letters and hyphens.");

        return new ValidationResult(true, null);
    }

    private static void ValidateWordOrThrow(string word)
    {
        var result = Validate(word);
        if (!result.IsValid)
            throw new ArgumentException(result.ErrorMessage, nameof(word));
    }
}

public record ValidationResult(bool IsValid, string? ErrorMessage);
