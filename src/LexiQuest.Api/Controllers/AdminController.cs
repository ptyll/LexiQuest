using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IDailyChallengeRepository _dailyChallengeRepository;
    private readonly IAdminWordService _adminWordService;

    public AdminController(
        IUserRepository userRepository,
        IDailyChallengeRepository dailyChallengeRepository,
        IAdminWordService adminWordService)
    {
        _userRepository = userRepository;
        _dailyChallengeRepository = dailyChallengeRepository;
        _adminWordService = adminWordService;
    }

    [HttpGet("check")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public ActionResult<bool> Check()
    {
        return Ok(User.IsInRole("Admin"));
    }

    [HttpGet("check/words")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public ActionResult<bool> CheckWords()
    {
        return Ok(User.IsInRole("Admin") || User.IsInRole("ContentManager"));
    }

    [HttpGet("dashboard/stats")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(AdminDashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminDashboardStatsDto>> GetDashboardStats(CancellationToken cancellationToken)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var wordStats = await _adminWordService.GetStatsAsync(cancellationToken);

        var stats = new AdminDashboardStatsDto(
            await _userRepository.CountAsync(cancellationToken),
            await _userRepository.CountActiveSinceAsync(todayUtc, cancellationToken),
            wordStats.TotalWords,
            await _dailyChallengeRepository.CountAsync(cancellationToken));

        return Ok(stats);
    }
}
