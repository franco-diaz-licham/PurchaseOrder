using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Loads and persists inventory item aggregate roots.
/// </summary>
public interface IInventoryItemRepository
{
    /// <summary>
    /// Gets an inventory item by id.
    /// </summary>
    Task<InventoryItem?> GetAsync(InventoryItemId inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an inventory item to the current unit of work.
    /// </summary>
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken);
}
