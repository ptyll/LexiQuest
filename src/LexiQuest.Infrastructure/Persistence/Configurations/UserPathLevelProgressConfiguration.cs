using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class UserPathLevelProgressConfiguration : IEntityTypeConfiguration<UserPathLevelProgress>
{
    public void Configure(EntityTypeBuilder<UserPathLevelProgress> builder)
    {
        builder.ToTable("UserPathLevelProgresses");
        builder.HasKey(progress => progress.Id);

        builder.Property(progress => progress.UserId).IsRequired();
        builder.Property(progress => progress.PathId).IsRequired();
        builder.Property(progress => progress.PathLevelId).IsRequired();
        builder.Property(progress => progress.LevelNumber).IsRequired();
        builder.Property(progress => progress.Status).IsRequired();
        builder.Property(progress => progress.IsPerfect).IsRequired();
        builder.Property(progress => progress.CompletedAt).IsRequired();

        builder.HasIndex(progress => progress.UserId);
        builder.HasIndex(progress => progress.PathId);
        builder.HasIndex(progress => progress.PathLevelId);
        builder.HasIndex(progress => new { progress.UserId, progress.PathId, progress.LevelNumber })
            .IsUnique();
    }
}
