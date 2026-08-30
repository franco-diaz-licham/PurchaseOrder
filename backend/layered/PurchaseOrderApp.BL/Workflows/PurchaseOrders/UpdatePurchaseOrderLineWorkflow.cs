using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.PurchaseOrders;

public sealed class UpdatePurchaseOrderLineWorkflow(
    PurchaseOrderMutationRunner purchaseOrderMutationRunner,
    PurchaseOrderPolicy purchaseOrderPolicy,
    PurchaseOrderLinePolicy purchaseOrderLinePolicy,
    InventoryQuantityPolicy inventoryQuantityPolicy)
{
    public Task<Result<PurchaseOrderResponse>> ExecuteAsync(
        UpdatePurchaseOrderLineCommand command,
        CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.PurchaseOrderId, "Purchase order id is required."),
            CommandValidation.Required(command.PurchaseOrderLineId, "Purchase order line id is required."),
            CommandValidation.Quantity(command.QuantityOrdered, "Line quantity must be greater than zero."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<PurchaseOrderResponse>(validation.Error!, validation.Status));

        return purchaseOrderMutationRunner.RunAsync(
            command.PurchaseOrderId,
            command.User,
            command.OccurredAt,
            (purchaseOrder, _) => {
                var canChange = purchaseOrderPolicy.CanChangeLines(purchaseOrder);
                if (!canChange.IsSuccess) return Task.FromResult(canChange);

                var line = purchaseOrder.Lines.SingleOrDefault(line => line.Id == command.PurchaseOrderLineId);
                if (line is null) return Task.FromResult(Result.Fail("Purchase order line was not found.", ResultStatus.NotFound));
                if (line.InventoryItem is null) return Task.FromResult(Result.Fail("Inventory item was not found.", ResultStatus.NotFound));

                var updateRule = purchaseOrderLinePolicy.CanUpdateQuantity(line, command.QuantityOrdered);
                if (!updateRule.IsSuccess) return Task.FromResult(updateRule);

                var quantityRule = inventoryQuantityPolicy.CanUseQuantity(line.InventoryItem.TrackingMode, command.QuantityOrdered, "Purchase order line quantity");
                if (!quantityRule.IsSuccess) return Task.FromResult(quantityRule);

                line.QuantityOrdered = command.QuantityOrdered;
                line.UpdatedBy = command.User.Trim();
                line.UpdatedAt = command.OccurredAt;

                return Task.FromResult(Result.Success());
            },
            cancellationToken);
    }
}
