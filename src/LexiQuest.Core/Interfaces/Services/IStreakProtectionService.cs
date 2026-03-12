using LexiQuest.Core.Domain.Entities;

namespace LexiQuest.Core.Interfaces.Services;

public interface IStreakProtectionService
{
    Task<StreakProtection?> GetProtectionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ActivateShieldAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> TryAutoFreezeAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> PurchaseShieldsAsync(Guid userId, int quantity, int coinCost, CancellationToken cancellationToken = default);
    Task<bool> PurchaseEmergencyShieldAsync(Guid userId, int coinCost, CancellationToken cancellationToken = default);
    Task<bool> CanActivateFreeShieldAsync(Guid userId, bool isPremium, CancellationToken cancellationToken = default);
    Task ResetWeeklyFreezeAsync(Guid userId, CancellationToken cancellationToken = default);
}
