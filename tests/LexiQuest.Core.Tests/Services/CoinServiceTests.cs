using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using NSubstitute;
using Xunit;
using AchievementRarity = LexiQuest.Core.Interfaces.Services.AchievementRarity;

namespace LexiQuest.Core.Tests.Services;

public class CoinServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoinService _service;

    public CoinServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _service = new CoinService(_userRepository, _unitOfWork);
    }

    [Fact]
    public async Task EarnCoins_LevelComplete_10Coins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.EarnCoinsAsync(userId, 10, CoinTransactionType.LevelComplete, "Level 5 completed");

        // Assert
        result.Should().BeTrue();
        user.CoinBalance.Should().Be(10);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task EarnCoins_BossLevel_50Coins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.EarnCoinsAsync(userId, 50, CoinTransactionType.BossLevel, "Boss defeated");

        // Assert
        result.Should().BeTrue();
        user.CoinBalance.Should().Be(50);
    }

    [Fact]
    public async Task EarnCoins_DailyChallenge_20Coins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.EarnCoinsAsync(userId, 20, CoinTransactionType.DailyChallenge, "Daily challenge completed");

        // Assert
        result.Should().BeTrue();
        user.CoinBalance.Should().Be(20);
    }

    [Theory]
    [InlineData(50, LexiQuest.Core.Interfaces.Services.AchievementRarity.Common)]
    [InlineData(100, LexiQuest.Core.Interfaces.Services.AchievementRarity.Rare)]
    [InlineData(150, LexiQuest.Core.Interfaces.Services.AchievementRarity.Epic)]
    [InlineData(200, LexiQuest.Core.Interfaces.Services.AchievementRarity.Legendary)]
    public async Task EarnCoins_Achievement_ReturnsCorrectAmount(int expectedCoins, LexiQuest.Core.Interfaces.Services.AchievementRarity rarity)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.EarnCoinsFromAchievementAsync(userId, rarity, $"Achievement {rarity} unlocked");

        // Assert
        result.Should().BeTrue();
        user.CoinBalance.Should().Be(expectedCoins);
    }

    [Fact]
    public async Task SpendCoins_SufficientBalance_DeductsCoins()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        user.AddCoins(100);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.SpendCoinsAsync(userId, 30, CoinTransactionType.ShopPurchase, "Purchased item");

        // Assert
        result.Success.Should().BeTrue();
        user.CoinBalance.Should().Be(70);
        await _unitOfWork.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task SpendCoins_InsufficientBalance_ReturnsError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        user.AddCoins(20);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var result = await _service.SpendCoinsAsync(userId, 50, CoinTransactionType.ShopPurchase, "Purchased item");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        user.CoinBalance.Should().Be(20); // Balance unchanged
        await _unitOfWork.Received(0).SaveChangesAsync();
    }

    [Fact]
    public async Task GetBalance_ReturnsCurrentBalance()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        user.AddCoins(150);
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var balance = await _service.GetBalanceAsync(userId);

        // Assert
        balance.Should().Be(150);
    }

    [Fact]
    public async Task GetBalance_UserNotFound_ReturnsZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var balance = await _service.GetBalanceAsync(userId);

        // Assert
        balance.Should().Be(0);
    }

    [Fact]
    public async Task GetTransactionHistory_ReturnsTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = User.Create("test@test.com", "testuser");
        user.GetType().GetProperty("Id")?.SetValue(user, userId);
        user.AddCoinTransaction(50, "LevelComplete", "Level 5 completed");
        
        _userRepository.GetByIdAsync(userId).Returns(user);

        // Act
        var transactions = await _service.GetTransactionHistoryAsync(userId, 10);

        // Assert
        transactions.Should().NotBeNull();
        transactions.Should().HaveCountGreaterThan(0);
    }
}

public enum AchievementRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
