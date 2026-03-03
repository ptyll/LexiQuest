using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.Enums;
using NSubstitute;

namespace LexiQuest.Core.Tests.Services;

public class PathServiceTests
{
    private readonly IPathRepository _pathRepository;
    private readonly IUserRepository _userRepository;
    private readonly PathService _sut;

    public PathServiceTests()
    {
        _pathRepository = Substitute.For<IPathRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new PathService(_pathRepository, _userRepository);
    }

    [Fact]
    public async Task PathService_GetPaths_Returns4Paths()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        
        var paths = CreateDefaultPaths();
        _pathRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(paths);

        // Act
        var result = await _sut.GetPathsAsync(userId);

        // Assert
        result.Should().HaveCount(4);
    }

    [Fact]
    public async Task PathService_GetPaths_BeginnerAlwaysUnlocked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 1);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        
        var paths = CreateDefaultPaths();
        _pathRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(paths);

        // Act
        var result = await _sut.GetPathsAsync(userId);

        // Assert
        var beginnerPath = result.First(p => p.Difficulty == DifficultyLevel.Beginner);
        beginnerPath.IsUnlocked.Should().BeTrue();
    }

    [Fact]
    public async Task PathService_IsPathUnlocked_Intermediate_RequiresPath1OrLevel5()
    {
        // Arrange - User at level 5 (requirement met)
        var userId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.IsPathUnlockedAsync(userId, DifficultyLevel.Intermediate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PathService_IsPathUnlocked_Intermediate_LockedForLowLevel()
    {
        // Arrange - User at level 3 (requirement not met)
        var userId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 3);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.IsPathUnlockedAsync(userId, DifficultyLevel.Intermediate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task PathService_IsPathUnlocked_Advanced_RequiresPath2Complete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 10);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        
        // Setup paths so Intermediate can be found
        var paths = CreateDefaultPaths();
        _pathRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(paths);
        
        // Simulate Intermediate path (25 levels) completed
        _pathRepository.GetCompletedLevelsCountAsync(userId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(25);

        // Act
        var result = await _sut.IsPathUnlockedAsync(userId, DifficultyLevel.Advanced);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PathService_GetPathProgress_ReturnsCorrectProgress()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pathId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        
        var path = CreatePath(pathId, "Beginner", DifficultyLevel.Beginner, 20);
        _pathRepository.GetByIdAsync(pathId, Arg.Any<CancellationToken>()).Returns(path);
        _pathRepository.GetCompletedLevelsCountAsync(userId, pathId, Arg.Any<CancellationToken>()).Returns(5);

        // Act
        var result = await _sut.GetPathProgressAsync(userId, pathId);

        // Assert
        result.Should().NotBeNull();
        result.PathId.Should().Be(pathId);
        result.TotalLevels.Should().Be(20);
        result.CompletedLevels.Should().Be(5);
        result.CurrentLevel.Should().Be(6);
    }

    [Fact]
    public async Task PathService_GetPathProgress_ReturnsLevelsWithCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var pathId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        
        var path = CreatePath(pathId, "Beginner", DifficultyLevel.Beginner, 5);
        _pathRepository.GetByIdAsync(pathId, Arg.Any<CancellationToken>()).Returns(path);
        _pathRepository.GetCompletedLevelsCountAsync(userId, pathId, Arg.Any<CancellationToken>()).Returns(2);

        // Act
        var result = await _sut.GetPathProgressAsync(userId, pathId);

        // Assert
        result.Levels.Should().HaveCount(5);
        result.Levels[0].Status.Should().Be("Completed");
        result.Levels[1].Status.Should().Be("Completed");
        result.Levels[2].Status.Should().Be("Current");
        result.Levels[3].Status.Should().Be("Locked");
        result.Levels[4].Status.Should().Be("Locked");
    }

    [Fact]
    public async Task PathService_GetPaths_CalculatesProgressPercentage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLevel(userId, level: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        
        var pathId = Guid.NewGuid();
        var paths = new List<LearningPath> { CreatePath(pathId, "Beginner", DifficultyLevel.Beginner, 20) };
        _pathRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(paths);
        _pathRepository.GetCompletedLevelsCountAsync(userId, pathId, Arg.Any<CancellationToken>()).Returns(5);

        // Act
        var result = await _sut.GetPathsAsync(userId);

        // Assert
        result[0].ProgressPercentage.Should().Be(25.0); // 5/20 = 25%
    }

    private User CreateUserWithLevel(Guid userId, int level)
    {
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        typeof(UserStats).GetProperty(nameof(UserStats.Level))?.SetValue(user.Stats, level);
        return user;
    }

    private List<LearningPath> CreateDefaultPaths()
    {
        return new List<LearningPath>
        {
            CreatePath(Guid.NewGuid(), "Beginner", DifficultyLevel.Beginner, 20),
            CreatePath(Guid.NewGuid(), "Intermediate", DifficultyLevel.Intermediate, 25),
            CreatePath(Guid.NewGuid(), "Advanced", DifficultyLevel.Advanced, 30),
            CreatePath(Guid.NewGuid(), "Expert", DifficultyLevel.Expert, 40)
        };
    }

    private LearningPath CreatePath(Guid id, string name, DifficultyLevel difficulty, int totalLevels)
    {
        var path = LearningPath.Create(name, $"Description for {name}", difficulty, totalLevels, 3, 5, 30);
        typeof(LearningPath).GetProperty(nameof(LearningPath.Id))?.SetValue(path, id);
        
        for (int i = 1; i <= totalLevels; i++)
        {
            path.AddLevel(i, isBoss: i % 5 == 0);
        }
        
        return path;
    }
}
