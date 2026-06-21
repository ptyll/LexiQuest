using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Interfaces;
using LexiQuest.Core.Interfaces.Repositories;
using LexiQuest.Core.Interfaces.Services;
using LexiQuest.Shared.DTOs.Leagues;
using LexiQuest.Shared.Enums;
using Microsoft.Extensions.Localization;

namespace LexiQuest.Core.Services;

public class LeagueService : ILeagueService
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly IUserRepository? _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<LeagueService> _localizer;

    public LeagueService(
        ILeagueRepository leagueRepository,
        IUnitOfWork unitOfWork,
        IStringLocalizer<LeagueService> localizer,
        IUserRepository? userRepository = null)
    {
        _leagueRepository = leagueRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task AssignUserToLeagueAsync(Guid userId, DateTime weekStart, DateTime weekEnd, CancellationToken cancellationToken = default)
    {
        await AssignUserToLeagueAsync(userId, LeagueTier.Bronze, weekStart, weekEnd, cancellationToken);
    }

    public async Task AssignUserToLeagueAsync(Guid userId, LeagueTier tier, DateTime weekStart, DateTime weekEnd, CancellationToken cancellationToken = default)
    {
        var league = await _leagueRepository.GetActiveLeagueForTierAndWeekAsync(tier, weekStart, cancellationToken);
        
        if (league == null || league.IsFull)
        {
            league = League.Create(tier, weekStart, weekEnd);
            league.AddParticipant(userId);
            await _leagueRepository.AddAsync(league, cancellationToken);
        }
        else
        {
            if (league.Participants.Any(p => p.UserId == userId))
                throw new InvalidOperationException("User is already in this league");

            await _leagueRepository.AddParticipantAsync(league.Id, userId, cancellationToken);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<LeagueInfoDto?> GetCurrentLeagueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var league = await _leagueRepository.GetActiveLeagueForUserAsync(userId, cancellationToken);
        
        if (league == null)
            return null;

        var participant = league.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            return null;

        league.UpdateRanks();

        var (promotionThreshold, demotionThreshold) = GetThresholds(league.Tier, league.Participants.Count);

        return new LeagueInfoDto(
            LeagueId: league.Id,
            Tier: league.Tier,
            WeekStart: league.WeekStart,
            WeekEnd: league.WeekEnd,
            CurrentRank: participant.Rank,
            TotalParticipants: league.Participants.Count,
            UserXP: participant.WeeklyXP,
            PromotionThreshold: promotionThreshold,
            DemotionThreshold: demotionThreshold,
            XPReward: GetRewards(league.Tier)
        );
    }

    public async Task AddXPAsync(Guid userId, int xp, CancellationToken cancellationToken = default)
    {
        var league = await _leagueRepository.GetActiveLeagueForUserAsync(userId, cancellationToken);
        
        if (league == null)
            throw new InvalidOperationException(_localizer["Error.NotInLeague"]);

        var participant = league.Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant == null)
            throw new InvalidOperationException(_localizer["Error.NotInLeague"]);

        participant.AddXP(xp);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<LeagueParticipantDto>> GetLeaderboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var league = await _leagueRepository.GetActiveLeagueForUserAsync(userId, cancellationToken);
        
        if (league == null)
            return new List<LeagueParticipantDto>();

        league.UpdateRanks();
        var (promotionThreshold, demotionThreshold) = GetThresholds(league.Tier, league.Participants.Count);
        var hasDemotionZone = demotionThreshold <= league.Participants.Count;
        var usernames = await GetUsernamesAsync(league.Participants.Select(p => p.UserId), cancellationToken);

        return league.Participants
            .OrderBy(p => p.Rank)
            .Select(p => new LeagueParticipantDto(
                UserId: p.UserId,
                Username: usernames.GetValueOrDefault(p.UserId, ""),
                AvatarUrl: null,
                Rank: p.Rank,
                WeeklyXP: p.WeeklyXP,
                IsCurrentUser: p.UserId == userId,
                IsPromoted: p.Rank <= promotionThreshold,
                IsDemoted: hasDemotionZone && p.Rank >= demotionThreshold
            ))
            .ToList();
    }

    private async Task<Dictionary<Guid, string>> GetUsernamesAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken)
    {
        if (_userRepository is null)
        {
            return new Dictionary<Guid, string>();
        }

        var result = new Dictionary<Guid, string>();
        foreach (var userId in userIds.Distinct())
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user is not null)
            {
                result[userId] = user.Username;
            }
        }

        return result;
    }

    public Task CalculatePromotionsAndDemotionsAsync(League league, CancellationToken cancellationToken = default)
    {
        league.UpdateRanks();

        var (promotionCount, demotionCount) = GetPromotionDemotionCounts(league.Tier, league.Participants.Count);

        // Mark promotions
        var topParticipants = league.GetTopParticipants(promotionCount);
        foreach (var participant in topParticipants)
        {
            participant.MarkAsPromoted();
        }

        // Mark demotions
        var bottomParticipants = league.GetBottomParticipants(demotionCount);
        foreach (var participant in bottomParticipants)
        {
            participant.MarkAsDemoted();
        }

        return Task.CompletedTask;
    }

    public int GetRewards(LeagueTier tier)
    {
        return tier switch
        {
            LeagueTier.Bronze => 50,
            LeagueTier.Silver => 100,
            LeagueTier.Gold => 200,
            LeagueTier.Diamond => 500,
            LeagueTier.Legend => 1000,
            _ => 0
        };
    }

    public async Task<List<LeagueHistoryDto>> GetLeagueHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var leagues = await _leagueRepository.GetLeagueHistoryForUserAsync(userId, cancellationToken);

        return leagues
            .Select(league => (League: league, Participant: league.Participants.FirstOrDefault(p => p.UserId == userId)))
            .Where(entry => entry.Participant is not null)
            .Select(entry => new LeagueHistoryDto(
                LeagueId: entry.League.Id,
                Tier: entry.League.Tier,
                WeekStart: entry.League.WeekStart,
                WeekEnd: entry.League.WeekEnd,
                FinalRank: entry.Participant!.Rank,
                WeeklyXP: entry.Participant.WeeklyXP,
                ChangeStatus: GetChangeStatus(entry.Participant),
                XPEarned: entry.Participant.IsPromoted ? GetRewards(entry.League.Tier) : 0
            ))
            .ToList();
    }

    private static LeagueChangeStatus GetChangeStatus(LeagueParticipant participant)
    {
        if (participant.IsPromoted) return LeagueChangeStatus.Promoted;
        if (participant.IsDemoted) return LeagueChangeStatus.Demoted;
        return LeagueChangeStatus.Stayed;
    }

    private static (int PromotionCount, int DemotionCount) GetPromotionDemotionCounts(LeagueTier tier, int participantCount)
    {
        var requestedPromotionCount = tier switch
        {
            LeagueTier.Legend => 3,
            _ => 5
        };
        var requestedDemotionCount = tier switch
        {
            LeagueTier.Legend => Math.Min(10, participantCount / 2),
            _ => 5
        };

        var promotionCount = Math.Min(requestedPromotionCount, participantCount);
        var remainingAfterPromotion = Math.Max(0, participantCount - promotionCount);
        var demotionCount = Math.Min(requestedDemotionCount, remainingAfterPromotion);

        return (promotionCount, demotionCount);
    }

    private static (int PromotionThreshold, int DemotionThreshold) GetThresholds(LeagueTier tier, int participantCount)
    {
        var (promotionCount, demotionCount) = GetPromotionDemotionCounts(tier, participantCount);
        return (promotionCount, participantCount - demotionCount + 1);
    }
}
