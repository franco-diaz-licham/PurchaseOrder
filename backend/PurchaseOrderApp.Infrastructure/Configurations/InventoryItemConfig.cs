using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Configurations;

public sealed class InventoryItemConfig : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("inventory_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasConversion(id => id.Value, value => new InventoryItemId(value))
            .ValueGeneratedNever();

        builder.Property(item => item.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(item => item.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.Category)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(item => item.TrackingMode)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(item => item.StandardCost)
            .HasConversion(cost => cost.Value, value => new Money(value))
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(item => item.CreatedAt).IsRequired();
        builder.Property(item => item.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(item => item.UpdatedAt).IsRequired(false);
        builder.Property(item => item.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(item => item.Sku).IsUnique();
        builder.Ignore(item => item.DomainEvents);
    }
}
