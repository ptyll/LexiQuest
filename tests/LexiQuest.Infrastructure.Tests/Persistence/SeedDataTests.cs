using FluentAssertions;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Infrastructure.Persistence;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Infrastructure.Tests.Persistence;

public class SeedDataTests
{
    [Fact]
    public void WordSeedData_Contains_MinimumWords()
    {
        var words = SeedData.GetWords();

        words.Should().HaveCountGreaterThanOrEqualTo(100);
    }

    [Fact]
    public void WordSeedData_Contains_AllDifficultyLevels()
    {
        var words = SeedData.GetWords();

        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Beginner);
        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Intermediate);
        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Advanced);
        words.Should().Contain(w => w.Difficulty == DifficultyLevel.Expert);
    }

    [Fact]
    public void WordSeedData_Beginner_HasAtLeast50Words()
    {
        var words = SeedData.GetWords();

        words.Count(w => w.Difficulty == DifficultyLevel.Beginner).Should().BeGreaterThanOrEqualTo(50);
    }
}
