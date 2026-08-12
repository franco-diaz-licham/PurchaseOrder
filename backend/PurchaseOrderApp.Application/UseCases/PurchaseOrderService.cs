using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.Helpers;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.UseCases;

public interface IPurchaseOrderService
{
    Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> GetAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken);

    Task<Result<List<PurchaseOrderSummaryResponse>>> ListSummariesAsync(CancellationToken cancellationToken);

    Task<Result<List<PurchaseOrderResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> AddLineAsync(AddPurchaseOrderLineCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> RemoveLineAsync(RemovePurchaseOrderLineCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> CloseAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> CancelAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<List<ApprovedPurchaseOrderLineResponse>>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken);
}

public sealed class PurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    IWarehouseRepository warehouseRepository,
    IInventoryItemRepository inventoryItemRepository,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IUnitOfWork unitOfWork) : IPurchaseOrderService
{
    public async Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        if (command.WarehouseId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Warehouse id is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        foreach (var line in command.Lines) {
            if (line.InventoryItemId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Inventory item id is required.", ResultStatus.Invalid);
            if (line.QuantityOrdered.IsZero) return Result.Fail<PurchaseOrderResponse>("Line quantity must be greater than zero.", ResultStatus.Invalid);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var warehouse = await warehouseRepository.GetAsync(command.WarehouseId, cancellationToken);
            if (warehouse is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Warehouse was not found.", cancellationToken);

            var purchaseOrder = PurchaseOrder.CreatePending(warehouse.Id, command.User, command.OccurredAt);

            foreach (var line in command.Lines) {
                var item = await inventoryItemRepository.GetAsync(line.InventoryItemId, cancellationToken);
                if (item is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Inventory item was not found.", cancellationToken);
                purchaseOrder.AddLine(item, line.QuantityOrdered, command.User, command.OccurredAt);
            }

            await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Created(PurchaseOrderMapper.ToResponse(purchaseOrder));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<PurchaseOrderResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<PurchaseOrderResponse>> GetAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken)
    {
        if (purchaseOrderId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.", ResultStatus.Invalid);

        var purchaseOrder = await purchaseOrderRepository.GetResponseAsync(purchaseOrderId, cancellationToken);
        if (purchaseOrder is null) return Result.Fail<PurchaseOrderResponse>("Purchase order was not found.", ResultStatus.NotFound);

        return Result.Success(purchaseOrder);
    }

    public async Task<Result<List<PurchaseOrderResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var purchaseOrders = await purchaseOrderRepository.ListResponsesAsync(cancellationToken);
        return Result.Success(purchaseOrders);
    }

    public async Task<Result<List<PurchaseOrderSummaryResponse>>> ListSummariesAsync(CancellationToken cancellationToken)
    {
        var purchaseOrders = await purchaseOrderRepository.ListSummariesAsync(cancellationToken);
        return Result.Success(purchaseOrders);
    }

    public async Task<Result<PurchaseOrderResponse>> AddLineAsync(AddPurchaseOrderLineCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.", ResultStatus.Invalid);
        if (command.InventoryItemId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Inventory item id is required.", ResultStatus.Invalid);
        if (command.QuantityOrdered.IsZero) return Result.Fail<PurchaseOrderResponse>("Line quantity must be greater than zero.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(command.PurchaseOrderId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Purchase order was not found.", cancellationToken);

            var item = await inventoryItemRepository.GetAsync(command.InventoryItemId, cancellationToken);
            if (item is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Inventory item was not found.", cancellationToken);

            purchaseOrder.AddLine(item, command.QuantityOrdered, command.User, command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(PurchaseOrderMapper.ToResponse(purchaseOrder));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<PurchaseOrderResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<PurchaseOrderResponse>> RemoveLineAsync(RemovePurchaseOrderLineCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.", ResultStatus.Invalid);
        if (command.PurchaseOrderLineId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order line id is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(command.PurchaseOrderId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Purchase order was not found.", cancellationToken);

            var line = purchaseOrder.Lines.SingleOrDefault(line => line.Id == command.PurchaseOrderLineId);
            if (line is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Purchase order line was not found.", cancellationToken);

            var activeReservations = await stockReservationRepository.ListActiveByLineAsync(command.PurchaseOrderLineId, cancellationToken);
            var activeReservedByItem = new Dictionary<InventoryItemId, Quantity>();

            foreach (var reservation in activeReservations) {
                var stock = await warehouseStockRepository.GetForUpdateAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
                if (stock is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Warehouse stock was not found.", cancellationToken);

                if (!activeReservedByItem.TryGetValue(reservation.InventoryItemId, out var activeReservedQuantity)) {
                    activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
                }

                var releaseQuantity = reservation.QuantityReserved;
                StockReservationDomainService.Release(
                    purchaseOrder,
                    reservation,
                    stock,
                    activeReservedQuantity,
                    releaseQuantity,
                    command.User,
                    command.OccurredAt);

                activeReservedByItem[reservation.InventoryItemId] = activeReservedQuantity.Subtract(releaseQuantity);
            }

            purchaseOrder.RemoveLine(command.PurchaseOrderLineId, command.User, command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(PurchaseOrderMapper.ToResponse(purchaseOrder));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<PurchaseOrderResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<PurchaseOrderResponse>> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(command.PurchaseOrderId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Purchase order was not found.", cancellationToken);

            purchaseOrder.Approve(command.User, command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(PurchaseOrderMapper.ToResponse(purchaseOrder));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<PurchaseOrderResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<PurchaseOrderResponse>> CloseAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(command.PurchaseOrderId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Purchase order was not found.", cancellationToken);

            purchaseOrder.Close(command.User, command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(PurchaseOrderMapper.ToResponse(purchaseOrder));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<PurchaseOrderResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<PurchaseOrderResponse>> CancelAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(command.PurchaseOrderId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Purchase order was not found.", cancellationToken);

            purchaseOrder.Cancel(command.User, command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(PurchaseOrderMapper.ToResponse(purchaseOrder));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<PurchaseOrderResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<List<ApprovedPurchaseOrderLineResponse>>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId.Value == Guid.Empty) return Result.Fail<List<ApprovedPurchaseOrderLineResponse>>("Warehouse id is required.", ResultStatus.Invalid);
        var lines = await purchaseOrderRepository.ListApprovedOutstandingLinesAsync(warehouseId, cancellationToken);
        return Result.Success(lines);
    }
}
