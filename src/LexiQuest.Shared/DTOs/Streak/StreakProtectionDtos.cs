namespace LexiQuest.Shared.DTOs.Streak;

// StreakProtectionDto is defined in StreakProtectionDto.cs

public record ActivateShieldRequest();

public record ActivateShieldResponse(
    bool Success,
    string Message,
    int RemainingShields);

public record PurchaseShieldsRequest(
    int Quantity);

public record PurchaseShieldsResponse(
    bool Success,
    string Message,
    int TotalShields,
    int RemainingCoins);

public record EmergencyShieldRequest();

public record EmergencyShieldResponse(
    bool Success,
    string Message,
    bool IsShieldActive,
    int RemainingCoins);
