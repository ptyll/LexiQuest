using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain;

public class WordScrambleTests
{
    private readonly Random _rng = new(42); // Seeded for reproducibility

    [Fact]
    public void ScrambleService_Scramble_UsesFisherYatesShuffle()
    {
        // Arrange
        var word = Word.Create("HELLO", DifficultyLevel.Beginner, WordCategory.Food);
        var results = new HashSet<string>();

        // Act - Run multiple times to verify randomness (Fisher-Yates produces different results)
        for (int i = 0; i < 20; i++)
        {
            results.Add(word.Scramble(new Random(i)));
        }

        // Assert - Should have multiple different results proving shuffle works
        results.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public void ScrambleService_Scramble_NeverReturnsOriginal()
    {
        // Arrange
        var word = Word.Create("HELLO", DifficultyLevel.Beginner, WordCategory.Food);

        // Act & Assert - Run many times to ensure it never returns original
        for (int i = 0; i < 100; i++)
        {
            var scrambled = word.Scramble(new Random(i));
            scrambled.Should().NotBe("HELLO", $"Iteration {i} returned original word");
        }
    }

    [Fact]
    public void ScrambleService_Scramble_ContainsSameLetters()
    {
        // Arrange
        var word = Word.Create("HELLO", DifficultyLevel.Beginner, WordCategory.Food);

        // Act
        var scrambled = word.Scramble(_rng);

        // Assert - Same letters, same count
        scrambled.Should().HaveLength("HELLO".Length);
        scrambled.OrderBy(c => c).Should().Equal("HELLO".OrderBy(c => c));
    }

    [Theory]
    [InlineData("AB", 2)]      // 2 letters
    [InlineData("ABC", 3)]     // 3 letters
    public void ScrambleService_Scramble_HandlesShortWords(string original, int expectedLength)
    {
        // Arrange
        var word = Word.Create(original, DifficultyLevel.Beginner, WordCategory.Food);

        // Act
        var scrambled = word.Scramble(_rng);

        // Assert
        scrambled.Should().HaveLength(expectedLength);
        scrambled.Should().NotBe(original);
        scrambled.OrderBy(c => c).Should().Equal(original.OrderBy(c => c));
    }

    [Fact]
    public void ScrambleService_Scramble_HandlesDuplicateLetters()
    {
        // Arrange - "ANNA" has duplicate letters A and N
        var word = Word.Create("ANNA", DifficultyLevel.Beginner, WordCategory.Food);

        // Act & Assert - Run multiple times to ensure it handles duplicates
        for (int i = 0; i < 50; i++)
        {
            var scrambled = word.Scramble(new Random(i));
            
            // Should not return original
            scrambled.Should().NotBe("ANNA");
            
            // Should have same letters
            scrambled.Should().HaveLength(4);
            scrambled.Count(c => c == 'A').Should().Be(2);
            scrambled.Count(c => c == 'N').Should().Be(2);
        }
    }

    [Fact]
    public void ScrambleService_Scramble_AllIdenticalLetters_ReturnsOriginal()
    {
        // Arrange - "AAA" cannot be scrambled differently
        var word = Word.Create("AAA", DifficultyLevel.Beginner, WordCategory.Food);

        // Act
        var scrambled = word.Scramble(_rng);

        // Assert - Returns original when all letters are identical
        scrambled.Should().Be("AAA");
    }

    [Fact]
    public void ScrambleService_Scramble_MultipleRunsProduceDifferentResults()
    {
        // Arrange
        var word = Word.Create("SCRAMBLE", DifficultyLevel.Intermediate, WordCategory.Science);
        var results = new List<string>();

        // Act - Run with different random seeds
        for (int i = 0; i < 20; i++)
        {
            results.Add(word.Scramble(new Random(i)));
        }

        // Assert - Results should be diverse (not all the same)
        results.Distinct().Count().Should().BeGreaterThan(10);
    }
}
