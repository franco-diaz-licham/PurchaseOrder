using PurchaseOrder.Application.Models;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Loads, persists, and reads purchase order data needed by application use cases.
/// </summary>
public interface IPurchaseOrderRepository
{
    /// <summary>
    /// Gets the purchase order that owns the requested line.
    /// </summary>
    Task<PurchaseOrderHeader?> GetByLineIdAsync(PurchaseOrderLineId purchaseOrderLineId, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a purchase order to the current unit of work.
    /// </summary>
    Task AddAsync(PurchaseOrderHeader purchaseOrder, CancellationToken cancellationToken);

    /// <summary>
    /// Lists approved purchase order lines that still have quantity left to reserve for a warehouse.
    /// </summary>
    Task<List<ApprovedPurchaseOrderLineResponse>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken);
}
