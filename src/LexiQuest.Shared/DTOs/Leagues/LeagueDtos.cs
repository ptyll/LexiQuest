using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Leagues;

public record LeagueInfoDto(
    Guid LeagueId,
    LeagueTier Tier,
    DateTime WeekStart,
    DateTime WeekEnd,
    int CurrentRank,
    int TotalParticipants,
    int UserXP,
    int PromotionThreshold,
    int DemotionThreshold,
    int XPReward
);

public record LeagueParticipantDto(
    Guid UserId,
    string Username,
    string? AvatarUrl,
    int Rank,
    int WeeklyXP,
    bool IsCurrentUser,
    bool IsPromoted,
    bool IsDemoted
);

public record LeagueLeaderboardDto(
    List<LeagueParticipantDto> Participants,
    int TotalParticipants,
    DateTime WeekEndsAt,
    TimeSpan TimeRemaining
);

public record LeagueHistoryDto(
    Guid LeagueId,
    LeagueTier Tier,
    DateTime WeekStart,
    DateTime WeekEnd,
    int FinalRank,
    int WeeklyXP,
    LeagueChangeStatus ChangeStatus,
    int XPEarned
);

public record LeagueRewardsDto(
    LeagueTier Tier,
    int XPReward,
    string Description
);
