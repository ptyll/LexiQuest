using LexiQuest.Shared.DTOs.Streak;

namespace LexiQuest.Blazor.Services;

public interface IStreakProtectionClient
{
    Task<ActivateShieldResponse?> ActivateShieldAsync(CancellationToken cancellationToken = default);
    Task<PurchaseShieldsResponse?> PurchaseShieldsAsync(int quantity, CancellationToken cancellationToken = default);
    Task<EmergencyShieldResponse?> PurchaseEmergencyShieldAsync(CancellationToken cancellationToken = default);
}
