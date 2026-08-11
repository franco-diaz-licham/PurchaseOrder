using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Loads and persists inventory item aggregate roots.
/// </summary>
public interface IInventoryItemRepository
{
    /// <summary>
    /// Lists all inventory items.
    /// </summary>
    Task<List<InventoryItem>> ListAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets an inventory item by id.
    /// </summary>
    Task<InventoryItem?> GetAsync(InventoryItemId inventoryItemId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds an inventory item to the current unit of work.
    /// </summary>
    Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken);
}
