using LexiQuest.Api.Extensions;
using LexiQuest.Core.Domain.Enums;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Premium;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/premium")]
[Authorize]
public class PremiumController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly IPremiumFeatureService _premiumFeatureService;

    public PremiumController(
        ISubscriptionService subscriptionService,
        IPremiumFeatureService premiumFeatureService)
    {
        _subscriptionService = subscriptionService;
        _premiumFeatureService = premiumFeatureService;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> CreateCheckout(
        [FromBody] CreateCheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "";
        
        var checkoutUrl = await _subscriptionService.CreateCheckoutSessionAsync(
            userId, 
            (Core.Domain.Enums.SubscriptionPlan)request.Plan, 
            email,
            cancellationToken);
        
        return Ok(new CheckoutResponse(checkoutUrl));
    }

    [HttpGet("status")]
    public async Task<ActionResult<SubscriptionStatusDto>> GetStatus(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var subscription = await _subscriptionService.GetActiveSubscriptionAsync(userId, cancellationToken);
        
        if (subscription == null)
        {
            return Ok(new SubscriptionStatusDto(
                IsActive: false,
                Plan: null,
                ExpiresAt: null,
                Status: Shared.DTOs.Premium.SubscriptionStatus.Expired));
        }

        return Ok(new SubscriptionStatusDto(
            IsActive: subscription.IsActive,
            Plan: (Shared.DTOs.Premium.SubscriptionPlan)subscription.Plan,
            ExpiresAt: subscription.ExpiresAt,
            Status: (Shared.DTOs.Premium.SubscriptionStatus)subscription.Status));
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _subscriptionService.CancelSubscriptionAsync(userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("features")]
    public async Task<ActionResult<IEnumerable<PremiumFeatureDto>>> GetFeatures(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var isPremium = await _subscriptionService.IsPremiumAsync(userId, cancellationToken);
        
        var features = new List<PremiumFeatureDto>
        {
            new("NoAds", "Bez reklam", isPremium),
            new("StreakFreeze", "Streak Freeze - automatická ochrana", isPremium),
            new("StreakShield", "Streak Shield - manuální ochrana", isPremium),
            new("DoubleXPWeekends", "2x XP o víkendech", isPremium),
            new("ExclusivePaths", "Exkluzivní cesty", isPremium),
            new("CustomDictionaries", "Vlastní slovníky", isPremium),
            new("DetailedStats", "Detailní statistiky", isPremium),
            new("CustomAvatar", "Vlastní avatar", isPremium),
            new("DiamondLeague", "Diamantová liga", isPremium),
            new("TeamCreation", "Vytváření týmů", isPremium)
        };

        return Ok(features);
    }

}
