using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class MatchResultConfiguration : IEntityTypeConfiguration<MatchResult>
{
    public void Configure(EntityTypeBuilder<MatchResult> builder)
    {
        builder.ToTable("MatchResults");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MatchId).IsRequired();
        builder.Property(m => m.Player1Id).IsRequired();
        builder.Property(m => m.Player2Id).IsRequired();
        builder.Property(m => m.Player1Username).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Player2Username).IsRequired().HasMaxLength(50);
        builder.Property(m => m.Player1Avatar).HasMaxLength(500);
        builder.Property(m => m.Player2Avatar).HasMaxLength(500);

        builder.Property(m => m.Player1Score).IsRequired();
        builder.Property(m => m.Player2Score).IsRequired();
        builder.Property(m => m.Player1Time).IsRequired();
        builder.Property(m => m.Player2Time).IsRequired();
        builder.Property(m => m.Player1MaxCombo).IsRequired();
        builder.Property(m => m.Player2MaxCombo).IsRequired();

        builder.Property(m => m.WinnerId);
        builder.Property(m => m.IsDraw).IsRequired();

        builder.Property(m => m.Player1XPEarned).IsRequired();
        builder.Property(m => m.Player2XPEarned).IsRequired();
        builder.Property(m => m.Player1LeagueXPEarned).IsRequired();
        builder.Property(m => m.Player2LeagueXPEarned).IsRequired();

        builder.Property(m => m.IsPrivateRoom).IsRequired();
        builder.Property(m => m.RoomCode).HasMaxLength(20);
        builder.Property(m => m.SeriesPlayer1Wins);
        builder.Property(m => m.SeriesPlayer2Wins);

        builder.Property(m => m.WordCount).IsRequired();
        builder.Property(m => m.TimeLimitMinutes).IsRequired();
        builder.Property(m => m.Difficulty).IsRequired();

        builder.Property(m => m.StartedAt).IsRequired();
        builder.Property(m => m.CompletedAt).IsRequired();

        // Indexes
        builder.HasIndex(m => m.MatchId).IsUnique();
        builder.HasIndex(m => m.Player1Id);
        builder.HasIndex(m => m.Player2Id);
        builder.HasIndex(m => m.CompletedAt);
        builder.HasIndex(m => m.IsPrivateRoom);
    }
}
