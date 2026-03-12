using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class ShopItemConfiguration : IEntityTypeConfiguration<ShopItem>
{
    public void Configure(EntityTypeBuilder<ShopItem> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Category)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Price)
            .IsRequired();

        builder.Property(s => s.Rarity)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.IsPremiumOnly)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.IsLimited)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(s => s.AvailableUntil);

        builder.HasIndex(s => s.Category);
        builder.HasIndex(s => s.IsPremiumOnly);
        builder.HasIndex(s => s.IsLimited);
    }
}
