using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain;

public class WordTests
{
    [Fact]
    public void Create_SetsProperties()
    {
        var word = Word.Create("jablko", DifficultyLevel.Beginner, WordCategory.Food);

        word.Id.Should().NotBe(Guid.Empty);
        word.Original.Should().Be("jablko");
        word.Normalized.Should().Be("jablko");
        word.Length.Should().Be(6);
        word.Difficulty.Should().Be(DifficultyLevel.Beginner);
        word.Category.Should().Be(WordCategory.Food);
    }

    [Fact]
    public void Scramble_ReturnsDifferentOrder()
    {
        var word = Word.Create("programovani", DifficultyLevel.Advanced, WordCategory.Technology);
        var rng = new Random(42);

        var scrambled = word.Scramble(rng);

        scrambled.Should().NotBe(word.Original);
        scrambled.Length.Should().Be(word.Original.Length);
        new string(scrambled.OrderBy(c => c).ToArray())
            .Should().Be(new string(word.Original.OrderBy(c => c).ToArray()));
    }

    [Fact]
    public void Scramble_NeverReturnsOriginal()
    {
        var word = Word.Create("testing", DifficultyLevel.Intermediate, WordCategory.Technology);

        for (var i = 0; i < 100; i++)
        {
            var rng = new Random(i);
            var scrambled = word.Scramble(rng);
            scrambled.Should().NotBe(word.Original);
        }
    }

    [Fact]
    public void Scramble_TwoCharWord_ReturnsDifferentOrder()
    {
        var word = Word.Create("ab", DifficultyLevel.Beginner, WordCategory.Animals);
        var rng = new Random(42);

        var scrambled = word.Scramble(rng);

        scrambled.Should().Be("ba");
    }

    [Fact]
    public void Scramble_AllSameChars_ReturnsSameString()
    {
        var word = Word.Create("aaa", DifficultyLevel.Beginner, WordCategory.Animals);
        var rng = new Random(42);

        var scrambled = word.Scramble(rng);

        scrambled.Should().Be("aaa");
    }
}
