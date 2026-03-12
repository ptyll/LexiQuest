using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/game/daily")]
[Authorize]
public class DailyChallengeController : ControllerBase
{
    private readonly IDailyChallengeService _dailyChallengeService;

    public DailyChallengeController(IDailyChallengeService dailyChallengeService)
    {
        _dailyChallengeService = dailyChallengeService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DailyChallengeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyChallengeDto>> GetToday(CancellationToken cancellationToken)
    {
        var challenge = await _dailyChallengeService.GetOrCreateTodayAsync(cancellationToken);
        
        var dto = new DailyChallengeDto(
            Date: challenge.Date,
            WordId: challenge.WordId,
            Modifier: challenge.Modifier,
            ModifierDescription: GetModifierDescription(challenge.Modifier),
            XPMultiplier: GetXPMultiplier(challenge.Modifier)
        );

        return Ok(dto);
    }

    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(List<DailyLeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DailyLeaderboardEntryDto>>> GetLeaderboard(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var entries = await _dailyChallengeService.GetLeaderboardAsync(today, cancellationToken);

        var dtos = entries.Select((e, index) => new DailyLeaderboardEntryDto(
            UserId: e.UserId,
            Username: e.Username,
            AvatarUrl: null,
            TimeTaken: e.TimeTaken,
            XPEarned: e.XPEarned,
            Rank: index + 1,
            IsCurrentUser: e.UserId == GetCurrentUserId()
        )).ToList();

        return Ok(dtos);
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(GameSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameSessionResponse>> Start(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized();

        // This would integrate with GameSessionService to start a daily challenge session
        // For now, return a placeholder response
        return Ok(new GameSessionResponse(
            SessionId: Guid.NewGuid(),
            Message: "Daily challenge started"
        ));
    }

    private static string GetModifierDescription(Shared.Enums.DailyModifier modifier)
    {
        return modifier switch
        {
            Shared.Enums.DailyModifier.Category => "Slovo z konkrétní kategorie",
            Shared.Enums.DailyModifier.Speed => "Bonus za rychlost",
            Shared.Enums.DailyModifier.NoHints => "Žádné nápovědy",
            Shared.Enums.DailyModifier.DoubleLetters => "Dvojité body za písmena",
            Shared.Enums.DailyModifier.Team => "Týmová výzva",
            Shared.Enums.DailyModifier.Hard => "Obtížná slova",
            Shared.Enums.DailyModifier.Easy => "Jednoduchá slova",
            _ => ""
        };
    }

    private static int GetXPMultiplier(Shared.Enums.DailyModifier modifier)
    {
        return modifier switch
        {
            Shared.Enums.DailyModifier.Speed => 150,
            Shared.Enums.DailyModifier.Hard => 200,
            Shared.Enums.DailyModifier.NoHints => 130,
            Shared.Enums.DailyModifier.DoubleLetters => 140,
            _ => 100
        };
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Guid.Empty;
        return userId;
    }
}

public record GameSessionResponse(Guid SessionId, string Message);
