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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LexiQuestDbContext).Assembly);
    }
}
