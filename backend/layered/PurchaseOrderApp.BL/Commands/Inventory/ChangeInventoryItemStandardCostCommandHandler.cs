using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;

namespace PurchaseOrderApp.BL.Commands.Inventory;

public sealed class ChangeInventoryItemStandardCostCommandHandler(
    IInventoryItemRepository inventoryItemRepository,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    TransactionCoordinator transactionCoordinator)
{
    public Task<Result> ExecuteAsync(ChangeInventoryItemStandardCostCommand command, CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.InventoryItemId, "Inventory item id is required."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(validation);

        var costRule = inventoryQuantityPolicy.CanUseCost(command.StandardCost);
        if (!costRule.IsSuccess) return Task.FromResult(costRule);

        return transactionCoordinator.ExecuteAsync(async ct => {
            var item = await inventoryItemRepository.GetAsync(command.InventoryItemId, ct);
            if (item is null) return Result.Fail("Inventory item was not found.", ResultStatus.NotFound);

            item.StandardCost = command.StandardCost;
            item.UpdatedBy = command.User.Trim();
            item.UpdatedAt = command.OccurredAt;

            return Result.Success();
        }, cancellationToken);
    }
}
