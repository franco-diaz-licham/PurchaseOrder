using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Loads warehouse read models and warehouse aggregate roots.
/// </summary>
public interface IWarehouseRepository
{
    /// <summary>
    /// Lists all warehouses as read models.
    /// </summary>
    Task<List<WarehouseResponse>> ListResponsesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a warehouse by id.
    /// </summary>
    Task<Warehouse?> GetAsync(WarehouseId warehouseId, CancellationToken cancellationToken);

}
