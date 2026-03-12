using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Leagues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LexiQuest.Api.Controllers;

[ApiController]
[Route("api/v1/leagues")]
[Authorize]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueService _leagueService;

    public LeaguesController(ILeagueService leagueService)
    {
        _leagueService = leagueService;
    }

    [HttpGet("current")]
    [ProducesResponseType(typeof(LeagueInfoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeagueInfoDto>> GetCurrent(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var league = await _leagueService.GetCurrentLeagueAsync(userId.Value, cancellationToken);
        
        if (league == null)
            return NotFound();

        return Ok(league);
    }

    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(List<LeagueParticipantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LeagueParticipantDto>>> GetLeaderboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var leaderboard = await _leagueService.GetLeaderboardAsync(userId.Value, cancellationToken);
        return Ok(leaderboard);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(List<LeagueHistoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LeagueHistoryDto>>> GetHistory(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var history = await _leagueService.GetLeagueHistoryAsync(userId.Value, cancellationToken);
        return Ok(history);
    }

    [HttpGet("rewards")]
    [ProducesResponseType(typeof(List<LeagueRewardsDto>), StatusCodes.Status200OK)]
    public ActionResult<List<LeagueRewardsDto>> GetRewards()
    {
        var rewards = Enum.GetValues<Shared.Enums.LeagueTier>()
            .Select(tier => new LeagueRewardsDto(
                Tier: tier,
                XPReward: _leagueService.GetRewards(tier),
                Description: GetTierDescription(tier)
            ))
            .ToList();

        return Ok(rewards);
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return null;
        return userId;
    }

    private static string GetTierDescription(Shared.Enums.LeagueTier tier)
    {
        return tier switch
        {
            Shared.Enums.LeagueTier.Bronze => "Základní liga pro nové hráče",
            Shared.Enums.LeagueTier.Silver => "Stříbrná liga pro pokročilé",
            Shared.Enums.LeagueTier.Gold => "Zlatá liga pro zkušené",
            Shared.Enums.LeagueTier.Diamond => "Diamantová liga pro experty",
            Shared.Enums.LeagueTier.Legend => "Legendární liga pro mistry",
            _ => ""
        };
    }
}
