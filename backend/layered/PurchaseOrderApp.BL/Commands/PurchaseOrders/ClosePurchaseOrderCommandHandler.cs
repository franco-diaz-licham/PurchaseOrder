using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Commands.PurchaseOrders;

public sealed class ClosePurchaseOrderCommandHandler(PurchaseOrderMutationCoordinator purchaseOrderMutationCoordinator)
{
    public Task<Result<PurchaseOrderResponse>> ExecuteAsync(ClosePurchaseOrderCommand command, CancellationToken cancellationToken)
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
                purchaseOrder.Status = PurchaseOrderStatus.Closed;
                return Task.FromResult(Result.Success());
            },
            cancellationToken);
    }
}
