using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Mappers;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class PurchaseOrderWorkflowService(
    IPurchaseOrderRepository purchaseOrderRepository,
    IWarehouseRepository warehouseRepository,
    IInventoryItemRepository inventoryItemRepository,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IAuditLogRepository auditLogRepository,
    PurchaseOrderPolicy purchaseOrderPolicy,
    PurchaseOrderLinePolicy purchaseOrderLinePolicy,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    IUnitOfWork unitOfWork) : IPurchaseOrderWorkflowService
{
    public async Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.WarehouseId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Warehouse id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.");

        foreach (var line in command.Lines) {
            if (line.InventoryItemId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Inventory item id is required.");
            if (line.QuantityOrdered < 0) return Result.Fail<PurchaseOrderResponse>("Quantity cannot be negative.");
            if (decimal.Round(line.QuantityOrdered, 3) != line.QuantityOrdered) return Result.Fail<PurchaseOrderResponse>("Quantity cannot have more than 3 decimal places.");
            if (line.QuantityOrdered == 0) return Result.Fail<PurchaseOrderResponse>("Line quantity must be greater than zero.");
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try {
            var warehouse = await warehouseRepository.GetAsync(command.WarehouseId, cancellationToken);
            if (warehouse is null) return await Rollback<PurchaseOrderResponse>("Warehouse was not found.", ResultStatus.NotFound, cancellationToken);

            var purchaseOrder = new PurchaseOrder {
                Id = Guid.NewGuid(),
                WarehouseId = command.WarehouseId,
                Status = PurchaseOrderStatus.Pending,
                CreatedBy = command.User.Trim(),
                CreatedAt = command.OccurredAt
            };

            foreach (var lineCommand in command.Lines) {
                var item = await inventoryItemRepository.GetAsync(lineCommand.InventoryItemId, cancellationToken);
                if (item is null) return await Rollback<PurchaseOrderResponse>("Inventory item was not found.", ResultStatus.NotFound, cancellationToken);
                if (purchaseOrder.Lines.Any(line => line.InventoryItemId == item.Id)) return await Rollback<PurchaseOrderResponse>("Inventory item has already been added to this purchase order.", ResultStatus.Invalid, cancellationToken);

                var quantityResult = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, lineCommand.QuantityOrdered, "Purchase order line quantity");
                if (!quantityResult.IsSuccess) return await Rollback<PurchaseOrderResponse>(quantityResult.Error!, quantityResult.Status, cancellationToken);

                purchaseOrder.Lines.Add(new PurchaseOrderLine {
                    Id = Guid.NewGuid(),
                    PurchaseOrderId = purchaseOrder.Id,
                    InventoryItemId = item.Id,
                    InventoryItem = item,
                    QuantityOrdered = lineCommand.QuantityOrdered,
                    QuantityReserved = 0,
                    CreatedBy = command.User.Trim(),
                    CreatedAt = command.OccurredAt
                });
            }

            await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Created(ResponseMapper.ToPurchaseOrderResponse(purchaseOrder));
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

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

    public async Task<Result<PurchaseOrderResponse>> AddLineAsync(AddPurchaseOrderLineCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.");
        if (command.InventoryItemId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Inventory item id is required.");
        if (command.QuantityOrdered < 0) return Result.Fail<PurchaseOrderResponse>("Quantity cannot be negative.");
        if (decimal.Round(command.QuantityOrdered, 3) != command.QuantityOrdered) return Result.Fail<PurchaseOrderResponse>("Quantity cannot have more than 3 decimal places.");
        if (command.QuantityOrdered == 0) return Result.Fail<PurchaseOrderResponse>("Line quantity must be greater than zero.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.");

        return await MutatePurchaseOrder(command.PurchaseOrderId, command.User, command.OccurredAt, cancellationToken, async purchaseOrder => {
            var canChange = purchaseOrderPolicy.CanChangeLines(purchaseOrder);
            if (!canChange.IsSuccess) return canChange;

            if (purchaseOrder.Lines.Any(line => line.InventoryItemId == command.InventoryItemId)) {
                return Result.Fail("Inventory item has already been added to this purchase order.");
            }

            var item = await inventoryItemRepository.GetAsync(command.InventoryItemId, cancellationToken);
            if (item is null) return Result.Fail("Inventory item was not found.", ResultStatus.NotFound);

            var quantityResult = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, command.QuantityOrdered, "Purchase order line quantity");
            if (!quantityResult.IsSuccess) return quantityResult;

            purchaseOrder.Lines.Add(new PurchaseOrderLine {
                Id = Guid.NewGuid(),
                PurchaseOrderId = purchaseOrder.Id,
                InventoryItemId = item.Id,
                InventoryItem = item,
                QuantityOrdered = command.QuantityOrdered,
                QuantityReserved = 0,
                CreatedBy = command.User.Trim(),
                CreatedAt = command.OccurredAt
            });

            return Result.Success();
        });
    }

    public Task<Result<PurchaseOrderResponse>> UpdateLineAsync(UpdatePurchaseOrderLineCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId == Guid.Empty) return Task.FromResult(Result.Fail<PurchaseOrderResponse>("Purchase order id is required."));
        if (command.PurchaseOrderLineId == Guid.Empty) return Task.FromResult(Result.Fail<PurchaseOrderResponse>("Purchase order line id is required."));
        if (command.QuantityOrdered < 0) return Task.FromResult(Result.Fail<PurchaseOrderResponse>("Quantity cannot be negative."));
        if (decimal.Round(command.QuantityOrdered, 3) != command.QuantityOrdered) return Task.FromResult(Result.Fail<PurchaseOrderResponse>("Quantity cannot have more than 3 decimal places."));
        if (command.QuantityOrdered == 0) return Task.FromResult(Result.Fail<PurchaseOrderResponse>("Line quantity must be greater than zero."));
        if (string.IsNullOrWhiteSpace(command.User)) return Task.FromResult(Result.Fail<PurchaseOrderResponse>("User is required."));

        return MutatePurchaseOrder(command.PurchaseOrderId, command.User, command.OccurredAt, cancellationToken, purchaseOrder => {
            var canChange = purchaseOrderPolicy.CanChangeLines(purchaseOrder);
            if (!canChange.IsSuccess) return Task.FromResult(canChange);

            var line = purchaseOrder.Lines.SingleOrDefault(line => line.Id == command.PurchaseOrderLineId);
            if (line is null) return Task.FromResult(Result.Fail("Purchase order line was not found.", ResultStatus.NotFound));
            if (line.InventoryItem is null) return Task.FromResult(Result.Fail("Inventory item was not found.", ResultStatus.NotFound));

            var updateResult = purchaseOrderLinePolicy.CanUpdateQuantity(line, command.QuantityOrdered);
            if (!updateResult.IsSuccess) return Task.FromResult(updateResult);

            var quantityResult = inventoryQuantityPolicy.CanUseQuantity(line.InventoryItem.TrackingMode, command.QuantityOrdered, "Purchase order line quantity");
            if (!quantityResult.IsSuccess) return Task.FromResult(quantityResult);

            line.QuantityOrdered = command.QuantityOrdered;
            line.UpdatedBy = command.User.Trim();
            line.UpdatedAt = command.OccurredAt;
            return Task.FromResult(Result.Success());
        });
    }

    public async Task<Result<PurchaseOrderResponse>> RemoveLineAsync(RemovePurchaseOrderLineCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.");
        if (command.PurchaseOrderLineId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order line id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.");

        return await MutatePurchaseOrder(command.PurchaseOrderId, command.User, command.OccurredAt, cancellationToken, async purchaseOrder => {
            var canChange = purchaseOrderPolicy.CanChangeLines(purchaseOrder);
            if (!canChange.IsSuccess) return canChange;

            var line = purchaseOrder.Lines.SingleOrDefault(line => line.Id == command.PurchaseOrderLineId);
            if (line is null) return Result.Fail("Purchase order line was not found.", ResultStatus.NotFound);

            var activeReservations = await stockReservationRepository.ListActiveByLineAsync(command.PurchaseOrderLineId, cancellationToken);
            var activeReservedByStock = new Dictionary<(Guid WarehouseId, Guid InventoryItemId), decimal>();
            foreach (var reservation in activeReservations) {
                var stock = await warehouseStockRepository.GetForUpdateAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
                if (stock is null) return Result.Fail("Warehouse stock was not found.", ResultStatus.NotFound);

                var stockKey = (reservation.WarehouseId, reservation.InventoryItemId);
                if (!activeReservedByStock.TryGetValue(stockKey, out var activeReservedQuantity)) {
                    activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
                }

                var releasedQuantity = reservation.QuantityReserved;
                reservation.QuantityReserved = 0;
                reservation.Status = ReservationStatus.Released;
                reservation.UpdatedBy = command.User.Trim();
                reservation.UpdatedAt = command.OccurredAt;

                line.QuantityReserved -= releasedQuantity;
                await auditLogRepository.AddAsync(CreateAudit(AuditAction.Release, reservation, releasedQuantity, stock.OnHandQuantity - (activeReservedQuantity - releasedQuantity), command.User, command.OccurredAt), cancellationToken);
                activeReservedByStock[stockKey] = activeReservedQuantity - releasedQuantity;
            }

            purchaseOrder.Lines.Remove(line);
            return Result.Success();
        });
    }

    public Task<Result<PurchaseOrderResponse>> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        return ChangeStatus(command, cancellationToken, (purchaseOrder, user, occurredAt) => {
            var approval = purchaseOrderPolicy.CanApprove(purchaseOrder);
            if (!approval.IsSuccess) return approval;

            purchaseOrder.Status = PurchaseOrderStatus.Approved;
            purchaseOrder.UpdatedBy = user;
            purchaseOrder.UpdatedAt = occurredAt;
            return Result.Success();
        });
    }

    public Task<Result<PurchaseOrderResponse>> CloseAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        return ChangeStatus(command, cancellationToken, (purchaseOrder, user, occurredAt) => {
            purchaseOrder.Status = PurchaseOrderStatus.Closed;
            purchaseOrder.UpdatedBy = user;
            purchaseOrder.UpdatedAt = occurredAt;
            return Result.Success();
        });
    }

    public Task<Result<PurchaseOrderResponse>> CancelAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        return ChangeStatus(command, cancellationToken, (purchaseOrder, user, occurredAt) => {
            var cancellation = purchaseOrderPolicy.CanCancel(purchaseOrder);
            if (!cancellation.IsSuccess) return cancellation;

            purchaseOrder.Status = PurchaseOrderStatus.Cancelled;
            purchaseOrder.UpdatedBy = user;
            purchaseOrder.UpdatedAt = occurredAt;
            return Result.Success();
        });
    }

    private async Task<Result<PurchaseOrderResponse>> ChangeStatus(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken, Func<PurchaseOrder, string, DateTimeOffset, Result> change)
    {
        if (command.PurchaseOrderId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.");

        return await MutatePurchaseOrder(command.PurchaseOrderId, command.User, command.OccurredAt, cancellationToken, purchaseOrder => Task.FromResult(change(purchaseOrder, command.User.Trim(), command.OccurredAt)));
    }

    private async Task<Result<PurchaseOrderResponse>> MutatePurchaseOrder(Guid purchaseOrderId, string user, DateTimeOffset occurredAt, CancellationToken cancellationToken, Func<PurchaseOrder, Task<Result>> mutate)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(purchaseOrderId, cancellationToken);
            if (purchaseOrder is null) return await Rollback<PurchaseOrderResponse>("Purchase order was not found.", ResultStatus.NotFound, cancellationToken);

            var mutation = await mutate(purchaseOrder);
            if (!mutation.IsSuccess) return await Rollback<PurchaseOrderResponse>(mutation.Error!, mutation.Status, cancellationToken);

            purchaseOrder.UpdatedBy = user.Trim();
            purchaseOrder.UpdatedAt = occurredAt;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success(ResponseMapper.ToPurchaseOrderResponse(purchaseOrder));
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<Result<T>> Rollback<T>(string error, ResultStatus status, CancellationToken cancellationToken)
    {
        await unitOfWork.RollbackTransactionAsync(cancellationToken);
        return Result.Fail<T>(error, status);
    }

    private static AuditLogEntry CreateAudit(AuditAction action, StockReservation reservation, decimal quantity, decimal resultingAvailableQuantity, string user, DateTimeOffset occurredAt)
    {
        return new AuditLogEntry {
            Id = Guid.NewGuid(),
            Action = action,
            InventoryItemId = reservation.InventoryItemId,
            WarehouseId = reservation.WarehouseId,
            PurchaseOrderLineId = reservation.PurchaseOrderLineId,
            StockReservationId = reservation.Id,
            Quantity = quantity,
            ResultingAvailableQuantity = resultingAvailableQuantity,
            CreatedBy = user.Trim(),
            CreatedAt = occurredAt
        };
    }
}
