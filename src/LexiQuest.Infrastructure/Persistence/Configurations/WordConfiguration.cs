using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class WordConfiguration : IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder)
    {
        builder.ToTable("Words");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Original)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Normalized)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Difficulty)
            .IsRequired();

        builder.Property(w => w.Category)
            .IsRequired();

        builder.HasIndex(w => w.Difficulty);
        builder.HasIndex(w => w.Category);
        builder.HasIndex(w => new { w.Difficulty, w.Category });
    }
}
