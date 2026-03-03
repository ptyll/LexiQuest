namespace LexiQuest.Infrastructure.Caching;

public static class CacheKeys
{
    public const string WordsByDifficulty = "words:difficulty:{0}";
    public const string WordsByCategory = "words:category:{0}";
    public const string UserStats = "user:stats:{0}";
    public const string LeagueLeaderboard = "league:leaderboard:{0}";
    public const string DailyChallenge = "daily:challenge:{0}";
    public const string UserAchievements = "user:achievements:{0}";

    public static string ForWordsByDifficulty(string difficulty) => string.Format(WordsByDifficulty, difficulty);
    public static string ForWordsByCategory(string category) => string.Format(WordsByCategory, category);
    public static string ForUserStats(Guid userId) => string.Format(UserStats, userId);
    public static string ForLeagueLeaderboard(string leagueId) => string.Format(LeagueLeaderboard, leagueId);
    public static string ForDailyChallenge(string date) => string.Format(DailyChallenge, date);
    public static string ForUserAchievements(Guid userId) => string.Format(UserAchievements, userId);
}
