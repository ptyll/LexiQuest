using LexiQuest.Api.Extensions;
using LexiQuest.Core.Interfaces.Repositories;
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
    private readonly IWordRepository _wordRepository;
    private readonly TimeProvider _timeProvider;

    public DailyChallengeController(
        IDailyChallengeService dailyChallengeService,
        IWordRepository wordRepository,
        TimeProvider timeProvider)
    {
        _dailyChallengeService = dailyChallengeService;
        _wordRepository = wordRepository;
        _timeProvider = timeProvider;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DailyChallengeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DailyChallengeDto>> GetToday(CancellationToken cancellationToken)
    {
        var challenge = await _dailyChallengeService.GetOrCreateTodayAsync(cancellationToken);
        var dto = await CreateDtoAsync(challenge, cancellationToken);

        return Ok(dto);
    }

    [HttpGet("completed")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> HasCompletedToday(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        return Ok(await _dailyChallengeService.HasCompletedTodayAsync(userId.Value, cancellationToken));
    }

    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(List<DailyLeaderboardEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DailyLeaderboardEntryDto>>> GetLeaderboard(CancellationToken cancellationToken)
    {
        var today = GetToday();
        var entries = await _dailyChallengeService.GetLeaderboardAsync(today, cancellationToken);
        var currentUserId = GetCurrentUserId();

        var dtos = entries.Take(10).Select((e, index) => new DailyLeaderboardEntryDto(
            UserId: e.UserId,
            Username: e.Username,
            AvatarUrl: null,
            TimeTaken: e.TimeTaken,
            XPEarned: e.XPEarned,
            Rank: index + 1,
            IsCurrentUser: currentUserId.HasValue && e.UserId == currentUserId.Value
        )).ToList();

        return Ok(dtos);
    }

    [HttpPost("start")]
    [ProducesResponseType(typeof(GameSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameSessionResponse>> Start(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // This would integrate with GameSessionService to start a daily challenge session
        // For now, return a placeholder response
        return Ok(new GameSessionResponse(
            SessionId: Guid.NewGuid(),
            Message: "Daily challenge started"
        ));
    }

    [HttpPost("submit")]
    [ProducesResponseType(typeof(ChallengeResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChallengeResultDto>> Submit(
        DailyChallengeSubmitRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        try
        {
            var result = await _dailyChallengeService.SubmitAnswerAsync(
                userId.Value,
                GetToday(),
                request.Answer,
                TimeSpan.FromMilliseconds(Math.Max(0, request.TimeTakenMs)),
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("AlreadyCompleted", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Denní výzva už byla dokončena.",
                Detail = ex.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Denní výzvu se nepodařilo vyhodnotit.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    private async Task<DailyChallengeDto> CreateDtoAsync(Core.Domain.Entities.DailyChallenge challenge, CancellationToken cancellationToken)
    {
        var word = await _wordRepository.GetByIdAsync(challenge.WordId, cancellationToken);
        var scrambled = word is null
            ? string.Empty
            : ScrambleDeterministically(word.Original, challenge.Date);

        return new DailyChallengeDto(
            Date: challenge.Date,
            WordId: challenge.WordId,
            Modifier: challenge.Modifier,
            ModifierDescription: GetModifierDescription(challenge.Modifier),
            XPMultiplier: GetXPMultiplier(challenge.Modifier),
            ScrambledWord: scrambled,
            WordLength: word?.Length ?? 0
        );
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

    private Guid? GetCurrentUserId()
    {
        try
        {
            return User.GetUserId();
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    private DateTime GetToday() => _timeProvider.GetUtcNow().UtcDateTime.Date;

    private static string ScrambleDeterministically(string value, DateTime date)
    {
        var chars = value.ToCharArray();
        var seed = HashCode.Combine(value.ToUpperInvariant(), date.Date);
        var rng = new Random(seed);

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        var scrambled = new string(chars);
        return scrambled.Equals(value, StringComparison.OrdinalIgnoreCase)
            ? new string(chars.Reverse().ToArray())
            : scrambled;
    }
}

public record GameSessionResponse(Guid SessionId, string Message);
