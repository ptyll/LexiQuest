using LexiQuest.Api.Extensions;
using LexiQuest.Api.Testing;
using LexiQuest.Core.Domain.ValueObjects;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using LexiQuest.Shared.DTOs.Stats;
using LexiQuest.Shared.DTOs.Streak;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/stats")]
public class StatsController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILevelCalculator _levelCalculator;
    private readonly IStreakService _streakService;
    private readonly IStreakProtectionService _streakProtectionService;
    private readonly IPremiumFeatureService _premiumFeatureService;
    private readonly E2EStatsRuntimeSettings? _e2eStatsSettings;

    public StatsController(
        IUserRepository userRepository,
        ILevelCalculator levelCalculator,
        IStreakService streakService,
        IStreakProtectionService streakProtectionService,
        IPremiumFeatureService premiumFeatureService,
        IWebHostEnvironment environment,
        IServiceProvider serviceProvider)
    {
        _userRepository = userRepository;
        _levelCalculator = levelCalculator;
        _streakService = streakService;
        _streakProtectionService = streakProtectionService;
        _premiumFeatureService = premiumFeatureService;
        _e2eStatsSettings = environment.IsEnvironment("E2E")
            ? serviceProvider.GetService<E2EStatsRuntimeSettings>()
            : null;
    }

    [HttpGet("user")]
    [ProducesResponseType(typeof(UserStatsSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserStatsSummaryDto>> GetCurrentUserStats(CancellationToken cancellationToken)
    {
        var e2eDelay = _e2eStatsSettings?.ConsumeDelayForUserStatsRequest();
        if (e2eDelay is not null)
        {
            await e2eDelay.WaitAsync(cancellationToken);
        }

        if (_e2eStatsSettings?.ConsumeFailNextUserStatsRequest() == true)
        {
            return Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "E2E forced dashboard stats failure");
        }

        Guid userId;
        try
        {
            userId = User.GetUserId();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var now = DateTime.UtcNow;
        var isPremium = await _premiumFeatureService.IsPremiumAsync(userId);
        var protection = await _streakProtectionService.GetProtectionAsync(userId, cancellationToken);
        var canActivateFreeShield = await _streakProtectionService.CanActivateFreeShieldAsync(
            userId,
            isPremium,
            cancellationToken);

        return Ok(new UserStatsSummaryDto
        {
            TotalXP = user.Stats.TotalXP,
            CurrentLevel = user.Stats.Level,
            CurrentStreak = user.Streak.CurrentDays,
            LongestStreak = user.Streak.LongestDays,
            Accuracy = user.Stats.Accuracy,
            AverageTime = FormatAverageTime(user.Stats.AverageResponseTime),
            TotalWordsSolved = user.Stats.TotalWordsSolved,
            XpProgress = _levelCalculator.GetXpProgress(user.Stats.TotalXP),
            StreakStatus = CreateStreakStatus(user.Streak, now),
            StreakProtection = CreateStreakProtectionDto(protection, isPremium, canActivateFreeShield),
            IsPremium = isPremium
        });
    }

    private static string FormatAverageTime(TimeSpan value)
    {
        if (value <= TimeSpan.Zero)
        {
            return "0 s";
        }

        return value.TotalSeconds < 10
            ? $"{value.TotalSeconds:0.0} s"
            : $"{Math.Round(value.TotalSeconds)} s";
    }

    private StreakStatus CreateStreakStatus(Streak streak, DateTime now)
    {
        DateTime? nextResetAt = null;
        TimeSpan? timeRemaining = null;

        if (streak.CurrentDays > 0 && streak.LastActivityDate.HasValue)
        {
            nextResetAt = streak.LastActivityDate.Value.AddHours(48);
            timeRemaining = nextResetAt.Value - now;
            if (timeRemaining < TimeSpan.Zero)
            {
                timeRemaining = TimeSpan.Zero;
            }
        }

        var isAtRisk = streak.CurrentDays > 0
            && streak.LastActivityDate.HasValue
            && streak.LastActivityDate.Value.Date < now.Date
            && timeRemaining > TimeSpan.Zero;

        return new StreakStatus(
            CurrentDays: streak.CurrentDays,
            LongestDays: streak.LongestDays,
            FireLevel: _streakService.GetFireLevel(streak.CurrentDays).ToString(),
            NextResetAt: nextResetAt,
            TimeRemaining: timeRemaining,
            IsAtRisk: isAtRisk);
    }

    private static StreakProtectionDto CreateStreakProtectionDto(
        Core.Domain.Entities.StreakProtection? protection,
        bool isPremium,
        bool canActivateFreeShield)
    {
        var nextShieldAvailableAt = protection?.LastShieldActivatedAt?.AddDays(isPremium ? 7 : 30);

        return new StreakProtectionDto(
            ShieldsRemaining: protection?.ShieldsRemaining ?? 0,
            HasActiveShield: protection?.IsShieldActive ?? false,
            FreezeUsedThisWeek: protection?.FreezeUsedThisWeek ?? false,
            NextShieldAvailableAt: nextShieldAvailableAt,
            CanActivateFreeShield: canActivateFreeShield);
    }
}
