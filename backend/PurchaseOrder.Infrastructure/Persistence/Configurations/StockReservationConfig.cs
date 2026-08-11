using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Configurations;

public sealed class StockReservationConfig : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.ToTable("stock_reservations");

        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.Id)
            .HasConversion(id => id.Value, value => new StockReservationId(value))
            .ValueGeneratedNever();

        builder.Property(reservation => reservation.PurchaseOrderLineId)
            .HasConversion(id => id.Value, value => new PurchaseOrderLineId(value))
            .IsRequired();

        builder.Property(reservation => reservation.WarehouseId)
            .HasConversion(id => id.Value, value => new WarehouseId(value))
            .IsRequired();

        builder.Property(reservation => reservation.InventoryItemId)
            .HasConversion(id => id.Value, value => new InventoryItemId(value))
            .IsRequired();

        builder.Property(reservation => reservation.QuantityReserved)
            .HasConversion(quantity => quantity.Value, value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(reservation => reservation.UnitCostSnapshot)
            .HasConversion(cost => cost.Value, value => new Money(value))
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(reservation => reservation.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(reservation => reservation.CreatedAt).IsRequired();
        builder.Property(reservation => reservation.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(reservation => reservation.UpdatedAt).IsRequired(false);
        builder.Property(reservation => reservation.UpdatedBy).HasMaxLength(100);

        builder.HasOne<PurchaseOrderLine>()
            .WithMany()
            .HasForeignKey(reservation => reservation.PurchaseOrderLineId);

        builder.HasOne<Warehouse>()
            .WithMany()
            .HasForeignKey(reservation => reservation.WarehouseId);

        builder.HasOne<InventoryItem>()
            .WithMany()
            .HasForeignKey(reservation => reservation.InventoryItemId);

        builder.HasIndex(reservation => new { reservation.WarehouseId, reservation.InventoryItemId, reservation.Status });
        builder.Ignore(reservation => reservation.CommittedValue);
        builder.Ignore(reservation => reservation.DomainEvents);
    }
}
