using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class LearningPathTests
{
    [Fact]
    public void LearningPath_Create_SetsDefaultValues()
    {
        // Act
        var path = LearningPath.Create(
            name: "Beginner Path",
            description: "Easy words for beginners",
            difficulty: DifficultyLevel.Beginner,
            totalLevels: 20,
            wordLengthMin: 3,
            wordLengthMax: 5,
            timePerWord: 30
        );

        // Assert
        path.Name.Should().Be("Beginner Path");
        path.Description.Should().Be("Easy words for beginners");
        path.Difficulty.Should().Be(DifficultyLevel.Beginner);
        path.TotalLevels.Should().Be(20);
        path.WordLengthMin.Should().Be(3);
        path.WordLengthMax.Should().Be(5);
        path.TimePerWord.Should().Be(30);
        path.Levels.Should().BeEmpty();
    }
}
