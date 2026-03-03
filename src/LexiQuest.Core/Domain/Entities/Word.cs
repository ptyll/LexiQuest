using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Domain.Entities;

public class Word
{
    public Guid Id { get; private set; }
    public string Original { get; private set; } = null!;
    public string Normalized { get; private set; } = null!;
    public int Length { get; private set; }
    public DifficultyLevel Difficulty { get; private set; }
    public int FrequencyRank { get; private set; }
    public WordCategory Category { get; private set; }

    private Word() { }

    public static Word Create(string original, DifficultyLevel difficulty, WordCategory category, int frequencyRank = 0)
    {
        return new Word
        {
            Id = Guid.NewGuid(),
            Original = original,
            Normalized = original.ToLowerInvariant(),
            Length = original.Length,
            Difficulty = difficulty,
            Category = category,
            FrequencyRank = frequencyRank
        };
    }

    /// <summary>
    /// Fisher-Yates shuffle ensuring result differs from original.
    /// For words with all identical characters, returns the original.
    /// </summary>
    public string Scramble(Random rng)
    {
        var chars = Original.ToCharArray();

        // Check if all characters are the same — can't produce different order
        if (chars.Distinct().Count() == 1)
            return Original;

        string scrambled;
        do
        {
            var array = (char[])chars.Clone();
            for (var i = array.Length - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            scrambled = new string(array);
        } while (scrambled == Original);

        return scrambled;
    }
}
