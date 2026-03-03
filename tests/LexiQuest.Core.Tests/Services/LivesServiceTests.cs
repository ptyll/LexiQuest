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

public class LivesServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LivesService _sut;

    public LivesServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _sut = new LivesService(_userRepository, _unitOfWork);
    }

    [Theory]
    [InlineData(DifficultyLevel.Beginner, 5)]
    [InlineData(DifficultyLevel.Intermediate, 4)]
    [InlineData(DifficultyLevel.Advanced, 3)]
    [InlineData(DifficultyLevel.Expert, 3)]
    public void LivesService_GetMaxLies_ByDifficulty_ReturnsCorrectValue(DifficultyLevel difficulty, int expectedLives)
    {
        // Act
        var result = _sut.GetMaxLives(difficulty);

        // Assert
        result.Should().Be(expectedLives);
    }

    [Fact]
    public async Task LivesService_GetLivesStatus_ReturnsCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 3, maxLives: 5);
        user.SetNextLifeRegenAt(DateTime.UtcNow.AddMinutes(15));
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.GetLivesStatusAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Current.Should().Be(3);
        result.Max.Should().Be(5);
        result.NextRegenAt.Should().NotBeNull();
        result.IsInfinite.Should().BeFalse();
    }

    [Fact]
    public async Task LivesService_GetLivesStatus_TrainingMode_ReturnsInfinite()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 999, maxLives: int.MaxValue, difficulty: DifficultyLevel.Beginner);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.GetLivesStatusAsync(userId);

        // Assert
        result.IsInfinite.Should().BeTrue();
        result.Current.Should().Be(999);
    }

    [Fact]
    public async Task LivesService_LoseLife_DecrementsCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 5, maxLives: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.LoseLifeAsync(userId);

        // Assert
        result.Should().BeTrue();
        user.LivesRemaining.Should().Be(4);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LivesService_LoseLife_AtZero_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 0, maxLives: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.LoseLifeAsync(userId);

        // Assert
        result.Should().BeFalse();
        user.LivesRemaining.Should().Be(0);
    }

    [Fact]
    public async Task LivesService_LoseLife_SetsNextRegenTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 3, maxLives: 3, difficulty: DifficultyLevel.Advanced);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _sut.LoseLifeAsync(userId);

        // Assert
        user.NextLifeRegenAt.Should().NotBeNull();
        user.NextLifeRegenAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LivesService_RegenerateLife_AfterInterval_IncrementsCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 2, maxLives: 5);
        user.SetNextLifeRegenAt(DateTime.UtcNow.AddMinutes(-5)); // Past time
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.RegenerateLifeAsync(userId);

        // Assert
        result.Should().BeTrue();
        user.LivesRemaining.Should().Be(3);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LivesService_RegenerateLife_BeforeInterval_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 2, maxLives: 5);
        user.SetNextLifeRegenAt(DateTime.UtcNow.AddMinutes(10)); // Future time
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.RegenerateLifeAsync(userId);

        // Assert
        result.Should().BeFalse();
        user.LivesRemaining.Should().Be(2);
    }

    [Fact]
    public async Task LivesService_RegenerateLife_AtMax_DoesNotExceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 5, maxLives: 5);
        user.SetNextLifeRegenAt(DateTime.UtcNow.AddMinutes(-5));
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.RegenerateLifeAsync(userId);

        // Assert
        result.Should().BeFalse();
        user.LivesRemaining.Should().Be(5);
    }

    [Fact]
    public async Task LivesService_RefillLives_SetsToMax()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: 1, maxLives: 5);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        await _sut.RefillLivesAsync(userId);

        // Assert
        user.LivesRemaining.Should().Be(5);
        user.NextLifeRegenAt.Should().BeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LivesService_LoseLife_InTrainingMode_DoesNotDecrement()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithLives(userId, currentLives: int.MaxValue, maxLives: int.MaxValue, difficulty: DifficultyLevel.Beginner);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        // Act
        var result = await _sut.LoseLifeAsync(userId);

        // Assert
        result.Should().BeTrue(); // Can still play
        user.LivesRemaining.Should().Be(int.MaxValue); // But lives don't decrease
    }

    private User CreateUserWithLives(Guid userId, int currentLives, int maxLives, DifficultyLevel difficulty = DifficultyLevel.Intermediate)
    {
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        user.SetLives(currentLives, maxLives);
        return user;
    }
}
