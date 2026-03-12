using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/challenges/ai")]
[Authorize]
public class AIChallengeController : ControllerBase
{
    private readonly IAIChallengeService _aiChallengeService;

    public AIChallengeController(IAIChallengeService aiChallengeService)
    {
        _aiChallengeService = aiChallengeService;
    }

    [HttpGet("analysis")]
    [ProducesResponseType(typeof(PlayerAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PlayerAnalysisDto>> GetAnalysis(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var analysis = await _aiChallengeService.AnalyzePlayerAsync(userId, cancellationToken);
        return Ok(analysis);
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(AIChallengeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AIChallengeDto>> StartChallenge(
        [FromBody] AIChallengeRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        var challenge = await _aiChallengeService.GenerateChallengeAsync(userId, request, cancellationToken);
        return Ok(challenge);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Guid.Empty;
        return userId;
    }
}
