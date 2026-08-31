using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.Finance;

public sealed record ListWarehouseCommittedStockValuesQuery;

public sealed class ListWarehouseCommittedStockValuesQueryHandler(IFinanceQueryRepository financeQueryRepository)
{
    public async Task<Result<List<WarehouseCommittedStockValueResponse>>> ExecuteAsync(ListWarehouseCommittedStockValuesQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await financeQueryRepository.ListWarehouseCommittedStockValuesAsync(cancellationToken));
    }
}
