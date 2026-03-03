using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Username)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();

        builder.OwnsOne(u => u.Stats, stats =>
        {
            stats.Property(s => s.TotalXP).HasColumnName("TotalXP");
            stats.Property(s => s.Level).HasColumnName("Level");
            stats.Property(s => s.Accuracy).HasColumnName("Accuracy");
            stats.Property(s => s.TotalWordsSolved).HasColumnName("TotalWordsSolved");
            stats.Property(s => s.AverageResponseTime).HasColumnName("AverageResponseTime");
        });

        builder.OwnsOne(u => u.Preferences, prefs =>
        {
            prefs.Property(p => p.Theme).HasColumnName("Theme").HasMaxLength(20);
            prefs.Property(p => p.Language).HasColumnName("Language").HasMaxLength(10);
            prefs.Property(p => p.AnimationsEnabled).HasColumnName("AnimationsEnabled");
            prefs.Property(p => p.SoundsEnabled).HasColumnName("SoundsEnabled");
        });

        builder.OwnsOne(u => u.Streak, streak =>
        {
            streak.Property(s => s.CurrentDays).HasColumnName("StreakCurrentDays");
            streak.Property(s => s.LongestDays).HasColumnName("StreakLongestDays");
            streak.Property(s => s.LastActivityDate).HasColumnName("StreakLastActivityDate");
        });

        builder.OwnsOne(u => u.Premium, premium =>
        {
            premium.Property(p => p.IsPremium).HasColumnName("IsPremium");
            premium.Property(p => p.ExpiresAt).HasColumnName("PremiumExpiresAt");
            premium.Property(p => p.Plan).HasColumnName("PremiumPlan").HasMaxLength(50);
        });
    }
}
