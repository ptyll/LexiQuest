using LexiQuest.Core.Domain.Entities;
using LexiQuest.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class DictionaryWordConfiguration : IEntityTypeConfiguration<DictionaryWord>
{
    public void Configure(EntityTypeBuilder<DictionaryWord> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.DictionaryId)
            .IsRequired();

        builder.Property(w => w.Word)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Difficulty)
            .IsRequired()
            .HasConversion<string>();

        builder.HasIndex(w => w.DictionaryId);
        builder.HasIndex(w => new { w.DictionaryId, w.Word })
            .IsUnique();
    }
}
