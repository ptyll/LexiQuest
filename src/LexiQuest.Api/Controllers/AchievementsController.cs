using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Achievements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/achievements")]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public AchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AchievementDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var achievements = await _achievementService.GetUserAchievementsAsync(userId, cancellationToken);
        return Ok(achievements);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AchievementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AchievementDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var achievements = await _achievementService.GetUserAchievementsAsync(userId, cancellationToken);
        var achievement = achievements.FirstOrDefault(a => a.Id == id);
        
        if (achievement == null)
            return NotFound();

        return Ok(achievement);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Guid.Empty;
        return userId;
    }
}

[ApiController]
[Route("api/v1/users/me/achievements")]
[Authorize]
public class UserAchievementsController : ControllerBase
{
    private readonly IAchievementService _achievementService;

    public UserAchievementsController(IAchievementService achievementService)
    {
        _achievementService = achievementService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<AchievementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AchievementDto>>> GetMyAchievements(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var achievements = await _achievementService.GetUserAchievementsAsync(userId, cancellationToken);
        return Ok(achievements);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Guid.Empty;
        return userId;
    }
}
