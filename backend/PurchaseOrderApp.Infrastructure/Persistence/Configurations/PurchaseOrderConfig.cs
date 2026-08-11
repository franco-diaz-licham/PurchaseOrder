using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfig : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .HasConversion(id => id.Value, value => new PurchaseOrderId(value))
            .ValueGeneratedNever();

        builder.Property(order => order.PurchaseOrderNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(order => order.WarehouseId)
            .HasConversion(id => id.Value, value => new WarehouseId(value))
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.HasMany(order => order.Lines)
            .WithOne()
            .HasForeignKey(line => line.PurchaseOrderId);

        builder.HasOne(order => order.Warehouse)
            .WithMany()
            .HasForeignKey(order => order.WarehouseId);

        builder.Navigation(order => order.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(order => order.CreatedAt).IsRequired();
        builder.Property(order => order.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(order => order.UpdatedAt).IsRequired(false);
        builder.Property(order => order.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(order => order.PurchaseOrderNumber).IsUnique();
        builder.Ignore(order => order.HasOutstandingLines);
        builder.Ignore(order => order.DomainEvents);
    }
}
