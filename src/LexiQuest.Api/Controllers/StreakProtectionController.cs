using LexiQuest.Api.Extensions;
using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Streak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/streak")]
[Authorize]
public class StreakProtectionController : ControllerBase
{
    private readonly IStreakProtectionService _streakProtectionService;
    private readonly IPremiumFeatureService _premiumFeatureService;

    public StreakProtectionController(
        IStreakProtectionService streakProtectionService,
        IPremiumFeatureService premiumFeatureService)
    {
        _streakProtectionService = streakProtectionService;
        _premiumFeatureService = premiumFeatureService;
    }

    [HttpGet("protection")]
    public async Task<ActionResult<StreakProtectionDto>> GetProtection(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var protection = await _streakProtectionService.GetProtectionAsync(userId, cancellationToken);

        if (protection == null)
        {
            // Return default protection state
            return Ok(new StreakProtectionDto(
                ShieldsRemaining: 0,
                HasActiveShield: false,
                FreezeUsedThisWeek: false,
                CanActivateFreeShield: false,
                NextShieldAvailableAt: null));
        }

        return Ok(new StreakProtectionDto(
            ShieldsRemaining: protection.ShieldsRemaining,
            HasActiveShield: protection.IsShieldActive,
            FreezeUsedThisWeek: protection.FreezeUsedThisWeek,
            CanActivateFreeShield: protection.CanUseFreeze(),
            NextShieldAvailableAt: protection.LastShieldActivatedAt));
    }

    [HttpPost("shield/activate")]
    public async Task<ActionResult<ActivateShieldResponse>> ActivateShield(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        // Check if user has shield feature available
        var hasShieldFeature = await _premiumFeatureService.HasFeatureAsync(userId, Core.Domain.Enums.PremiumFeature.StreakShield);

        var result = await _streakProtectionService.ActivateShieldAsync(userId, cancellationToken);

        if (!result)
        {
            return BadRequest(new ActivateShieldResponse(
                Success: false,
                Message: "Nelze aktivovat shield. Nemáte dostupné shieldy nebo je již aktivní.",
                RemainingShields: 0));
        }

        var protection = await _streakProtectionService.GetProtectionAsync(userId, cancellationToken);

        return Ok(new ActivateShieldResponse(
            Success: true,
            Message: "Shield byl úspěšně aktivován!",
            RemainingShields: protection?.ShieldsRemaining ?? 0));
    }

    [HttpPost("shield/purchase")]
    public async Task<ActionResult<PurchaseShieldsResponse>> PurchaseShields(
        [FromBody] PurchaseShieldsRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        // Standard price: 3 shields for 500 coins
        var coinCost = request.Quantity == 3 ? 500 : (request.Quantity * 170);

        var result = await _streakProtectionService.PurchaseShieldsAsync(userId, request.Quantity, coinCost, cancellationToken);

        if (!result)
        {
            return BadRequest(new PurchaseShieldsResponse(
                Success: false,
                Message: "Nepodařilo se zakoupit shieldy.",
                TotalShields: 0,
                RemainingCoins: 0));
        }

        var protection = await _streakProtectionService.GetProtectionAsync(userId, cancellationToken);

        return Ok(new PurchaseShieldsResponse(
            Success: true,
            Message: $"Úspěšně zakoupeno {request.Quantity} shieldů!",
            TotalShields: protection?.ShieldsRemaining ?? 0,
            RemainingCoins: 0)); // TODO: Get actual coin balance from user
    }

    [HttpPost("shield/emergency")]
    public async Task<ActionResult<EmergencyShieldResponse>> PurchaseEmergencyShield(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        // Check if user is premium
        var isPremium = await _premiumFeatureService.IsPremiumAsync(userId);
        if (!isPremium)
        {
            return Forbid();
        }

        var coinCost = 300;
        var result = await _streakProtectionService.PurchaseEmergencyShieldAsync(userId, coinCost, cancellationToken);

        if (!result)
        {
            return BadRequest(new EmergencyShieldResponse(
                Success: false,
                Message: "Nepodařilo se zakoupit emergency shield.",
                IsShieldActive: false,
                RemainingCoins: 0));
        }

        return Ok(new EmergencyShieldResponse(
            Success: true,
            Message: "Emergency shield byl úspěšně aktivován!",
            IsShieldActive: true,
            RemainingCoins: 0)); // TODO: Get actual coin balance from user
    }

    [HttpGet("shield/can-activate-free")]
    public async Task<ActionResult<bool>> CanActivateFreeShield(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var isPremium = await _premiumFeatureService.IsPremiumAsync(userId);

        var result = await _streakProtectionService.CanActivateFreeShieldAsync(userId, isPremium, cancellationToken);
        return Ok(result);
    }

}
