using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class CustomDictionaryConfiguration : IEntityTypeConfiguration<CustomDictionary>
{
    public void Configure(EntityTypeBuilder<CustomDictionary> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.IsPublic)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(d => d.WordCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt);

        builder.HasIndex(d => d.UserId);
        builder.HasIndex(d => d.IsPublic);
    }
}
