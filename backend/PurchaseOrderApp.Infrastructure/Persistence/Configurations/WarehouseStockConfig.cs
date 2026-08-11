using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Configurations;

public sealed class WarehouseStockConfig : IEntityTypeConfiguration<WarehouseStock>
{
    public void Configure(EntityTypeBuilder<WarehouseStock> builder)
    {
        builder.ToTable("warehouse_stock");

        builder.HasKey(stock => stock.Id);

        builder.Property(stock => stock.Id)
            .HasConversion(id => id.Value, value => new WarehouseStockId(value))
            .ValueGeneratedNever();

        builder.Property(stock => stock.WarehouseId)
            .HasConversion(id => id.Value, value => new WarehouseId(value))
            .IsRequired();

        builder.Property(stock => stock.InventoryItemId)
            .HasConversion(id => id.Value, value => new InventoryItemId(value))
            .IsRequired();

        builder.Property(stock => stock.OnHandQuantity)
            .HasConversion(quantity => quantity.Value, value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(stock => stock.CreatedAt).IsRequired();
        builder.Property(stock => stock.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(stock => stock.UpdatedAt).IsRequired(false);
        builder.Property(stock => stock.UpdatedBy).HasMaxLength(100);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(stock => stock.WarehouseId);

        builder.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(stock => stock.InventoryItemId);

        builder.HasIndex(stock => new { stock.WarehouseId, stock.InventoryItemId }).IsUnique();
        builder.Ignore(stock => stock.DomainEvents);
    }
}
