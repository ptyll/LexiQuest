using LexiQuest.Api.Extensions;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/boss")]
[Authorize]
public class BossController : ControllerBase
{
    private readonly IBossGameService _bossGameService;

    public BossController(IBossGameService bossGameService)
    {
        _bossGameService = bossGameService;
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(BossSessionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BossSessionDto>> Start(
        BossStartRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var session = await _bossGameService.StartBossGameAsync(userId, request, cancellationToken);
            return Created($"/api/v1/boss/{session.Id}", session);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Boss level se nepodařilo spustit.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BossSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BossSessionDto>> GetState(Guid id, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var session = await _bossGameService.GetBossStateAsync(userId, id, cancellationToken);
        return session == null ? NotFound() : Ok(session);
    }

    [HttpPost("{id:guid}/answer")]
    [ProducesResponseType(typeof(BossRoundResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BossRoundResultDto>> SubmitAnswer(
        Guid id,
        BossAnswerRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _bossGameService.SubmitAnswerAsync(userId, id, request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Odpověď boss levelu se nepodařilo vyhodnotit.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpGet("{id:guid}/twist-reveal")]
    [ProducesResponseType(typeof(TwistRevealStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TwistRevealStateDto>> GetTwistReveal(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var state = await _bossGameService.GetTwistRevealStateAsync(userId, id, cancellationToken);
        return state == null ? NotFound() : Ok(state);
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        try
        {
            userId = User.GetUserId();
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            userId = Guid.Empty;
            return false;
        }
    }
}
