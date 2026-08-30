using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.PurchaseOrders;

public sealed class AddPurchaseOrderLineWorkflow(
    PurchaseOrderMutationRunner purchaseOrderMutationRunner,
    IInventoryItemRepository inventoryItemRepository,
    PurchaseOrderPolicy purchaseOrderPolicy,
    InventoryQuantityPolicy inventoryQuantityPolicy)
{
    public Task<Result<PurchaseOrderResponse>> ExecuteAsync(
        AddPurchaseOrderLineCommand command,
        CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.PurchaseOrderId, "Purchase order id is required."),
            CommandValidation.Required(command.InventoryItemId, "Inventory item id is required."),
            CommandValidation.Quantity(command.QuantityOrdered, "Line quantity must be greater than zero."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<PurchaseOrderResponse>(validation.Error!, validation.Status));

        return purchaseOrderMutationRunner.RunAsync(
            command.PurchaseOrderId,
            command.User,
            command.OccurredAt,
            async (purchaseOrder, ct) => {
                var canChange = purchaseOrderPolicy.CanChangeLines(purchaseOrder);
                if (!canChange.IsSuccess) return canChange;

                if (purchaseOrder.Lines.Any(line => line.InventoryItemId == command.InventoryItemId)) {
                    return Result.Fail("Inventory item has already been added to this purchase order.");
                }

                var item = await inventoryItemRepository.GetAsync(command.InventoryItemId, ct);
                if (item is null) return Result.Fail("Inventory item was not found.", ResultStatus.NotFound);

                var quantityRule = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, command.QuantityOrdered, "Purchase order line quantity");
                if (!quantityRule.IsSuccess) return quantityRule;

                purchaseOrder.Lines.Add(PurchaseOrderLineFactory.Create(purchaseOrder.Id, item, command.QuantityOrdered, command.User, command.OccurredAt));
                return Result.Success();
            },
            cancellationToken);
    }
}
