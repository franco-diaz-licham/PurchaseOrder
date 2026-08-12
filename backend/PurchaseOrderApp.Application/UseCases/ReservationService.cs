using PurchaseOrderApp.Application.Helpers;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Core;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.UseCases;

public interface IReservationService
{
    Task<Result<ReservationResponse>> GetAsync(StockReservationId stockReservationId, CancellationToken cancellationToken);

    Task<Result<List<ReservationResponse>>> ListAsync(WarehouseId? warehouseId, ReservationStatus? status, CancellationToken cancellationToken);

    Task<Result<ReservationResponse>> ReserveAsync(CreateReservationCommand command, CancellationToken cancellationToken);

    Task<Result<ReservationResponse>> ReleaseAsync(ReleaseReservationCommand command, CancellationToken cancellationToken);
}

public sealed class ReservationService(
    IPurchaseOrderRepository purchaseOrderRepository,
    IInventoryItemRepository inventoryItemRepository,
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository,
    IUnitOfWork unitOfWork) : IReservationService
{
    public async Task<Result<ReservationResponse>> GetAsync(StockReservationId stockReservationId, CancellationToken cancellationToken)
    {
        if (stockReservationId.Value == Guid.Empty) return Result.Fail<ReservationResponse>("Stock reservation id is required.", ResultStatus.Invalid);

        var reservation = await stockReservationRepository.GetResponseAsync(stockReservationId, cancellationToken);
        if (reservation is null) return Result.Fail<ReservationResponse>("Stock reservation was not found.", ResultStatus.NotFound);

        return Result.Success(reservation);
    }

    public async Task<Result<List<ReservationResponse>>> ListAsync(WarehouseId? warehouseId, ReservationStatus? status, CancellationToken cancellationToken)
    {
        if (warehouseId is not null && warehouseId.Value.Value == Guid.Empty) return Result.Fail<List<ReservationResponse>>("Warehouse id is required.", ResultStatus.Invalid);

        var reservations = await stockReservationRepository.ListResponsesAsync(warehouseId, status, cancellationToken);
        return Result.Success(reservations);
    }

    public async Task<Result<ReservationResponse>> ReserveAsync(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderLineId.Value == Guid.Empty) return Result.Fail<ReservationResponse>("Purchase order line id is required.", ResultStatus.Invalid);
        if (command.WarehouseId.Value == Guid.Empty) return Result.Fail<ReservationResponse>("Warehouse id is required.", ResultStatus.Invalid);
        if (command.Quantity.IsZero) return Result.Fail<ReservationResponse>("Reservation quantity must be greater than zero.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<ReservationResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetByLineIdAsync(command.PurchaseOrderLineId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Purchase order line was not found.", cancellationToken);

            var line = purchaseOrder.Lines.Single(line => line.Id == command.PurchaseOrderLineId);
            var item = await inventoryItemRepository.GetAsync(line.InventoryItemId, cancellationToken);
            if (item is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Inventory item was not found.", cancellationToken);

            item.EnsureValidQuantity(command.Quantity);

            var stock = await warehouseStockRepository.GetForUpdateAsync(command.WarehouseId, item.Id, cancellationToken);
            if (stock is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Warehouse stock was not found.", cancellationToken);

            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(command.WarehouseId, item.Id, cancellationToken);
            var reservation = StockReservationDomainService.Reserve(
                purchaseOrder,
                command.PurchaseOrderLineId,
                stock,
                item,
                activeReservedQuantity,
                command.Quantity,
                command.User,
                command.OccurredAt);

            await stockReservationRepository.AddAsync(reservation, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Created(ReservationMapper.ToResponse(reservation));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<ReservationResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Result<ReservationResponse>> ReleaseAsync(ReleaseReservationCommand command, CancellationToken cancellationToken)
    {
        if (command.StockReservationId.Value == Guid.Empty) return Result.Fail<ReservationResponse>("Stock reservation id is required.", ResultStatus.Invalid);
        if (command.Quantity.IsZero) return Result.Fail<ReservationResponse>("Release quantity must be greater than zero.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail<ReservationResponse>("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var reservation = await stockReservationRepository.GetAsync(command.StockReservationId, cancellationToken);
            if (reservation is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Stock reservation was not found.", cancellationToken);

            var item = await inventoryItemRepository.GetAsync(reservation.InventoryItemId, cancellationToken);
            if (item is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Inventory item was not found.", cancellationToken);

            item.EnsureValidQuantity(command.Quantity);

            var purchaseOrder = await purchaseOrderRepository.GetByLineIdAsync(reservation.PurchaseOrderLineId, cancellationToken);
            if (purchaseOrder is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Purchase order line was not found.", cancellationToken);

            var stock = await warehouseStockRepository.GetForUpdateAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
            if (stock is null) return await TransactionResult.RollBackNotFoundAsync<ReservationResponse>(unitOfWork, "Warehouse stock was not found.", cancellationToken);

            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(reservation.WarehouseId, reservation.InventoryItemId, cancellationToken);
            StockReservationDomainService.Release(
                purchaseOrder,
                reservation,
                stock,
                activeReservedQuantity,
                command.Quantity,
                command.User,
                command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success(ReservationMapper.ToResponse(reservation));
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail<ReservationResponse>(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

}
