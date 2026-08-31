using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Mappers;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Commands.Reservations;

public sealed class ReserveStockCommandHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    IInventoryItemRepository inventoryItemRepository,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IAuditLogRepository auditLogRepository,
    PurchaseOrderPolicy purchaseOrderPolicy,
    PurchaseOrderLinePolicy purchaseOrderLinePolicy,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    StockAvailabilityPolicy stockAvailabilityPolicy,
    TransactionCoordinator transactionCoordinator)
{
    public Task<Result<ReservationResponse>> ExecuteAsync(
        CreateReservationCommand command,
        CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.PurchaseOrderLineId, "Purchase order line id is required."),
            CommandValidation.Required(command.WarehouseId, "Warehouse id is required."),
            CommandValidation.Quantity(command.Quantity, "Reservation quantity must be greater than zero."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<ReservationResponse>(validation.Error!, validation.Status));

        return transactionCoordinator.ExecuteAsync(async ct => {
            var purchaseOrder = await purchaseOrderRepository.GetByLineIdAsync(command.PurchaseOrderLineId, ct);
            if (purchaseOrder is null) return Result.Fail<ReservationResponse>("Purchase order line was not found.", ResultStatus.NotFound);

            var orderRule = purchaseOrderPolicy.CanReserve(purchaseOrder);
            if (!orderRule.IsSuccess) return Result.Fail<ReservationResponse>(orderRule.Error!, orderRule.Status);

            var line = purchaseOrder.Lines.Single(line => line.Id == command.PurchaseOrderLineId);
            var item = await inventoryItemRepository.GetAsync(line.InventoryItemId, ct);
            if (item is null) return Result.Fail<ReservationResponse>("Inventory item was not found.", ResultStatus.NotFound);

            var quantityRule = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, command.Quantity, "Reservation quantity");
            if (!quantityRule.IsSuccess) return Result.Fail<ReservationResponse>(quantityRule.Error!, quantityRule.Status);

            var stock = await warehouseStockRepository.GetForUpdateAsync(command.WarehouseId, item.Id, ct);
            if (stock is null) return Result.Fail<ReservationResponse>("Warehouse stock was not found.", ResultStatus.NotFound);

            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(command.WarehouseId, item.Id, ct);
            var stockRule = stockAvailabilityPolicy.CanReserve(stock, activeReservedQuantity, command.Quantity);
            if (!stockRule.IsSuccess) return Result.Fail<ReservationResponse>(stockRule.Error!, stockRule.Status);

            var lineRule = purchaseOrderLinePolicy.CanReserve(line, command.Quantity);
            if (!lineRule.IsSuccess) return Result.Fail<ReservationResponse>(lineRule.Error!, lineRule.Status);

            var reservation = ApplyReservation(command, purchaseOrder, line, item, stock);
            await stockReservationRepository.AddAsync(reservation, ct);

            var resultingAvailableQuantity = stock.OnHandQuantity - (activeReservedQuantity + command.Quantity);
            await auditLogRepository.AddAsync(AuditLogFactory.Create(AuditAction.Reserve, reservation, command.Quantity, resultingAvailableQuantity, command.User, command.OccurredAt), ct);

            return Result.Created(ResponseMapper.ToReservationResponse(reservation));
        }, cancellationToken);
    }

    private static StockReservation ApplyReservation(
        CreateReservationCommand command,
        PurchaseOrder purchaseOrder,
        PurchaseOrderLine line,
        InventoryItem item,
        WarehouseStock stock)
    {
        line.QuantityReserved += command.Quantity;
        line.UpdatedBy = command.User.Trim();
        line.UpdatedAt = command.OccurredAt;
        purchaseOrder.UpdatedBy = command.User.Trim();
        purchaseOrder.UpdatedAt = command.OccurredAt;

        return new StockReservation {
            Id = Guid.NewGuid(),
            PurchaseOrderLineId = line.Id,
            WarehouseId = stock.WarehouseId,
            InventoryItemId = item.Id,
            QuantityReserved = command.Quantity,
            UnitCostSnapshot = item.StandardCost,
            Status = ReservationStatus.Active,
            CreatedBy = command.User.Trim(),
            CreatedAt = command.OccurredAt
        };
    }
}
