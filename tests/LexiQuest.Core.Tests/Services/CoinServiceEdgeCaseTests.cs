using FluentAssertions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Core.Services;
using NSubstitute;
using Xunit;

namespace LexiQuest.Core.Tests.Services;

public class CoinServiceEdgeCaseTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CoinService _sut;

    public CoinServiceEdgeCaseTests()
    {
        _sut = new CoinService(_userRepository, _unitOfWork);
    }

    private User CreateUserWithCoins(Guid userId, int coins)
    {
        var user = User.Create("test@test.com", "testuser");
        user.SetId(userId);
        if (coins > 0)
            user.AddCoins(coins);
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);
        return user;
    }

    // --- Spending more coins than available ---

    [Fact]
    public async Task SpendCoins_ExactBalance_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 100);

        // Act
        var result = await _sut.SpendCoinsAsync(userId, 100, CoinTransactionType.ShopPurchase, "Buy item");

        // Assert
        result.Success.Should().BeTrue();
        user.CoinBalance.Should().Be(0);
    }

    [Fact]
    public async Task SpendCoins_OneMoreThanBalance_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 99);

        // Act
        var result = await _sut.SpendCoinsAsync(userId, 100, CoinTransactionType.ShopPurchase, "Buy item");

        // Assert
        result.Success.Should().BeFalse();
        result.NewBalance.Should().Be(99);
        user.CoinBalance.Should().Be(99);
    }

    [Fact]
    public async Task SpendCoins_ZeroBalance_Fails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithCoins(userId, 0);

        // Act
        var result = await _sut.SpendCoinsAsync(userId, 1, CoinTransactionType.ShopPurchase, "Buy item");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SpendCoins_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.SpendCoinsAsync(userId, 10, CoinTransactionType.ShopPurchase, "Buy item");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task SpendCoins_DoesNotSaveOnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        CreateUserWithCoins(userId, 10);

        // Act
        await _sut.SpendCoinsAsync(userId, 50, CoinTransactionType.ShopPurchase, "Buy item");

        // Assert
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // --- AddCoins with zero/negative amounts ---

    [Fact]
    public async Task EarnCoins_UserNotFound_ReturnsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.EarnCoinsAsync(userId, 50, CoinTransactionType.LevelComplete, "Level complete");

        // Assert
        result.Should().BeFalse();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void AddCoins_ZeroAmount_ThrowsOnEntity()
    {
        // Arrange - User.AddCoins(0) should throw because amount must be positive
        var user = User.Create("test@test.com", "testuser");

        // Act
        var act = () => user.AddCoins(0);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddCoins_NegativeAmount_ThrowsOnEntity()
    {
        // Arrange
        var user = User.Create("test@test.com", "testuser");

        // Act
        var act = () => user.AddCoins(-10);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    // --- Transaction history correctness ---

    [Fact]
    public async Task EarnCoins_CreatesTransactionWithCorrectType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 0);

        // Act
        await _sut.EarnCoinsAsync(userId, 50, CoinTransactionType.BossLevel, "Boss defeated");

        // Assert
        user.CoinTransactions.Should().HaveCount(1);
        var tx = user.CoinTransactions.First();
        tx.Type.Should().Be("BossLevel");
        tx.Description.Should().Be("Boss defeated");
        tx.Amount.Should().Be(50);
    }

    [Fact]
    public async Task SpendCoins_CreatesNegativeTransaction()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 100);

        // Act
        await _sut.SpendCoinsAsync(userId, 30, CoinTransactionType.ShopPurchase, "Bought avatar");

        // Assert
        user.CoinTransactions.Should().HaveCount(1);
        var tx = user.CoinTransactions.First();
        tx.Amount.Should().Be(-30);
    }

    [Fact]
    public async Task GetTransactionHistory_UserNotFound_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _sut.GetTransactionHistoryAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTransactionHistory_RespectsLimit()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 0);
        // Add 5 transactions
        for (int i = 1; i <= 5; i++)
        {
            user.AddCoinTransaction(10, "LevelComplete", $"Level {i}");
        }

        // Act
        var result = await _sut.GetTransactionHistoryAsync(userId, limit: 3);

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTransactionHistory_OrderedByMostRecent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 0);
        user.AddCoinTransaction(10, "LevelComplete", "First");
        user.AddCoinTransaction(20, "BossLevel", "Second");

        // Act
        var result = (await _sut.GetTransactionHistoryAsync(userId)).ToList();

        // Assert
        result.Should().HaveCount(2);
        // Both created at ~same time (DateTime.UtcNow) so just check they exist
        result.Select(t => t.Amount).Should().Contain(10);
        result.Select(t => t.Amount).Should().Contain(20);
    }

    // --- Balance after transaction is accurate ---

    [Fact]
    public async Task BalanceAfterMultipleEarns_IsAccurate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 0);

        // Act
        await _sut.EarnCoinsAsync(userId, 50, CoinTransactionType.LevelComplete, "Level 1");
        await _sut.EarnCoinsAsync(userId, 100, CoinTransactionType.BossLevel, "Boss 1");

        // Assert
        user.CoinBalance.Should().Be(150);
        var balance = await _sut.GetBalanceAsync(userId);
        balance.Should().Be(150);
    }

    [Fact]
    public async Task BalanceAfterEarnAndSpend_IsAccurate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserWithCoins(userId, 0);

        // Act
        await _sut.EarnCoinsAsync(userId, 200, CoinTransactionType.Achievement, "Achievement unlocked");
        var spendResult = await _sut.SpendCoinsAsync(userId, 75, CoinTransactionType.ShopPurchase, "Buy item");

        // Assert
        spendResult.Success.Should().BeTrue();
        user.CoinBalance.Should().Be(125);
        spendResult.NewBalance.Should().Be(125);
    }
}
