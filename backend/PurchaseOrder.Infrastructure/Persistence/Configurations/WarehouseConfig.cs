using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfig : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(warehouse => warehouse.Id);

        builder.Property(warehouse => warehouse.Id)
            .HasConversion(id => id.Value, value => new WarehouseId(value))
            .ValueGeneratedNever();

        builder.Property(warehouse => warehouse.Code)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(warehouse => warehouse.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(warehouse => warehouse.CreatedAt).IsRequired();
        builder.Property(warehouse => warehouse.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(warehouse => warehouse.UpdatedAt).IsRequired(false);
        builder.Property(warehouse => warehouse.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(warehouse => warehouse.Code).IsUnique();
        builder.Ignore(warehouse => warehouse.DomainEvents);
    }
}
