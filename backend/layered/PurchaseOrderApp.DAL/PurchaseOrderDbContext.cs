using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.DAL;

public sealed class PurchaseOrderDbContext(DbContextOptions<PurchaseOrderDbContext> options) : DbContext(options)
{
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseOrder>(entity => {
            entity.HasKey(purchaseOrder => purchaseOrder.Id);
            entity.Property(purchaseOrder => purchaseOrder.PurchaseOrderNumber).HasMaxLength(32);
            entity.Property(purchaseOrder => purchaseOrder.CreatedBy).HasMaxLength(128);
            entity.Property(purchaseOrder => purchaseOrder.UpdatedBy).HasMaxLength(128);

            entity.HasMany(purchaseOrder => purchaseOrder.Lines)
                .WithOne()
                .HasForeignKey(line => line.PurchaseOrderId);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity => {
            entity.HasKey(line => line.Id);
            entity.Property(line => line.QuantityOrdered).HasPrecision(18, 3);
            entity.Property(line => line.QuantityReserved).HasPrecision(18, 3);
        });
    }
}
