using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace LexiQuest.Core.Jobs;

public class LeagueResetJob
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly ILeagueService _leagueService;
    private readonly ILogger<LeagueResetJob> _logger;

    public LeagueResetJob(ILeagueRepository leagueRepository, ILeagueService leagueService, ILogger<LeagueResetJob> logger)
    {
        _leagueRepository = leagueRepository;
        _leagueService = leagueService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting weekly league reset job");

        var activeLeagues = await _leagueRepository.GetActiveLeaguesAsync(cancellationToken);
        var weekStart = GetWeekStart();
        var weekEnd = weekStart.AddDays(7);

        foreach (var league in activeLeagues)
        {
            await ProcessLeagueAsync(league, weekStart, weekEnd, cancellationToken);
        }

        _logger.LogInformation("Weekly league reset job completed");
    }

    private async Task ProcessLeagueAsync(League league, DateTime weekStart, DateTime weekEnd, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing league {LeagueId} (Tier: {Tier})", league.Id, league.Tier);

        // Calculate promotions and demotions
        await _leagueService.CalculatePromotionsAndDemotionsAsync(league, cancellationToken);

        // Deactivate current league
        league.Deactivate();

        // Process participants
        var promotedUsers = league.Participants.Where(p => p.IsPromoted).ToList();
        var demotedUsers = league.Participants.Where(p => p.IsDemoted).ToList();
        var stayingUsers = league.Participants.Where(p => !p.IsPromoted && !p.IsDemoted).ToList();

        // Assign promoted users to higher tier
        foreach (var user in promotedUsers)
        {
            var nextTier = GetNextTier(league.Tier);
            _logger.LogInformation("Promoting user {UserId} to {Tier}", user.UserId, nextTier);
            await _leagueService.AssignUserToLeagueAsync(user.UserId, weekStart, weekEnd, cancellationToken);
        }

        // Assign demoted users to lower tier
        foreach (var user in demotedUsers)
        {
            var previousTier = GetPreviousTier(league.Tier);
            _logger.LogInformation("Demoting user {UserId} to {Tier}", user.UserId, previousTier);
            await _leagueService.AssignUserToLeagueAsync(user.UserId, weekStart, weekEnd, cancellationToken);
        }

        // Assign staying users to same tier
        foreach (var user in stayingUsers)
        {
            _logger.LogInformation("Keeping user {UserId} in {Tier}", user.UserId, league.Tier);
            await _leagueService.AssignUserToLeagueAsync(user.UserId, weekStart, weekEnd, cancellationToken);
        }

        // Reset weekly XP for all participants
        foreach (var participant in league.Participants)
        {
            participant.ResetWeeklyXP();
        }

        _logger.LogInformation("Processed league {LeagueId}: {Promoted} promoted, {Demoted} demoted, {Stayed} stayed",
            league.Id, promotedUsers.Count, demotedUsers.Count, stayingUsers.Count);
    }

    private static LeagueTier GetNextTier(LeagueTier current)
    {
        return current switch
        {
            LeagueTier.Bronze => LeagueTier.Silver,
            LeagueTier.Silver => LeagueTier.Gold,
            LeagueTier.Gold => LeagueTier.Diamond,
            LeagueTier.Diamond => LeagueTier.Legend,
            LeagueTier.Legend => LeagueTier.Legend,
            _ => LeagueTier.Bronze
        };
    }

    private static LeagueTier GetPreviousTier(LeagueTier current)
    {
        return current switch
        {
            LeagueTier.Legend => LeagueTier.Diamond,
            LeagueTier.Diamond => LeagueTier.Gold,
            LeagueTier.Gold => LeagueTier.Silver,
            LeagueTier.Silver => LeagueTier.Bronze,
            LeagueTier.Bronze => LeagueTier.Bronze,
            _ => LeagueTier.Bronze
        };
    }

    private static DateTime GetWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        return today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
    }
}
