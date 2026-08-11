using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.Ports;

/// <summary>
/// Loads, persists, and reads purchase order data needed by application use cases.
/// </summary>
public interface IPurchaseOrderRepository
{
    /// <summary>
    /// Gets a purchase order aggregate by id for mutation workflows.
    /// </summary>
    Task<PurchaseOrder?> GetAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the purchase order that owns the requested line.
    /// </summary>
    Task<PurchaseOrder?> GetByLineIdAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a purchase order read model by id.
    /// </summary>
    Task<PurchaseOrderResponse?> GetResponseAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken);

    /// <summary>
    /// Lists purchase order read models.
    /// </summary>
    Task<List<PurchaseOrderResponse>> ListResponsesAsync(WarehouseId? warehouseId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a purchase order to the current unit of work.
    /// </summary>
    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken);

    /// <summary>
    /// Lists approved purchase order lines that still have quantity left to reserve for a warehouse.
    /// </summary>
    Task<List<ApprovedPurchaseOrderLineResponse>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken);
}
