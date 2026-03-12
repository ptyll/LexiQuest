using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class WordTests
{
    // --- Create ---

    [Fact]
    public void Create_StoresOriginalAndNormalizedLowercase()
    {
        var word = Word.Create("HELLO", DifficultyLevel.Beginner, WordCategory.Everyday);

        word.Original.Should().Be("HELLO");
        word.Normalized.Should().Be("hello");
        word.Length.Should().Be(5);
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var word = Word.Create("cat", DifficultyLevel.Intermediate, WordCategory.Animals, 42);

        word.Id.Should().NotBeEmpty();
        word.Original.Should().Be("cat");
        word.Normalized.Should().Be("cat");
        word.Length.Should().Be(3);
        word.Difficulty.Should().Be(DifficultyLevel.Intermediate);
        word.Category.Should().Be(WordCategory.Animals);
        word.FrequencyRank.Should().Be(42);
    }

    [Fact]
    public void Create_DefaultFrequencyRank_IsZero()
    {
        var word = Word.Create("dog", DifficultyLevel.Beginner, WordCategory.Animals);

        word.FrequencyRank.Should().Be(0);
    }

    [Fact]
    public void Create_MixedCase_NormalizesToLower()
    {
        var word = Word.Create("CaT", DifficultyLevel.Beginner, WordCategory.Animals);

        word.Normalized.Should().Be("cat");
        word.Original.Should().Be("CaT");
    }

    // --- Scramble ---

    [Fact]
    public void Scramble_MultipleChars_ReturnsDifferentFromOriginal()
    {
        var word = Word.Create("hello", DifficultyLevel.Beginner, WordCategory.Everyday);
        var rng = new Random(42);

        var scrambled = word.Scramble(rng);

        scrambled.Should().NotBe("hello");
        scrambled.Length.Should().Be(5);
        new string(scrambled.Order().ToArray()).Should().Be(new string("hello".Order().ToArray()));
    }

    [Fact]
    public void Scramble_AllSameChars_ReturnsOriginal()
    {
        var word = Word.Create("aaaa", DifficultyLevel.Beginner, WordCategory.Everyday);
        var rng = new Random(42);

        var scrambled = word.Scramble(rng);

        scrambled.Should().Be("aaaa");
    }

    [Fact]
    public void Scramble_TwoDistinctChars_ReturnsDifferent()
    {
        var word = Word.Create("ab", DifficultyLevel.Beginner, WordCategory.Everyday);
        var rng = new Random(42);

        var scrambled = word.Scramble(rng);

        scrambled.Should().Be("ba");
    }

    [Fact]
    public void Scramble_PreservesAllCharacters()
    {
        var word = Word.Create("scramble", DifficultyLevel.Advanced, WordCategory.Everyday);
        var rng = new Random(123);

        var scrambled = word.Scramble(rng);

        scrambled.Length.Should().Be(8);
        scrambled.Order().Should().BeEquivalentTo("scramble".Order());
    }

    [Fact]
    public void Scramble_DifferentSeeds_CanProduceDifferentResults()
    {
        var word = Word.Create("testing", DifficultyLevel.Beginner, WordCategory.Everyday);
        var results = new HashSet<string>();

        for (int i = 0; i < 10; i++)
        {
            var scrambled = word.Scramble(new Random(i));
            results.Add(scrambled);
        }

        results.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void Scramble_NeverReturnsOriginal_ForDistinctChars()
    {
        var word = Word.Create("abcdef", DifficultyLevel.Beginner, WordCategory.Everyday);

        for (int i = 0; i < 50; i++)
        {
            var scrambled = word.Scramble(new Random(i));
            scrambled.Should().NotBe("abcdef");
        }
    }

    [Fact]
    public void Scramble_SingleDistinctCharRepeated_ReturnsOriginal()
    {
        var word = Word.Create("bbb", DifficultyLevel.Beginner, WordCategory.Everyday);
        var rng = new Random(0);

        var scrambled = word.Scramble(rng);

        scrambled.Should().Be("bbb");
    }
}
