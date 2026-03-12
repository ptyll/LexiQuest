namespace LexiQuest.Shared.DTOs.Admin;

public record AdminWordDto(Guid Id, string Word, string Difficulty, string? Category, int Length, double SuccessRate);
public record AdminWordListRequest(string? Search, string? Difficulty, string? Category, int? MinLength, int? MaxLength, int Page = 1, int PageSize = 25);
public record AdminWordCreateRequest(string Word, string Difficulty, string? Category);
public record AdminWordUpdateRequest(string Word, string Difficulty, string? Category);
public record BulkImportResult(int Imported, int Skipped, int Errors, List<string> ErrorDetails);
public record WordStatsDto(Dictionary<string, int> DifficultyDistribution, Dictionary<string, double> SuccessRates, int TotalWords);
public record AdminUserDto(Guid Id, string Username, string Email, int Level, int XP, int StreakDays, bool IsSuspended, bool IsPremium, DateTime RegisteredAt, DateTime? LastLoginAt);
public record AdminUserListRequest(string? Search, bool? IsSuspended, bool? IsPremium, int? MinLevel, int? MaxLevel, int Page = 1, int PageSize = 25);
public record PaginatedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);
public record AdminDashboardStatsDto(int TotalUsers, int ActiveToday, int TotalWords, int DailyChallenges);
