using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Loads inventory item read models and inventory item aggregate roots.
/// </summary>
public interface IInventoryItemRepository
{
    /// <summary>
    /// Lists all inventory items as read models.
    /// </summary>
    Task<List<InventoryItemResponse>> ListResponsesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets an inventory item by id.
    /// </summary>
    Task<InventoryItem?> GetAsync(InventoryItemId inventoryItemId, CancellationToken cancellationToken);

}
