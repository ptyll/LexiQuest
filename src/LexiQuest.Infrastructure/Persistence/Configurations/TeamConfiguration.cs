using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(t => t.Tag)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.LeaderId)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasMany(t => t.Members)
            .WithOne()
            .HasForeignKey(m => m.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.Name).IsUnique();
        builder.HasIndex(t => t.Tag).IsUnique();
        builder.HasIndex(t => t.LeaderId);
    }
}

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.UserId)
            .IsRequired();

        builder.Property(m => m.TeamId)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(m => m.JoinedAt)
            .IsRequired();

        builder.Property(m => m.WeeklyXP)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.AllTimeXP)
            .IsRequired()
            .HasDefaultValue(0L);

        builder.Property(m => m.Wins)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(m => new { m.UserId, m.TeamId }).IsUnique();
        builder.HasIndex(m => m.UserId);
    }
}

public class TeamInviteConfiguration : IEntityTypeConfiguration<TeamInvite>
{
    public void Configure(EntityTypeBuilder<TeamInvite> builder)
    {
        builder.ToTable("TeamInvites");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TeamId)
            .IsRequired();

        builder.Property(i => i.InvitedUserId)
            .IsRequired();

        builder.Property(i => i.InvitedByUserId)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.HasIndex(i => new { i.TeamId, i.InvitedUserId });
        builder.HasIndex(i => i.InvitedUserId);
        builder.HasIndex(i => i.Status);
    }
}

public class TeamJoinRequestConfiguration : IEntityTypeConfiguration<TeamJoinRequest>
{
    public void Configure(EntityTypeBuilder<TeamJoinRequest> builder)
    {
        builder.ToTable("TeamJoinRequests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TeamId)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Message)
            .HasMaxLength(500);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.HasIndex(r => new { r.TeamId, r.UserId });
        builder.HasIndex(r => r.Status);
    }
}
