using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.DAL;

public sealed class PurchaseOrderDbContext(DbContextOptions<PurchaseOrderDbContext> options) : DbContext(options)
{
    public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseStock> WarehouseStock => Set<WarehouseStock>();

    public override int SaveChanges()
    {
        EnsureAuditEntriesAreImmutable();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuditEntriesAreImmutable();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasSequence<long>("purchase_order_number_seq")
            .StartsAt(1021)
            .HasMax(99999);

        ConfigurePurchaseOrders(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureReservations(modelBuilder);
        ConfigureAudit(modelBuilder);
    }

    private static void ConfigurePurchaseOrders(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PurchaseOrder>(entity => {
            entity.ToTable("purchase_orders");
            entity.HasKey(order => order.Id);
            entity.Property(order => order.Id).ValueGeneratedNever();
            entity.Property(order => order.PurchaseOrderNumber)
                .HasDefaultValueSql("'PO-' || nextval('purchase_order_number_seq')::text")
                .HasMaxLength(50)
                .IsRequired()
                .ValueGeneratedOnAdd();
            entity.Property(order => order.PurchaseOrderNumber).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(order => order.PurchaseOrderNumber).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
            entity.Property(order => order.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(order => order.CreatedAt).IsRequired();
            entity.Property(order => order.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(order => order.UpdatedAt).IsRequired(false);
            entity.Property(order => order.UpdatedBy).HasMaxLength(100);
            entity.HasIndex(order => order.PurchaseOrderNumber).IsUnique();
            entity.HasOne(order => order.Warehouse).WithMany().HasForeignKey(order => order.WarehouseId);
            entity.HasMany(order => order.Lines).WithOne().HasForeignKey(line => line.PurchaseOrderId);
        });

        modelBuilder.Entity<PurchaseOrderLine>(entity => {
            entity.ToTable("purchase_order_lines");
            entity.HasKey(line => line.Id);
            entity.Property(line => line.Id).ValueGeneratedNever();
            entity.Property(line => line.QuantityOrdered).HasPrecision(18, 3).IsRequired();
            entity.Property(line => line.QuantityReserved).HasPrecision(18, 3).IsRequired();
            entity.Property(line => line.CreatedAt).IsRequired();
            entity.Property(line => line.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(line => line.UpdatedAt).IsRequired(false);
            entity.Property(line => line.UpdatedBy).HasMaxLength(100);
            entity.HasOne(line => line.InventoryItem).WithMany().HasForeignKey(line => line.InventoryItemId);
            entity.Ignore(line => line.QuantityRemaining);
            entity.Ignore(line => line.HasOutstandingQuantity);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity => {
            entity.ToTable("inventory_items");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Id).ValueGeneratedNever();
            entity.Property(item => item.Sku).HasMaxLength(50).IsRequired();
            entity.Property(item => item.Name).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Category).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(item => item.TrackingMode).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(item => item.StandardCost).HasPrecision(18, 4).IsRequired();
            entity.Property(item => item.CreatedAt).IsRequired();
            entity.Property(item => item.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(item => item.UpdatedAt).IsRequired(false);
            entity.Property(item => item.UpdatedBy).HasMaxLength(100);
            entity.HasIndex(item => item.Sku).IsUnique();
        });

        modelBuilder.Entity<Warehouse>(entity => {
            entity.ToTable("warehouses");
            entity.HasKey(warehouse => warehouse.Id);
            entity.Property(warehouse => warehouse.Id).ValueGeneratedNever();
            entity.Property(warehouse => warehouse.Code).HasMaxLength(20).IsRequired();
            entity.Property(warehouse => warehouse.Name).HasMaxLength(100).IsRequired();
            entity.Property(warehouse => warehouse.CreatedAt).IsRequired();
            entity.Property(warehouse => warehouse.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(warehouse => warehouse.UpdatedAt).IsRequired(false);
            entity.Property(warehouse => warehouse.UpdatedBy).HasMaxLength(100);
            entity.HasIndex(warehouse => warehouse.Code).IsUnique();
        });

        modelBuilder.Entity<WarehouseStock>(entity => {
            entity.ToTable("warehouse_stock");
            entity.HasKey(stock => stock.Id);
            entity.Property(stock => stock.Id).ValueGeneratedNever();
            entity.Property(stock => stock.OnHandQuantity).HasPrecision(18, 3).IsRequired();
            entity.Property(stock => stock.CreatedAt).IsRequired();
            entity.Property(stock => stock.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(stock => stock.UpdatedAt).IsRequired(false);
            entity.Property(stock => stock.UpdatedBy).HasMaxLength(100);
            entity.HasOne<Warehouse>().WithMany().HasForeignKey(stock => stock.WarehouseId);
            entity.HasOne<InventoryItem>().WithMany().HasForeignKey(stock => stock.InventoryItemId);
            entity.HasIndex(stock => new { stock.WarehouseId, stock.InventoryItemId }).IsUnique();
        });
    }

    private static void ConfigureReservations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockReservation>(entity => {
            entity.ToTable("stock_reservations");
            entity.HasKey(reservation => reservation.Id);
            entity.Property(reservation => reservation.Id).ValueGeneratedNever();
            entity.Property(reservation => reservation.QuantityReserved).HasPrecision(18, 3).IsRequired();
            entity.Property(reservation => reservation.UnitCostSnapshot).HasPrecision(18, 4).IsRequired();
            entity.Property(reservation => reservation.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(reservation => reservation.CreatedAt).IsRequired();
            entity.Property(reservation => reservation.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(reservation => reservation.UpdatedAt).IsRequired(false);
            entity.Property(reservation => reservation.UpdatedBy).HasMaxLength(100);
            entity.HasOne<PurchaseOrderLine>().WithMany().HasForeignKey(reservation => reservation.PurchaseOrderLineId);
            entity.HasOne<Warehouse>().WithMany().HasForeignKey(reservation => reservation.WarehouseId);
            entity.HasOne<InventoryItem>().WithMany().HasForeignKey(reservation => reservation.InventoryItemId);
            entity.HasIndex(reservation => new { reservation.WarehouseId, reservation.InventoryItemId, reservation.Status });
            entity.Ignore(reservation => reservation.CommittedValue);
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLogEntry>(entity => {
            entity.ToTable("audit_log_entries");
            entity.HasKey(entry => entry.Id);
            entity.Property(entry => entry.Id).ValueGeneratedNever();
            entity.Property(entry => entry.Action).HasConversion<string>().HasMaxLength(40).IsRequired();
            entity.Property(entry => entry.Quantity).HasPrecision(18, 3).IsRequired();
            entity.Property(entry => entry.ResultingAvailableQuantity).HasPrecision(18, 3).IsRequired();
            entity.Property(entry => entry.CreatedAt).IsRequired();
            entity.Property(entry => entry.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(entry => entry.UpdatedAt).IsRequired(false);
            entity.Property(entry => entry.UpdatedBy).HasMaxLength(100);
        });
    }

    private void EnsureAuditEntriesAreImmutable()
    {
        var changedAuditEntry = ChangeTracker
            .Entries<AuditLogEntry>()
            .FirstOrDefault(IsAuditEntryMutation);

        if (changedAuditEntry is not null) throw new InvalidOperationException("Audit log entries are permanent and cannot be edited or deleted.");
    }

    private static bool IsAuditEntryMutation(EntityEntry<AuditLogEntry> entry)
    {
        return entry.State is EntityState.Modified or EntityState.Deleted;
    }
}
