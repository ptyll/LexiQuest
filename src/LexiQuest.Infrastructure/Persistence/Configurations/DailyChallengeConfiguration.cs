using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class DailyChallengeConfiguration : IEntityTypeConfiguration<DailyChallenge>
{
    public void Configure(EntityTypeBuilder<DailyChallenge> builder)
    {
        builder.ToTable("DailyChallenges");
        builder.HasKey(dc => dc.Id);

        builder.Property(dc => dc.Date)
            .IsRequired();

        builder.Property(dc => dc.WordId)
            .IsRequired();

        builder.Property(dc => dc.Modifier)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(dc => dc.CreatedAt)
            .IsRequired();

        builder.HasIndex(dc => dc.Date).IsUnique();
    }
}

public class DailyChallengeCompletionConfiguration : IEntityTypeConfiguration<DailyChallengeCompletion>
{
    public void Configure(EntityTypeBuilder<DailyChallengeCompletion> builder)
    {
        builder.ToTable("DailyChallengeCompletions");
        builder.HasKey(dcc => dcc.Id);

        builder.Property(dcc => dcc.UserId)
            .IsRequired();

        builder.Property(dcc => dcc.ChallengeDate)
            .IsRequired();

        builder.Property(dcc => dcc.TimeTaken)
            .IsRequired();

        builder.Property(dcc => dcc.XPEarned)
            .IsRequired();

        builder.Property(dcc => dcc.CompletedAt)
            .IsRequired();

        builder.HasIndex(dcc => new { dcc.UserId, dcc.ChallengeDate }).IsUnique();
        builder.HasIndex(dcc => dcc.ChallengeDate);
    }
}
