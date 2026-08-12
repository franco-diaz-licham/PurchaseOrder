using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PurchaseOrderApp.Domain.Entities;

namespace PurchaseOrderApp.Infrastructure;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
        base.OnModelCreating(modelBuilder);
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
