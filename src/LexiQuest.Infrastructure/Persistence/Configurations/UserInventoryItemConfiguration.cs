using LexiQuest.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LexiQuest.Infrastructure.Persistence.Configurations;

public class UserInventoryItemConfiguration : IEntityTypeConfiguration<UserInventoryItem>
{
    public void Configure(EntityTypeBuilder<UserInventoryItem> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.UserId)
            .IsRequired();

        builder.Property(u => u.ShopItemId)
            .IsRequired();

        builder.Property(u => u.PurchasedAt)
            .IsRequired();

        builder.Property(u => u.IsEquipped)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(u => new { u.UserId, u.ShopItemId })
            .IsUnique();

        builder.HasIndex(u => u.UserId);
        builder.HasIndex(u => new { u.UserId, u.IsEquipped });
    }
}
