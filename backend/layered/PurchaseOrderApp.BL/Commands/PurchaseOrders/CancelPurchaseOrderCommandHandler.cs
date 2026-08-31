using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Commands.PurchaseOrders;

public sealed class CancelPurchaseOrderCommandHandler(
    PurchaseOrderMutationCoordinator purchaseOrderMutationCoordinator,
    PurchaseOrderPolicy purchaseOrderPolicy)
{
    public Task<Result<PurchaseOrderResponse>> ExecuteAsync(CancelPurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.PurchaseOrderId, "Purchase order id is required."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<PurchaseOrderResponse>(validation.Error!, validation.Status));

        return purchaseOrderMutationCoordinator.ExecuteAsync(
            command.PurchaseOrderId,
            command.User,
            command.OccurredAt,
            (purchaseOrder, _) => {
                var rule = purchaseOrderPolicy.CanCancel(purchaseOrder);
                if (!rule.IsSuccess) return Task.FromResult(rule);

                purchaseOrder.Status = PurchaseOrderStatus.Cancelled;
                return Task.FromResult(Result.Success());
            },
            cancellationToken);
    }
}
