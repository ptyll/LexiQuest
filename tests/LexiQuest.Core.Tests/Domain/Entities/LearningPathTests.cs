using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;

namespace LexiQuest.Core.Tests.Domain.Entities;

public class LearningPathTests
{
    [Fact]
    public void LearningPath_Create_SetsDefaultValues()
    {
        var path = LearningPath.Create(
            name: "Beginner Path",
            description: "Easy words for beginners",
            difficulty: DifficultyLevel.Beginner,
            totalLevels: 20,
            wordLengthMin: 3,
            wordLengthMax: 5,
            timePerWord: 30
        );

        path.Name.Should().Be("Beginner Path");
        path.Description.Should().Be("Easy words for beginners");
        path.Difficulty.Should().Be(DifficultyLevel.Beginner);
        path.TotalLevels.Should().Be(20);
        path.WordLengthMin.Should().Be(3);
        path.WordLengthMax.Should().Be(5);
        path.TimePerWord.Should().Be(30);
        path.Levels.Should().BeEmpty();
    }

    [Fact]
    public void LearningPath_Create_GeneratesUniqueId()
    {
        var path = LearningPath.Create("Test", "Desc", DifficultyLevel.Beginner, 5, 3, 5, 30);

        path.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void AddLevel_AddsLevelToList()
    {
        var path = LearningPath.Create("Test", "Desc", DifficultyLevel.Beginner, 5, 3, 5, 30);

        path.AddLevel(1);

        path.Levels.Should().HaveCount(1);
        path.Levels[0].LevelNumber.Should().Be(1);
        path.Levels[0].PathId.Should().Be(path.Id);
        path.Levels[0].Status.Should().Be(LevelStatus.Locked);
        path.Levels[0].IsBoss.Should().BeFalse();
    }

    [Fact]
    public void AddLevel_BossLevel_SetsBossFlag()
    {
        var path = LearningPath.Create("Test", "Desc", DifficultyLevel.Beginner, 5, 3, 5, 30);

        path.AddLevel(5, isBoss: true);

        path.Levels[0].IsBoss.Should().BeTrue();
    }

    [Fact]
    public void AddLevel_MultipleLevels_AllAdded()
    {
        var path = LearningPath.Create("Test", "Desc", DifficultyLevel.Beginner, 5, 3, 5, 30);

        path.AddLevel(1);
        path.AddLevel(2);
        path.AddLevel(3, isBoss: true);

        path.Levels.Should().HaveCount(3);
    }
}

public class PathLevelTests
{
    private static PathLevel CreateLevel(bool isBoss = false)
    {
        return PathLevel.Create(Guid.NewGuid(), 1, isBoss);
    }

    // --- Create ---

    [Fact]
    public void Create_StartsLocked()
    {
        var level = CreateLevel();

        level.Status.Should().Be(LevelStatus.Locked);
        level.IsPerfect.Should().BeFalse();
        level.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Create_SetsProperties()
    {
        var pathId = Guid.NewGuid();

        var level = PathLevel.Create(pathId, 3, true);

        level.Id.Should().NotBeEmpty();
        level.PathId.Should().Be(pathId);
        level.LevelNumber.Should().Be(3);
        level.IsBoss.Should().BeTrue();
    }

    // --- Unlock: Locked -> Available ---

    [Fact]
    public void Unlock_FromLocked_TransitionsToAvailable()
    {
        var level = CreateLevel();

        level.Unlock();

        level.Status.Should().Be(LevelStatus.Available);
    }

    [Fact]
    public void Unlock_FromAvailable_RemainsAvailable()
    {
        var level = CreateLevel();
        level.Unlock();

        level.Unlock();

        level.Status.Should().Be(LevelStatus.Available);
    }

    [Fact]
    public void Unlock_FromCurrent_RemainsCurrent()
    {
        var level = CreateLevel();
        level.Unlock();
        level.Start();

        level.Unlock();

        level.Status.Should().Be(LevelStatus.Current);
    }

    // --- Start: Available -> Current ---

    [Fact]
    public void Start_FromAvailable_TransitionsToCurrent()
    {
        var level = CreateLevel();
        level.Unlock();

        level.Start();

        level.Status.Should().Be(LevelStatus.Current);
    }

    [Fact]
    public void Start_FromLocked_RemainsLocked()
    {
        var level = CreateLevel();

        level.Start();

        level.Status.Should().Be(LevelStatus.Locked);
    }

    [Fact]
    public void Start_FromCompleted_RemainsCompleted()
    {
        var level = CreateLevel();
        level.Unlock();
        level.Start();
        level.Complete();

        level.Start();

        level.Status.Should().Be(LevelStatus.Completed);
    }

    // --- Complete: Current/Available -> Completed/Perfect ---

    [Fact]
    public void Complete_NotPerfect_TransitionsToCompleted()
    {
        var level = CreateLevel();
        level.Unlock();
        level.Start();

        level.Complete(isPerfect: false);

        level.Status.Should().Be(LevelStatus.Completed);
        level.IsPerfect.Should().BeFalse();
        level.CompletedAt.Should().NotBeNull();
        level.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Complete_Perfect_TransitionsToPerfect()
    {
        var level = CreateLevel();
        level.Unlock();
        level.Start();

        level.Complete(isPerfect: true);

        level.Status.Should().Be(LevelStatus.Perfect);
        level.IsPerfect.Should().BeTrue();
        level.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_DefaultIsPerfectIsFalse()
    {
        var level = CreateLevel();
        level.Unlock();
        level.Start();

        level.Complete();

        level.Status.Should().Be(LevelStatus.Completed);
        level.IsPerfect.Should().BeFalse();
    }

    // --- Full transition flow ---

    [Fact]
    public void FullTransition_Locked_Available_Current_Completed()
    {
        var level = CreateLevel();

        level.Status.Should().Be(LevelStatus.Locked);

        level.Unlock();
        level.Status.Should().Be(LevelStatus.Available);

        level.Start();
        level.Status.Should().Be(LevelStatus.Current);

        level.Complete();
        level.Status.Should().Be(LevelStatus.Completed);
    }

    [Fact]
    public void FullTransition_Locked_Available_Current_Perfect()
    {
        var level = CreateLevel();

        level.Unlock();
        level.Start();
        level.Complete(isPerfect: true);

        level.Status.Should().Be(LevelStatus.Perfect);
        level.IsPerfect.Should().BeTrue();
    }

    // --- Boss level transitions work the same ---

    [Fact]
    public void BossLevel_SameTransitionRules()
    {
        var level = CreateLevel(isBoss: true);

        level.IsBoss.Should().BeTrue();

        level.Unlock();
        level.Start();
        level.Complete(isPerfect: true);

        level.Status.Should().Be(LevelStatus.Perfect);
    }
}
