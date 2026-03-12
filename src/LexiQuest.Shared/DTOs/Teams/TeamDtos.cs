namespace LexiQuest.Shared.DTOs.Teams;

/// <summary>
/// DTO for team information.
/// </summary>
public record TeamDto(
    Guid Id,
    string Name,
    string Tag,
    string? Description,
    string? LogoUrl,
    Guid LeaderId,
    string LeaderUsername,
    DateTime CreatedAt,
    int MemberCount,
    TeamStatsDto Stats);

/// <summary>
/// DTO for team member information.
/// </summary>
public record TeamMemberDto(
    Guid UserId,
    string Username,
    string? AvatarUrl,
    TeamRoleDto Role,
    DateTime JoinedAt,
    int WeeklyXP,
    long AllTimeXP);

/// <summary>
/// DTO for team statistics.
/// </summary>
public record TeamStatsDto(
    int WeeklyXP,
    long AllTimeXP,
    int Rank,
    int TotalWins,
    int MatchesPlayed,
    int WinRatePercentage);

/// <summary>
/// DTO for team ranking entry.
/// </summary>
public record TeamRankingDto(
    Guid TeamId,
    string Name,
    string Tag,
    int Rank,
    int WeeklyXP,
    long AllTimeXP,
    int MemberCount);

/// <summary>
/// Request to create a new team.
/// </summary>
public record CreateTeamRequest(
    string Name,
    string Tag,
    string? Description,
    string? LogoUrl);

/// <summary>
/// Request to invite a member to a team.
/// </summary>
public record InviteMemberRequest(
    Guid UserId);

/// <summary>
/// Request to create a join request.
/// </summary>
public record CreateJoinRequest(
    string? Message);

/// <summary>
/// DTO for team invite.
/// </summary>
public record TeamInviteDto(
    Guid Id,
    Guid TeamId,
    string TeamName,
    string TeamTag,
    Guid InvitedByUserId,
    string InvitedByUsername,
    InviteStatusDto Status,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    bool IsExpired);

/// <summary>
/// DTO for team join request.
/// </summary>
public record TeamJoinRequestDto(
    Guid Id,
    Guid TeamId,
    Guid UserId,
    string Username,
    string? Message,
    JoinRequestStatusDto Status,
    DateTime CreatedAt);

/// <summary>
/// Team role DTO.
/// </summary>
public enum TeamRoleDto
{
    Member = 0,
    Officer = 1,
    Leader = 2
}

/// <summary>
/// Invite status DTO.
/// </summary>
public enum InviteStatusDto
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Cancelled = 3,
    Expired = 4
}

/// <summary>
/// Join request status DTO.
/// </summary>
public enum JoinRequestStatusDto
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
