using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class StreakProtectionConfiguration : IEntityTypeConfiguration<StreakProtection>
{
    public void Configure(EntityTypeBuilder<StreakProtection> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.UserId)
            .IsRequired();

        builder.Property(sp => sp.ShieldsRemaining)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sp => sp.FreezeUsedThisWeek)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sp => sp.IsShieldActive)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sp => sp.LastShieldActivatedAt);

        builder.HasIndex(sp => sp.UserId)
            .IsUnique();
    }
}
