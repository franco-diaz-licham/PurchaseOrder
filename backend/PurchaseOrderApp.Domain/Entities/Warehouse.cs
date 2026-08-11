using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Domain.Entities;

/// <summary>
/// Aggregate root for a warehouse location used to scope purchase orders, stock, and finance reporting.
/// </summary>
public sealed class Warehouse : Entity<WarehouseId>
{
    private Warehouse() { }

    private Warehouse(WarehouseId id, string code, string name, string user, DateTimeOffset occurredAt)
    {
        Id = id;
        Code = Required(code, nameof(code)).ToUpperInvariant();
        Name = Required(name, nameof(name));
        SetCreated(user, occurredAt);
    }

    /// <summary>
    /// Short warehouse code used in operational displays and seed data.
    /// </summary>
    public string Code { get; private set; } = default!;

    /// <summary>
    /// Display name for the warehouse.
    /// </summary>
    public string Name { get; private set; } = default!;

    public static Warehouse Create(string code, string name, string user, DateTimeOffset occurredAt)
    {
        return new Warehouse(new WarehouseId(Guid.NewGuid()), code, name, user, occurredAt);
    }

    public void Rename(string name, string user, DateTimeOffset occurredAt)
    {
        Name = Required(name, nameof(name));
        SetUpdated(user, occurredAt);
    }
}
