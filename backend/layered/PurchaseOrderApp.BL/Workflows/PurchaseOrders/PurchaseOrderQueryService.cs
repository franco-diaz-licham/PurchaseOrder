using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.PurchaseOrders;

public sealed class PurchaseOrderQueryService(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<Result<PurchaseOrderResponse>> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        if (purchaseOrderId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.");

        var purchaseOrder = await purchaseOrderRepository.GetResponseAsync(purchaseOrderId, cancellationToken);
        return purchaseOrder is null
            ? Result.Fail<PurchaseOrderResponse>("Purchase order was not found.", ResultStatus.NotFound)
            : Result.Success(purchaseOrder);
    }

    public async Task<Result<List<PurchaseOrderSummaryResponse>>> ListSummariesAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await purchaseOrderRepository.ListSummariesAsync(cancellationToken));
    }
}
