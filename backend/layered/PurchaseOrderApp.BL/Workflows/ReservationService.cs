using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Mappers;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class ReservationService(
    IPurchaseOrderRepository purchaseOrderRepository,
    IInventoryItemRepository inventoryItemRepository,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IAuditLogRepository auditLogRepository,
    PurchaseOrderPolicy purchaseOrderPolicy,
    PurchaseOrderLinePolicy purchaseOrderLinePolicy,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    ReservationPolicy reservationPolicy,
    StockAvailabilityPolicy stockAvailabilityPolicy,
    IUnitOfWork unitOfWork) : IReservationService
{
    public async Task<Result<List<ReservationResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await stockReservationRepository.ListResponsesAsync(cancellationToken));
    }

    public async Task<Result<ReservationResponse>> ReserveAsync(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderLineId == Guid.Empty) return Result.Fail<ReservationResponse>("Purchase order line id is required.");
        if (command.WarehouseId == Guid.Empty) return Result.Fail<ReservationResponse>("Warehouse id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<ReservationResponse>("User is required.");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try {
            var purchaseOrder = await purchaseOrderRepository.GetByLineIdAsync(command.PurchaseOrderLineId, cancellationToken);
            if (purchaseOrder is null) return await Rollback<ReservationResponse>("Purchase order line was not found.", ResultStatus.NotFound, cancellationToken);

            var orderRule = purchaseOrderPolicy.CanReserve(purchaseOrder);
            if (!orderRule.IsSuccess) return await Rollback<ReservationResponse>(orderRule.Error!, orderRule.Status, cancellationToken);

            var line = purchaseOrder.Lines.Single(line => line.Id == command.PurchaseOrderLineId);
            var item = await inventoryItemRepository.GetAsync(line.InventoryItemId, cancellationToken);
            if (item is null) return await Rollback<ReservationResponse>("Inventory item was not found.", ResultStatus.NotFound, cancellationToken);

            var quantityRule = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, command.Quantity, "Reservation quantity");
            if (!quantityRule.IsSuccess) return await Rollback<ReservationResponse>(quantityRule.Error!, quantityRule.Status, cancellationToken);

            var lineRule = purchaseOrderLinePolicy.CanReserve(line, command.Quantity);
            if (!lineRule.IsSuccess) return await Rollback<ReservationResponse>(lineRule.Error!, lineRule.Status, cancellationToken);

            var stock = await warehouseStockRepository.GetForUpdateAsync(command.WarehouseId, item.Id, cancellationToken);
            if (stock is null) return await Rollback<ReservationResponse>("Warehouse stock was not found.", ResultStatus.NotFound, cancellationToken);

            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(command.WarehouseId, item.Id, cancellationToken);
            var stockRule = stockAvailabilityPolicy.CanReserve(stock, activeReservedQuantity, command.Quantity);
            if (!stockRule.IsSuccess) return await Rollback<ReservationResponse>(stockRule.Error!, stockRule.Status, cancellationToken);

            line.QuantityReserved += command.Quantity;
            line.UpdatedBy = command.User.Trim();
            line.UpdatedAt = command.OccurredAt;
            purchaseOrder.UpdatedBy = command.User.Trim();
            purchaseOrder.UpdatedAt = command.OccurredAt;

            var reservation = new StockReservation {
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

            await stockReservationRepository.AddAsync(reservation, cancellationToken);
            await auditLogRepository.AddAsync(CreateAudit(AuditAction.Reserve, reservation, command.Quantity, stock.OnHandQuantity - (activeReservedQuantity + command.Quantity), command.User, command.OccurredAt), cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Created(ResponseMapper.ToReservationResponse(reservation));
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<ReservationResponse>> ReleaseAsync(ReleaseReservationCommand command, CancellationToken cancellationToken)
    {
        if (command.StockReservationId == Guid.Empty) return Result.Fail<ReservationResponse>("Stock reservation id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<ReservationResponse>("User is required.");

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try {
            var reservation = await stockReservationRepository.GetAsync(command.StockReservationId, cancellationToken);
            if (reservation is null) return await Rollback<ReservationResponse>("Stock reservation was not found.", ResultStatus.NotFound, cancellationToken);

            var item = await inventoryItemRepository.GetAsync(reservation.InventoryItemId, cancellationToken);
            if (item is null) return await Rollback<ReservationResponse>("Inventory item was not found.", ResultStatus.NotFound, cancellationToken);

            var quantityRule = inventoryQuantityPolicy.CanUseQuantity(item.TrackingMode, command.Quantity, "Release quantity");
            if (!quantityRule.IsSuccess) return await Rollback<ReservationResponse>(quantityRule.Error!, quantityRule.Status, cancellationToken);

            var reservationRule = reservationPolicy.CanRelease(reservation, command.Quantity);
            if (!reservationRule.IsSuccess) return await Rollback<ReservationResponse>(reservationRule.Error!, reservationRule.Status, cancellationToken);

            var purchaseOrder = await purchaseOrderRepository.GetByLineIdAsync(reservation.PurchaseOrderLineId, cancellationToken);
            if (purchaseOrder is null) return await Rollback<ReservationResponse>("Purchase order line was not found.", ResultStatus.NotFound, cancellationToken);

            var line = purchaseOrder.Lines.Single(line => line.Id == reservation.PurchaseOrderLineId);
            var lineRule = purchaseOrderLinePolicy.CanRelease(line, command.Quantity);
            if (!lineRule.IsSuccess) return await Rollback<ReservationResponse>(lineRule.Error!, lineRule.Status, cancellationToken);

            var stock = await warehouseStockRepository.GetForUpdateAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
            if (stock is null) return await Rollback<ReservationResponse>("Warehouse stock was not found.", ResultStatus.NotFound, cancellationToken);

            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
            var resultingAvailableQuantity = stock.OnHandQuantity - (activeReservedQuantity - command.Quantity);

            reservation.QuantityReserved -= command.Quantity;
            if (reservation.QuantityReserved == 0) reservation.Status = ReservationStatus.Released;
            reservation.UpdatedBy = command.User.Trim();
            reservation.UpdatedAt = command.OccurredAt;

            line.QuantityReserved -= command.Quantity;
            line.UpdatedBy = command.User.Trim();
            line.UpdatedAt = command.OccurredAt;
            purchaseOrder.UpdatedBy = command.User.Trim();
            purchaseOrder.UpdatedAt = command.OccurredAt;

            await auditLogRepository.AddAsync(CreateAudit(AuditAction.Release, reservation, command.Quantity, resultingAvailableQuantity, command.User, command.OccurredAt), cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(ResponseMapper.ToReservationResponse(reservation));
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
