using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Mappers;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Commands.PurchaseOrders;

public sealed class SubmitPurchaseOrderCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IWarehouseRepository warehouseRepository,
    IInventoryItemRepository inventoryItemRepository,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    TransactionCoordinator transactionCoordinator)
{
    public Task<Result<PurchaseOrderResponse>> ExecuteAsync(
        SubmitPurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        var validation = Validate(command);
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<PurchaseOrderResponse>(validation.Error!, validation.Status));

        return transactionCoordinator.ExecuteAsync(async ct => {
            var warehouse = await warehouseRepository.GetAsync(command.WarehouseId, ct);
            if (warehouse is null) return Result.Fail<PurchaseOrderResponse>("Warehouse was not found.", ResultStatus.NotFound);

            var purchaseOrder = new PurchaseOrder {
                Id = Guid.NewGuid(),
                WarehouseId = command.WarehouseId,
                Status = PurchaseOrderStatus.Pending,
                CreatedBy = command.User.Trim(),
                CreatedAt = command.OccurredAt
            };

            foreach (var lineCommand in command.Lines) {
                var line = await CreateLineAsync(purchaseOrder, lineCommand, command, ct);
                if (!line.IsSuccess) return Result.Fail<PurchaseOrderResponse>(line.Error!, line.Status);

                purchaseOrder.Lines.Add(line.Value!);
            }

            await purchaseOrderRepository.AddAsync(purchaseOrder, ct);
            return Result.Created(ResponseMapper.ToPurchaseOrderResponse(purchaseOrder));
        }, cancellationToken);
    }

    private async Task<Result<PurchaseOrderLine>> CreateLineAsync(
        PurchaseOrder purchaseOrder,
        SubmitPurchaseOrderLineCommand lineCommand,
        SubmitPurchaseOrderCommand command,
        CancellationToken cancellationToken)
    {
        var item = await inventoryItemRepository.GetAsync(lineCommand.InventoryItemId, cancellationToken);
        if (item is null) return Result.Fail<PurchaseOrderLine>("Inventory item was not found.", ResultStatus.NotFound);

        if (purchaseOrder.Lines.Any(line => line.InventoryItemId == item.Id)) {
            return Result.Fail<PurchaseOrderLine>("Inventory item has already been added to this purchase order.");
        }

        var quantityRule = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, lineCommand.QuantityOrdered, "Purchase order line quantity");
        return !quantityRule.IsSuccess
            ? Result.Fail<PurchaseOrderLine>(quantityRule.Error!, quantityRule.Status)
            : Result.Success(PurchaseOrderLineFactory.Create(purchaseOrder.Id, item, lineCommand.QuantityOrdered, command.User, command.OccurredAt));
    }

    private static Result Validate(SubmitPurchaseOrderCommand command)
    {
        var headerValidation = CommandValidation.All(
            CommandValidation.Required(command.WarehouseId, "Warehouse id is required."),
            CommandValidation.User(command.User));
        if (!headerValidation.IsSuccess) return headerValidation;

        foreach (var line in command.Lines) {
            var lineValidation = CommandValidation.All(
                CommandValidation.Required(line.InventoryItemId, "Inventory item id is required."),
                CommandValidation.Quantity(line.QuantityOrdered, "Line quantity must be greater than zero."));
            if (!lineValidation.IsSuccess) return lineValidation;
        }

        return Result.Success();
    }
}
