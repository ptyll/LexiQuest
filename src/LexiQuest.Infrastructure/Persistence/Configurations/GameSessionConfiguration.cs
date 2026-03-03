using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class GameSessionConfiguration : IEntityTypeConfiguration<GameSession>
{
    public void Configure(EntityTypeBuilder<GameSession> builder)
    {
        builder.ToTable("GameSessions");
        builder.HasKey(gs => gs.Id);

        builder.Property(gs => gs.UserId).IsRequired();
        builder.Property(gs => gs.Mode).IsRequired();
        builder.Property(gs => gs.Status).IsRequired();
        builder.Property(gs => gs.Difficulty).IsRequired();

        builder.HasIndex(gs => gs.UserId);
        builder.HasIndex(gs => gs.Status);

        builder.HasMany(gs => gs.Rounds)
            .WithOne()
            .HasForeignKey(gr => gr.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
