using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.PurchaseOrders;

public sealed record GetPurchaseOrderQuery(Guid PurchaseOrderId);

public sealed class GetPurchaseOrderQueryHandler(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<Result<PurchaseOrderResponse>> ExecuteAsync(GetPurchaseOrderQuery query, CancellationToken cancellationToken)
    {
        if (query.PurchaseOrderId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.");

        var purchaseOrder = await purchaseOrderRepository.GetResponseAsync(query.PurchaseOrderId, cancellationToken);
        return purchaseOrder is null
            ? Result.Fail<PurchaseOrderResponse>("Purchase order was not found.", ResultStatus.NotFound)
            : Result.Success(purchaseOrder);
    }
}
