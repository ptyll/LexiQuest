using System.Security.Claims;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Multiplayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/multiplayer")]
[Authorize]
public class MultiplayerController : ControllerBase
{
    private readonly IMatchHistoryService _matchHistoryService;

    public MultiplayerController(IMatchHistoryService matchHistoryService)
    {
        _matchHistoryService = matchHistoryService;
    }

    /// <summary>
    /// Gets match history for the current user.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(MatchHistoryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MatchHistoryResponseDto>> GetHistory(
        [FromQuery] MatchHistoryFilter filter = MatchHistoryFilter.All,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var history = await _matchHistoryService.GetMatchHistoryAsync(
            userId.Value, filter, pageNumber, pageSize, cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Gets multiplayer statistics for the current user.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(MultiplayerStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MultiplayerStatsDto>> GetStats(
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var stats = await _matchHistoryService.GetStatsAsync(userId.Value, cancellationToken);
        return Ok(stats);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}
