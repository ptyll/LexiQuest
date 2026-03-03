using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class GameRoundConfiguration : IEntityTypeConfiguration<GameRound>
{
    public void Configure(EntityTypeBuilder<GameRound> builder)
    {
        builder.ToTable("GameRounds");
        builder.HasKey(gr => gr.Id);

        builder.Property(gr => gr.Scrambled)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(gr => gr.UserAnswer)
            .HasMaxLength(100);

        builder.HasIndex(gr => gr.GameSessionId);
    }
}
