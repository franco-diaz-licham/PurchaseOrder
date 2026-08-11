using PurchaseOrder.Application.Models;

namespace PurchaseOrder.Application.Ports;

/// <summary>
/// Reads finance reporting projections.
/// </summary>
public interface IFinanceQueryRepository
{
    /// <summary>
    /// Lists committed stock value totals for each warehouse.
    /// </summary>
    Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken);
}
