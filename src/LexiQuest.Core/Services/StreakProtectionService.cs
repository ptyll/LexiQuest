using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

public class StreakProtectionService : IStreakProtectionService
{
    private readonly IStreakProtectionRepository _protectionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository? _userRepository;

    public StreakProtectionService(
        IStreakProtectionRepository protectionRepository,
        IUnitOfWork unitOfWork,
        IUserRepository? userRepository = null)
    {
        _protectionRepository = protectionRepository;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<StreakProtection?> GetProtectionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _protectionRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> ActivateShieldAsync(Guid userId, bool isPremium = false, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        var isNewProtection = false;
        if (protection == null)
        {
            protection = StreakProtection.Create(userId);
            await _protectionRepository.AddAsync(protection);
            isNewProtection = true;
        }

        if (protection.IsShieldActive)
            return false;

        if (protection.ShieldsRemaining <= 0)
        {
            if (!CanActivateFreeShield(protection, isPremium, DateTime.UtcNow))
                return false;

            protection.AddShields(1);
        }

        var result = protection.ActivateShield();
        if (result)
        {
            if (!isNewProtection)
            {
                _protectionRepository.Update(protection);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    public async Task<bool> TryAutoFreezeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
            return false;

        if (!protection.CanUseFreeze())
            return false;

        protection.UseFreeze();
        _protectionRepository.Update(protection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PurchaseShieldsAsync(Guid userId, int quantity, int coinCost, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0 || coinCost <= 0)
            return false;

        var user = _userRepository is null
            ? null
            : await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (_userRepository is not null)
        {
            if (user == null || user.CoinBalance < coinCost)
                return false;

            user.SpendCoins(coinCost);
        }

        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        var isNewProtection = false;
        if (protection == null)
        {
            protection = StreakProtection.Create(userId);
            await _protectionRepository.AddAsync(protection);
            isNewProtection = true;
        }

        protection.AddShields(quantity);
        if (!isNewProtection)
        {
            _protectionRepository.Update(protection);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PurchaseEmergencyShieldAsync(Guid userId, int coinCost, CancellationToken cancellationToken = default)
    {
        if (coinCost <= 0)
            return false;

        var user = _userRepository is null
            ? null
            : await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (_userRepository is not null)
        {
            if (user == null || user.CoinBalance < coinCost)
                return false;

            user.SpendCoins(coinCost);
        }

        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        var isNewProtection = false;
        if (protection == null)
        {
            protection = StreakProtection.Create(userId);
            await _protectionRepository.AddAsync(protection);
            isNewProtection = true;
        }

        // Emergency shield activates immediately without consuming from inventory
        protection.AddShields(1);
        var result = protection.ActivateShield();
        if (result)
        {
            if (!isNewProtection)
            {
                _protectionRepository.Update(protection);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    public async Task<bool> CanActivateFreeShieldAsync(Guid userId, bool isPremium, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
            return true; // No protection record means they haven't used their free shield

        return CanActivateFreeShield(protection, isPremium, DateTime.UtcNow);
    }

    public async Task ResetWeeklyFreezeAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
            return;

        protection.ResetWeeklyFreeze();
        _protectionRepository.Update(protection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static bool CanActivateFreeShield(StreakProtection protection, bool isPremium, DateTime now)
    {
        if (protection.IsShieldActive)
            return false;

        if (protection.LastShieldActivatedAt == null)
            return true;

        var daysSinceLastActivation = (now - protection.LastShieldActivatedAt.Value).TotalDays;
        var requiredDays = isPremium ? 7 : 30;
        return daysSinceLastActivation >= requiredDays;
    }
}
