using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Application.UseCases;

public interface IFinanceService
{
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
