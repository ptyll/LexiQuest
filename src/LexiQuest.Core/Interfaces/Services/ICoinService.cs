namespace LexiQuest.Core.Interfaces.Services;

public interface ICoinService
{
    Task<bool> EarnCoinsAsync(Guid userId, int amount, CoinTransactionType type, string description, CancellationToken cancellationToken = default);
    Task<bool> EarnCoinsFromAchievementAsync(Guid userId, AchievementRarity rarity, string description, CancellationToken cancellationToken = default);
    Task<SpendCoinsResult> SpendCoinsAsync(Guid userId, int amount, CoinTransactionType type, string description, CancellationToken cancellationToken = default);
    Task<int> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CoinTransactionDto>> GetTransactionHistoryAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
}

public enum CoinTransactionType
{
    LevelComplete,
    BossLevel,
    DailyChallenge,
    Achievement,
    ShopPurchase,
    ShieldPurchase,
    EmergencyShield,
    Refund
}

public enum AchievementRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

public record SpendCoinsResult(bool Success, int NewBalance, string? ErrorMessage = null);

public record CoinTransactionDto(
    Guid Id,
    int Amount,
    CoinTransactionType Type,
    string Description,
    DateTime CreatedAt,
    int BalanceAfter);
