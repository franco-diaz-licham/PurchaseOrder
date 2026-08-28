using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Application.UseCases;

/// <summary>
/// Coordinates finance reporting reads.
/// </summary>
public interface IFinanceService
{
    /// <summary>
    /// Lists committed reserved stock value grouped by warehouse.
    /// </summary>
    Task<Result<List<WarehouseCommittedStockValueResponse>>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken);
}

public sealed class FinanceService(IFinanceQueryRepository financeRepository) : IFinanceService
{
    public async Task<Result<List<WarehouseCommittedStockValueResponse>>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        var values = await financeRepository.ListWarehouseCommittedStockValuesAsync(cancellationToken);
        return Result.Success(values);
    }
}
