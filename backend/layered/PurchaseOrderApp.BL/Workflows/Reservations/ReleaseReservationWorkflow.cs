using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Mappers;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.Reservations;

public sealed class ReleaseReservationWorkflow(
    IPurchaseOrderRepository purchaseOrderRepository,
    IInventoryItemRepository inventoryItemRepository,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IAuditLogRepository auditLogRepository,
    PurchaseOrderLinePolicy purchaseOrderLinePolicy,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    ReservationPolicy reservationPolicy,
    TransactionCoordinator transactionCoordinator)
{
    public Task<Result<ReservationResponse>> ExecuteAsync(
        ReleaseReservationCommand command,
        CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.StockReservationId, "Stock reservation id is required."),
            CommandValidation.Quantity(command.Quantity, "Release quantity must be greater than zero."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<ReservationResponse>(validation.Error!, validation.Status));

        return transactionCoordinator.ExecuteAsync(async ct => {
            var reservation = await stockReservationRepository.GetAsync(command.StockReservationId, ct);
            if (reservation is null) return Result.Fail<ReservationResponse>("Stock reservation was not found.", ResultStatus.NotFound);

            var item = await inventoryItemRepository.GetAsync(reservation.InventoryItemId, ct);
            if (item is null) return Result.Fail<ReservationResponse>("Inventory item was not found.", ResultStatus.NotFound);

            var quantityRule = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, command.Quantity, "Release quantity");
            if (!quantityRule.IsSuccess) return Result.Fail<ReservationResponse>(quantityRule.Error!, quantityRule.Status);

            var purchaseOrder = await purchaseOrderRepository.GetByLineIdAsync(reservation.PurchaseOrderLineId, ct);
            if (purchaseOrder is null) return Result.Fail<ReservationResponse>("Purchase order line was not found.", ResultStatus.NotFound);

            var line = purchaseOrder.Lines.Single(line => line.Id == reservation.PurchaseOrderLineId);
            var stock = await warehouseStockRepository.GetForUpdateAsync(reservation.WarehouseId, reservation.InventoryItemId, ct);
            if (stock is null) return Result.Fail<ReservationResponse>("Warehouse stock was not found.", ResultStatus.NotFound);

            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(reservation.WarehouseId, reservation.InventoryItemId, ct);
            var resultingAvailableQuantity = stock.OnHandQuantity - (activeReservedQuantity - command.Quantity);

            var reservationRule = reservationPolicy.CanRelease(reservation, command.Quantity);
            if (!reservationRule.IsSuccess) return Result.Fail<ReservationResponse>(reservationRule.Error!, reservationRule.Status);

            var lineRule = purchaseOrderLinePolicy.CanRelease(line, command.Quantity);
            if (!lineRule.IsSuccess) return Result.Fail<ReservationResponse>(lineRule.Error!, lineRule.Status);

            reservation.QuantityReserved -= command.Quantity;
            if (reservation.QuantityReserved == 0) reservation.Status = ReservationStatus.Released;
            reservation.UpdatedBy = command.User.Trim();
            reservation.UpdatedAt = command.OccurredAt;

            line.QuantityReserved -= command.Quantity;
            line.UpdatedBy = command.User.Trim();
            line.UpdatedAt = command.OccurredAt;
            purchaseOrder.UpdatedBy = command.User.Trim();
            purchaseOrder.UpdatedAt = command.OccurredAt;

            await auditLogRepository.AddAsync(AuditLogFactory.Create(AuditAction.Release, reservation, command.Quantity, resultingAvailableQuantity, command.User, command.OccurredAt), ct);
            return Result.Success(ResponseMapper.ToReservationResponse(reservation));
        }, cancellationToken);
    }
}
