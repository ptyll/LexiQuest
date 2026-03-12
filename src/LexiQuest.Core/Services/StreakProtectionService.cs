using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;

namespace LexiQuest.Core.Services;

public class StreakProtectionService : IStreakProtectionService
{
    private readonly IStreakProtectionRepository _protectionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StreakProtectionService(IStreakProtectionRepository protectionRepository, IUnitOfWork unitOfWork)
    {
        _protectionRepository = protectionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<StreakProtection?> GetProtectionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _protectionRepository.GetByUserIdAsync(userId);
    }

    public async Task<bool> ActivateShieldAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
            throw new InvalidOperationException("Streak protection not found for user");

        var result = protection.ActivateShield();
        if (result)
        {
            _protectionRepository.Update(protection);
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
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
        {
            protection = StreakProtection.Create(userId);
            await _protectionRepository.AddAsync(protection);
        }

        protection.AddShields(quantity);
        _protectionRepository.Update(protection);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> PurchaseEmergencyShieldAsync(Guid userId, int coinCost, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
        {
            protection = StreakProtection.Create(userId);
            await _protectionRepository.AddAsync(protection);
        }

        // Emergency shield activates immediately without consuming from inventory
        protection.AddShields(1);
        var result = protection.ActivateShield();
        if (result)
        {
            _protectionRepository.Update(protection);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return result;
    }

    public async Task<bool> CanActivateFreeShieldAsync(Guid userId, bool isPremium, CancellationToken cancellationToken = default)
    {
        var protection = await _protectionRepository.GetByUserIdAsync(userId);
        if (protection == null)
            return true; // No protection record means they haven't used their free shield

        if (protection.LastShieldActivatedAt == null)
            return true; // Never activated a shield

        var daysSinceLastActivation = (DateTime.UtcNow - protection.LastShieldActivatedAt.Value).TotalDays;

        // Premium users get 1 free shield per week (7 days)
        // Free users get 1 free shield per month (30 days)
        var requiredDays = isPremium ? 7 : 30;
        return daysSinceLastActivation >= requiredDays;
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
}
