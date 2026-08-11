using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderLineConfig : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("purchase_order_lines");

        builder.HasKey(line => line.Id);

        builder.Property(line => line.Id)
            .HasConversion(id => id.Value, value => new PurchaseOrderLineId(value))
            .ValueGeneratedNever();

        builder.Property(line => line.PurchaseOrderId)
            .HasConversion(id => id.Value, value => new PurchaseOrderId(value))
            .IsRequired();

        builder.Property(line => line.InventoryItemId)
            .HasConversion(id => id.Value, value => new InventoryItemId(value))
            .IsRequired();

        builder.Property(line => line.QuantityOrdered)
            .HasConversion(quantity => quantity.Value, value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(line => line.QuantityReserved)
            .HasConversion(quantity => quantity.Value, value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(line => line.CreatedAt).IsRequired();
        builder.Property(line => line.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(line => line.UpdatedAt).IsRequired(false);
        builder.Property(line => line.UpdatedBy).HasMaxLength(100);

        builder.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(line => line.InventoryItemId);

        builder.Ignore(line => line.QuantityRemaining);
        builder.Ignore(line => line.HasOutstandingQuantity);
        builder.Ignore(line => line.IsFullyReserved);
        builder.Ignore(line => line.DomainEvents);
    }
}
