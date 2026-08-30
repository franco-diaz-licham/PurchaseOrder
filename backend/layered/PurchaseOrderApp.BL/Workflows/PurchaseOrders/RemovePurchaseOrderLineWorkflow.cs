using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.PurchaseOrders;

public sealed class RemovePurchaseOrderLineWorkflow(
    PurchaseOrderMutationRunner purchaseOrderMutationRunner,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IAuditLogRepository auditLogRepository,
    PurchaseOrderPolicy purchaseOrderPolicy)
{
    public Task<Result<PurchaseOrderResponse>> ExecuteAsync(
        RemovePurchaseOrderLineCommand command,
        CancellationToken cancellationToken)
    {
        var validation = CommandValidation.All(
            CommandValidation.Required(command.PurchaseOrderId, "Purchase order id is required."),
            CommandValidation.Required(command.PurchaseOrderLineId, "Purchase order line id is required."),
            CommandValidation.User(command.User));
        if (!validation.IsSuccess) return Task.FromResult(Result.Fail<PurchaseOrderResponse>(validation.Error!, validation.Status));

        return purchaseOrderMutationRunner.RunAsync(
            command.PurchaseOrderId,
            command.User,
            command.OccurredAt,
            async (purchaseOrder, ct) => {
                var canChange = purchaseOrderPolicy.CanChangeLines(purchaseOrder);
                if (!canChange.IsSuccess) return canChange;

                var line = purchaseOrder.Lines.SingleOrDefault(line => line.Id == command.PurchaseOrderLineId);
                if (line is null) return Result.Fail("Purchase order line was not found.", ResultStatus.NotFound);

                var releaseResult = await ReleaseActiveReservationsAsync(line, command, ct);
                if (!releaseResult.IsSuccess) return releaseResult;

                purchaseOrder.Lines.Remove(line);
                return Result.Success();
            },
            cancellationToken);
    }

    private async Task<Result> ReleaseActiveReservationsAsync(
        Models.PurchaseOrderLine line,
        RemovePurchaseOrderLineCommand command,
        CancellationToken cancellationToken)
    {
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
            var resultingAvailableQuantity = stock.OnHandQuantity - (activeReservedQuantity - releasedQuantity);

            reservation.QuantityReserved = 0;
            reservation.Status = ReservationStatus.Released;
            reservation.UpdatedBy = command.User.Trim();
            reservation.UpdatedAt = command.OccurredAt;
            line.QuantityReserved -= releasedQuantity;

            var auditEntry = AuditLogFactory.Create(AuditAction.Release, reservation, releasedQuantity, resultingAvailableQuantity, command.User, command.OccurredAt);
            await auditLogRepository.AddAsync(auditEntry, cancellationToken);

            activeReservedByStock[stockKey] = activeReservedQuantity - releasedQuantity;
        }

        return Result.Success();
    }
}
