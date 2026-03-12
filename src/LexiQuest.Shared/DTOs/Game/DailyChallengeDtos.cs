using LexiQuest.Shared.Enums;

namespace LexiQuest.Shared.DTOs.Game;

public record DailyChallengeDto(
    DateTime Date,
    Guid WordId,
    DailyModifier Modifier,
    string ModifierDescription,
    int XPMultiplier
);

public record ChallengeResultDto(
    bool IsCorrect,
    string CorrectAnswer,
    int XPEarned,
    TimeSpan TimeTaken,
    int Rank
);

public record DailyLeaderboardEntryDto(
    Guid UserId,
    string Username,
    string? AvatarUrl,
    TimeSpan TimeTaken,
    int XPEarned,
    int Rank,
    bool IsCurrentUser
);
