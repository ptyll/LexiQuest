using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Tier)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(l => l.WeekStart)
            .IsRequired();

        builder.Property(l => l.WeekEnd)
            .IsRequired();

        builder.Property(l => l.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        builder.HasMany(l => l.Participants)
            .WithOne()
            .HasForeignKey(p => p.LeagueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => new { l.Tier, l.IsActive });
        builder.HasIndex(l => l.WeekStart);
    }
}

public class LeagueParticipantConfiguration : IEntityTypeConfiguration<LeagueParticipant>
{
    public void Configure(EntityTypeBuilder<LeagueParticipant> builder)
    {
        builder.ToTable("LeagueParticipants");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.LeagueId)
            .IsRequired();

        builder.Property(p => p.WeeklyXP)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Rank)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.IsPromoted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.IsDemoted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.HasIndex(p => new { p.UserId, p.LeagueId });
        builder.HasIndex(p => p.UserId);
        builder.HasIndex(p => p.WeeklyXP);
    }
}
