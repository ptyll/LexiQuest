using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("Achievements");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Key)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.XPReward)
            .IsRequired();

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(a => a.RequiredValue)
            .IsRequired();

        builder.Property(a => a.IconName)
            .HasMaxLength(50);

        builder.HasIndex(a => a.Key).IsUnique();
        builder.HasIndex(a => a.Category);
    }
}

public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
{
    public void Configure(EntityTypeBuilder<UserAchievement> builder)
    {
        builder.ToTable("UserAchievements");
        builder.HasKey(ua => ua.Id);

        builder.Property(ua => ua.UserId)
            .IsRequired();

        builder.Property(ua => ua.AchievementId)
            .IsRequired();

        builder.Property(ua => ua.Progress)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(ua => ua.IsUnlocked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ua => ua.UnlockedAt);

        builder.HasIndex(ua => new { ua.UserId, ua.AchievementId }).IsUnique();
        builder.HasIndex(ua => ua.UserId);
    }
}
