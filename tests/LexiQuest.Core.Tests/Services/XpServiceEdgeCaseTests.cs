using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class XpServiceEdgeCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly LevelCalculator _levelCalculator = new();
    private readonly XpService _sut;

    public XpServiceEdgeCaseTests()
    {
        _sut = new XpService(_userRepository, _unitOfWork, _levelCalculator);
    }

    private User CreateUserWithXp(Guid userId, int currentXp)
    {
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        if (currentXp > 0)
        {
            typeof(UserStats).GetProperty(nameof(UserStats.TotalXP))?.SetValue(user.Stats, currentXp);
        }
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    // --- Very large XP values don't overflow ---

    [Fact]
    public async Task AddXp_LargeAmount_DoesNotOverflow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 0);

        // Act
        var result = await _sut.AddXpAsync(userId, 1_000_000, XpSource.Game);

        // Assert
        result.TotalXP.Should().Be(1_000_000);
        result.Amount.Should().Be(1_000_000);
    }

    [Fact]
    public async Task AddXp_AccumulatedLargeXP_DoesNotOverflow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 500_000);

        // Act
        var result = await _sut.AddXpAsync(userId, 500_000, XpSource.Game);

        // Assert
        result.TotalXP.Should().Be(1_000_000);
    }

    [Fact]
    public async Task AddXp_VerySmallAmount_StillAdds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 50);

        // Act
        var result = await _sut.AddXpAsync(userId, 1, XpSource.Game);

        // Assert
        result.TotalXP.Should().Be(51);
        result.LeveledUp.Should().BeFalse();
    }

    // --- Multiple level-ups in single add ---

    [Fact]
    public async Task AddXp_JumpMultipleLevels_ReportsHighestLevel()
    {
        // Arrange - level 1 to high level in one shot
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 0);
        // 100 (L1->L2) + 150 (L2->L3) + 225 (L3->L4) = 475 to reach L4
        // Adding 500 should take us from L1 to at least L4

        // Act
        var result = await _sut.AddXpAsync(userId, 500, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().BeGreaterThan(2);
    }

    [Fact]
    public async Task AddXp_ExactlyAtLevelBoundary_LevelsUp()
    {
        // Arrange - exactly at boundary for level 2 (100 XP)
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 0);

        // Act
        var result = await _sut.AddXpAsync(userId, 100, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(2);
    }

    [Fact]
    public async Task AddXp_OneLessThanBoundary_DoesNotLevelUp()
    {
        // Arrange - 99 XP is not enough for level 2 (needs 100)
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 0);

        // Act
        var result = await _sut.AddXpAsync(userId, 99, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeFalse();
        result.NewLevel.Should().Be(1);
    }

    // --- Level cap behavior ---

    [Fact]
    public async Task AddXp_VeryHighXP_LevelKeepsGrowing()
    {
        // There is no hard level cap - ensure very high XP still works
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 10_000_000);

        // Act
        var result = await _sut.AddXpAsync(userId, 100, XpSource.Game);

        // Assert
        result.NewLevel.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task AddXp_UserNotFound_Throws()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = () => _sut.AddXpAsync(userId, 100, XpSource.Game);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{userId}*");
    }

    [Fact]
    public async Task AddXp_Level3Unlock_ContainsIntermediatePath()
    {
        // Arrange - position user just below level 3 boundary
        // Level 3 needs cumulative: 100 + 150 = 250
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 240);

        // Act
        var result = await _sut.AddXpAsync(userId, 20, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(3);
        result.Unlocks.Should().Contain(u => u.Name == "Intermediate");
    }

    [Fact]
    public async Task AddXp_Level5Unlock_ContainsLeagues()
    {
        // Level 5 needs cumulative: 100 + 150 + 225 + 337 = 812
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 810);

        // Act
        var result = await _sut.AddXpAsync(userId, 10, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeTrue();
        result.NewLevel.Should().Be(5);
        result.Unlocks.Should().Contain(u => u.Name == "Leagues");
    }

    [Fact]
    public async Task AddXp_NoLevelUp_UnlocksIsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 50);

        // Act
        var result = await _sut.AddXpAsync(userId, 10, XpSource.Game);

        // Assert
        result.LeveledUp.Should().BeFalse();
        result.Unlocks.Should().BeNull();
    }

    [Fact]
    public async Task AddXp_AlwaysSavesChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithXp(userId, 0);

        // Act
        await _sut.AddXpAsync(userId, 50, XpSource.Game);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- LevelCalculator edge cases ---

    [Fact]
    public void LevelCalculator_ZeroXP_ReturnsLevel1()
    {
        _levelCalculator.GetLevelFromXp(0).Should().Be(1);
    }

    [Fact]
    public void LevelCalculator_NegativeXP_ReturnsLevel1()
    {
        _levelCalculator.GetLevelFromXp(-100).Should().Be(1);
    }

    [Fact]
    public void LevelCalculator_HasLeveledUp_SameLevelXP_ReturnsFalse()
    {
        _levelCalculator.HasLeveledUp(50, 99).Should().BeFalse();
    }

    [Fact]
    public void LevelCalculator_HasLeveledUp_CrossesBoundary_ReturnsTrue()
    {
        _levelCalculator.HasLeveledUp(99, 100).Should().BeTrue();
    }
}
