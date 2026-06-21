using LexiQuest.Api.Extensions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/paths")]
public class PathsController : ControllerBase
{
    private readonly IPathService _pathService;

    public PathsController(IPathService pathService)
    {
        _pathService = pathService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<LearningPathDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<LearningPathDto>>> GetPaths(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var paths = await _pathService.GetPathsAsync(userId, cancellationToken);
        return Ok(paths);
    }

    [HttpGet("{pathId:guid}/progress")]
    [ProducesResponseType(typeof(PathProgressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PathProgressDto>> GetPathProgress(Guid pathId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        try
        {
            return Ok(await _pathService.GetPathProgressAsync(userId, pathId, cancellationToken));
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}
