using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.PurchaseOrders;

public sealed record ListPurchaseOrderSummariesQuery;

public sealed class ListPurchaseOrderSummariesQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<Result<List<PurchaseOrderSummaryResponse>>> ExecuteAsync(ListPurchaseOrderSummariesQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await purchaseOrderRepository.ListSummariesAsync(cancellationToken));
    }
}
