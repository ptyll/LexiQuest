using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

public class CoinService : ICoinService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CoinService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> EarnCoinsAsync(Guid userId, int amount, CoinTransactionType type, string description, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return false;

        user.AddCoinTransaction(amount, type.ToString(), description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public Task<bool> EarnCoinsFromAchievementAsync(Guid userId, AchievementRarity rarity, string description, CancellationToken cancellationToken = default)
    {
        var amount = rarity switch
        {
            AchievementRarity.Common => 50,
            AchievementRarity.Rare => 100,
            AchievementRarity.Epic => 150,
            AchievementRarity.Legendary => 200,
            _ => 50
        };

        return EarnCoinsAsync(userId, amount, CoinTransactionType.Achievement, description, cancellationToken);
    }

    public async Task<SpendCoinsResult> SpendCoinsAsync(Guid userId, int amount, CoinTransactionType type, string description, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return new SpendCoinsResult(false, 0, "User not found");

        if (user.CoinBalance < amount)
            return new SpendCoinsResult(false, user.CoinBalance, "Nedostatek mincí");

        user.AddCoinTransaction(-amount, type.ToString(), description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return new SpendCoinsResult(true, user.CoinBalance, null);
    }

    public async Task<int> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.CoinBalance ?? 0;
    }

    public async Task<IEnumerable<CoinTransactionDto>> GetTransactionHistoryAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Enumerable.Empty<CoinTransactionDto>();

        return user.CoinTransactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(t => new CoinTransactionDto(
                t.Id,
                t.Amount,
                Enum.Parse<CoinTransactionType>(t.Type),
                t.Description,
                t.CreatedAt,
                t.BalanceAfter));
    }
}
