using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class AdminRoleAssignmentConfiguration : IEntityTypeConfiguration<AdminRoleAssignment>
{
    public void Configure(EntityTypeBuilder<AdminRoleAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(a => a.AssignedAt)
            .IsRequired();

        builder.HasIndex(a => new { a.UserId, a.Role })
            .IsUnique();

        builder.HasIndex(a => a.UserId);
    }
}
