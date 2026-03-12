using LexiQuest.Core.Domain.Entities;
using LexiQuest.Core.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.Plan)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.StripeSubscriptionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.StartedAt)
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .IsRequired();

        builder.Property(s => s.CancelledAt)
            .IsRequired(false);

        builder.HasIndex(s => s.UserId)
            .IsUnique();

        builder.HasIndex(s => s.StripeSubscriptionId)
            .IsUnique();

        builder.HasIndex(s => s.Status);

        builder.HasIndex(s => s.ExpiresAt);
    }
}
