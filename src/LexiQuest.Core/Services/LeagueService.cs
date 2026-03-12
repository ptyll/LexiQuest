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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStringLocalizer<LeagueService> _localizer;

    public LeagueService(ILeagueRepository leagueRepository, IUnitOfWork unitOfWork, IStringLocalizer<LeagueService> localizer)
    {
        _leagueRepository = leagueRepository;
        _unitOfWork = unitOfWork;
        _localizer = localizer;
    }

    public async Task AssignUserToLeagueAsync(Guid userId, DateTime weekStart, DateTime weekEnd, CancellationToken cancellationToken = default)
    {
        // Find an active Bronze league with space
        var league = await _leagueRepository.GetActiveLeagueForTierAsync(LeagueTier.Bronze, cancellationToken);
        
        if (league == null || league.IsFull)
        {
            // Create new league
            league = League.Create(LeagueTier.Bronze, weekStart, weekEnd);
            await _leagueRepository.AddAsync(league, cancellationToken);
        }

        league.AddParticipant(userId);
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

        return league.Participants
            .OrderBy(p => p.Rank)
            .Select(p => new LeagueParticipantDto(
                UserId: p.UserId,
                Username: "", // Will be filled by caller with user data
                AvatarUrl: null,
                Rank: p.Rank,
                WeeklyXP: p.WeeklyXP,
                IsCurrentUser: p.UserId == userId,
                IsPromoted: p.IsPromoted,
                IsDemoted: p.IsDemoted
            ))
            .ToList();
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
        // This would require a new repository method for historical data
        // For now, return empty list
        return new List<LeagueHistoryDto>();
    }

    private static (int PromotionCount, int DemotionCount) GetPromotionDemotionCounts(LeagueTier tier, int participantCount)
    {
        return tier switch
        {
            LeagueTier.Legend => (3, Math.Min(10, participantCount / 2)),
            _ => (5, 5)
        };
    }

    private static (int PromotionThreshold, int DemotionThreshold) GetThresholds(LeagueTier tier, int participantCount)
    {
        var (promotionCount, demotionCount) = GetPromotionDemotionCounts(tier, participantCount);
        return (promotionCount, participantCount - demotionCount + 1);
    }
}
