using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.PurchaseOrders;

public sealed class ChangePurchaseOrderStatusWorkflow(
    PurchaseOrderMutationCoordinator purchaseOrderMutationCoordinator,
    PurchaseOrderPolicy purchaseOrderPolicy)
{
    public Task<Result<PurchaseOrderResponse>> ApproveAsync(
        ChangePurchaseOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(command, PurchaseOrderStatus.Approved, purchaseOrderPolicy.CanApprove, cancellationToken);
    }

    public Task<Result<PurchaseOrderResponse>> CloseAsync(
        ChangePurchaseOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(command, PurchaseOrderStatus.Closed, _ => Result.Success(), cancellationToken);
    }

    public Task<Result<PurchaseOrderResponse>> CancelAsync(
        ChangePurchaseOrderStatusCommand command,
        CancellationToken cancellationToken)
    {
        return ChangeStatusAsync(command, PurchaseOrderStatus.Cancelled, purchaseOrderPolicy.CanCancel, cancellationToken);
    }

    private Task<Result<PurchaseOrderResponse>> ChangeStatusAsync(
        ChangePurchaseOrderStatusCommand command,
        PurchaseOrderStatus status,
        Func<PurchaseOrder, Result> rule,
        CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.PurchaseOrderId, "Purchase order id is required."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<PurchaseOrderResponse>(validation.Error!, validation.Status));

        return purchaseOrderMutationCoordinator.RunAsync(
            command.PurchaseOrderId,
            command.User,
            command.OccurredAt,
            (purchaseOrder, _) => {
                var ruleResult = rule(purchaseOrder);
                if (!ruleResult.IsSuccess) return Task.FromResult(ruleResult);

                purchaseOrder.Status = status;
                return Task.FromResult(Result.Success());
            },
            cancellationToken);
    }
}
