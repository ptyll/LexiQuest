using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LexiQuest.Infrastructure.Persistence;

public class LexiQuestDbContext : DbContext
{
    public LexiQuestDbContext(DbContextOptions<LexiQuestDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Word> Words => Set<Word>();
    public DbSet<GameSession> GameSessions => Set<GameSession>();
    public DbSet<GameRound> GameRounds => Set<GameRound>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<PathLevel> PathLevels => Set<PathLevel>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<League> Leagues => Set<League>();
    public DbSet<LeagueParticipant> LeagueParticipants => Set<LeagueParticipant>();
    public DbSet<DailyChallenge> DailyChallenges => Set<DailyChallenge>();
    public DbSet<DailyChallengeCompletion> DailyChallengeCompletions => Set<DailyChallengeCompletion>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<StreakProtection> StreakProtections => Set<StreakProtection>();
    public DbSet<ShopItem> ShopItems => Set<ShopItem>();
    public DbSet<UserInventoryItem> UserInventoryItems => Set<UserInventoryItem>();
    public DbSet<CustomDictionary> CustomDictionaries => Set<CustomDictionary>();
    public DbSet<DictionaryWord> DictionaryWords => Set<DictionaryWord>();
    public DbSet<MatchResult> MatchResults => Set<MatchResult>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamInvite> TeamInvites => Set<TeamInvite>();
    public DbSet<TeamJoinRequest> TeamJoinRequests => Set<TeamJoinRequest>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();
    public DbSet<AdminRoleAssignment> AdminRoleAssignments => Set<AdminRoleAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LexiQuestDbContext).Assembly);
    }
}
