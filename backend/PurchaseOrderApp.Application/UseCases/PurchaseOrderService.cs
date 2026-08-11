using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.Helpers;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.UseCases;

public interface IPurchaseOrderService
{
    Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> GetAsync(PurchaseOrderId purchaseOrderId, CancellationToken cancellationToken);

    Task<Result<List<PurchaseOrderResponse>>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> AddLineAsync(AddPurchaseOrderLineCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> CloseAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> CancelAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<List<ApprovedPurchaseOrderLineResponse>>> ListApprovedOutstandingLinesAsync(WarehouseId warehouseId, CancellationToken cancellationToken);
}

public sealed class PurchaseOrderService(
    IPurchaseOrderRepository purchaseOrderRepository,
    IWarehouseRepository warehouseRepository,
    IInventoryItemRepository inventoryItemRepository,
    IUnitOfWork unitOfWork) : IPurchaseOrderService
{
    public async Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.PurchaseOrderNumber)) return Result.Fail<PurchaseOrderResponse>("Purchase order number is required.", ResultStatus.Invalid);
        if (command.WarehouseId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Warehouse id is required.", ResultStatus.Invalid);
        if (command.Lines is null || command.Lines.Count == 0) return Result.Fail<PurchaseOrderResponse>("At least one purchase order line is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<PurchaseOrderResponse>("User is required.", ResultStatus.Invalid);

        foreach (var line in command.Lines) {
            if (line.InventoryItemId.Value == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Inventory item id is required.", ResultStatus.Invalid);
            if (line.QuantityOrdered.IsZero) return Result.Fail<PurchaseOrderResponse>("Line quantity must be greater than zero.", ResultStatus.Invalid);
        }

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var warehouse = await warehouseRepository.GetAsync(command.WarehouseId, cancellationToken);
            if (warehouse is null) return await TransactionResult.RollBackNotFoundAsync<PurchaseOrderResponse>(unitOfWork, "Warehouse was not found.", cancellationToken);

            var purchaseOrder = PurchaseOrder.CreateApproved(command.PurchaseOrderNumber, warehouse.Id, command.User, command.OccurredAt);

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

    public async Task<Result<List<PurchaseOrderResponse>>> ListAsync(WarehouseId? warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId is not null && warehouseId.Value.Value == Guid.Empty) return Result.Fail<List<PurchaseOrderResponse>>("Warehouse id is required.", ResultStatus.Invalid);

        var purchaseOrders = await purchaseOrderRepository.ListResponsesAsync(warehouseId, cancellationToken);
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
