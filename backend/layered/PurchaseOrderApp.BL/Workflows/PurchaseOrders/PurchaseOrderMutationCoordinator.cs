using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Mappers;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.PurchaseOrders;

public sealed class PurchaseOrderMutationCoordinator(
    IPurchaseOrderRepository purchaseOrderRepository,
    TransactionCoordinator transactionCoordinator)
{
    public Task<Result<PurchaseOrderResponse>> RunAsync(
        Guid purchaseOrderId,
        string user,
        DateTimeOffset occurredAt,
        Func<PurchaseOrder, CancellationToken, Task<Result>> mutate,
        CancellationToken cancellationToken)
    {
        return transactionCoordinator.RunAsync(async ct => {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(purchaseOrderId, ct);
            if (purchaseOrder is null) return Result.Fail<PurchaseOrderResponse>("Purchase order was not found.", ResultStatus.NotFound);

            var mutation = await mutate(purchaseOrder, ct);
            if (!mutation.IsSuccess) return Result.Fail<PurchaseOrderResponse>(mutation.Error!, mutation.Status);

            purchaseOrder.UpdatedBy = user.Trim();
            purchaseOrder.UpdatedAt = occurredAt;

            return Result.Success(ResponseMapper.ToPurchaseOrderResponse(purchaseOrder));
        }, cancellationToken);
    }
}
