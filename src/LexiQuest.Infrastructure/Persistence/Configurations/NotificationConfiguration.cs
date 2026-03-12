using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.Severity)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.HasIndex(n => new { n.UserId, n.IsRead });
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.CreatedAt);
    }
}

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");
        builder.HasKey(np => np.Id);

        builder.Property(np => np.UserId)
            .IsRequired();

        builder.Property(np => np.PushEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.EmailEnabled)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.StreakReminder)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.StreakReminderTime)
            .IsRequired();

        builder.Property(np => np.LeagueUpdates)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.AchievementNotifications)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(np => np.DailyChallengeReminder)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(np => np.UserId).IsUnique();
    }
}

public class PushSubscriptionConfiguration : IEntityTypeConfiguration<PushSubscription>
{
    public void Configure(EntityTypeBuilder<PushSubscription> builder)
    {
        builder.ToTable("PushSubscriptions");
        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.UserId)
            .IsRequired();

        builder.Property(ps => ps.Endpoint)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(ps => ps.P256dh)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ps => ps.Auth)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(ps => ps.UserId);
        builder.HasIndex(ps => ps.Endpoint).IsUnique();
    }
}
