namespace LexiQuest.Shared.DTOs.Game;

/// <summary>
/// Request to start a guest game session.
/// </summary>
public record GuestStartRequest();

/// <summary>
/// Response from starting a guest game session.
/// </summary>
public record GuestStartResponse(
    Guid SessionId,
    List<GuestScrambledWordDto> ScrambledWords,
    int RemainingGames,
    string Message
);

/// <summary>
/// Scrambled word info for guest sessions.
/// </summary>
public record GuestScrambledWordDto(
    Guid WordId,
    string Scrambled,
    int Length
);

/// <summary>
/// Request to submit an answer in guest mode.
/// </summary>
public record GuestAnswerRequest(
    Guid SessionId,
    Guid WordId,
    string Answer
);

/// <summary>
/// Response from submitting an answer in guest mode.
/// </summary>
public record GuestAnswerResponse(
    bool IsCorrect,
    int XpEarned,
    string CorrectAnswer,
    string? UserAnswer,
    int TotalSessionXp,
    int WordsSolved,
    int WordsRemaining,
    bool IsGameComplete
);

/// <summary>
/// Response with guest game status.
/// </summary>
public record GuestStatusResponse(
    int TotalAllowed,
    int Used,
    int Remaining,
    DateTime? ResetTime
);

/// <summary>
/// Request to convert guest progress to registered account.
/// </summary>
public record GuestConvertRequest(
    Guid SessionId,
    string Email,
    string Username,
    string Password,
    bool TransferProgress
);

/// <summary>
/// Response from converting guest to registered account.
/// </summary>
public record GuestConvertResponse(
    bool Success,
    string? UserId,
    string? Message,
    int TransferredXp,
    int TransferredWordsSolved
);

/// <summary>
/// Progress information for a guest session.
/// </summary>
public record GuestSessionProgress(
    int TotalXp,
    int WordsSolved
);
