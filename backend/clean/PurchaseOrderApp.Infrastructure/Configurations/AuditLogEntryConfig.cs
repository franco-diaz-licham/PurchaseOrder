using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Configurations;

public sealed class AuditLogEntryConfig : IEntityTypeConfiguration<AuditLogEntry>
{
    public void Configure(EntityTypeBuilder<AuditLogEntry> builder)
    {
        builder.ToTable("audit_log_entries");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasConversion(id => id.Value, value => new AuditLogEntryId(value))
            .ValueGeneratedNever();

        builder.Property(entry => entry.Action)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(entry => entry.InventoryItemId)
            .HasConversion(id => id.Value, value => new InventoryItemId(value))
            .IsRequired();

        builder.Property(entry => entry.WarehouseId)
            .HasConversion(id => id.Value, value => new WarehouseId(value))
            .IsRequired();

        builder.Property(entry => entry.PurchaseOrderLineId)
            .HasConversion(id => id.Value, value => new PurchaseOrderLineId(value))
            .IsRequired();

        builder.Property(entry => entry.StockReservationId)
            .HasConversion(id => id.Value, value => new StockReservationId(value))
            .IsRequired();

        builder.Property(entry => entry.Quantity)
            .HasConversion(quantity => quantity.Value, value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(entry => entry.ResultingAvailableQuantity)
            .HasConversion(quantity => quantity.Value, value => new Quantity(value))
            .HasPrecision(18, 3)
            .IsRequired();

        builder.Property(entry => entry.CreatedAt).IsRequired();
        builder.Property(entry => entry.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(entry => entry.UpdatedAt).IsRequired(false);
        builder.Property(entry => entry.UpdatedBy).HasMaxLength(100);

        builder.Ignore(entry => entry.DomainEvents);
    }
}
