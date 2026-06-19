namespace LexiQuest.Blazor.Services;

public interface IStatsService
{
    Task<UserStatsDto> GetUserStatsAsync();
}

public record UserStatsDto
{
    public int TotalXP { get; init; }
    public int CurrentLevel { get; init; }
    public int CurrentStreak { get; init; }
    public int LongestStreak { get; init; }
    public double Accuracy { get; init; }
    public string AverageTime { get; init; } = string.Empty;
    public int TotalWordsSolved { get; init; }
}
