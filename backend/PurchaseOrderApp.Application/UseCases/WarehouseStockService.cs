using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.UseCases;

public interface IWarehouseStockService
{
    /// <summary>
    /// Lists on-hand, reserved, and available stock for one warehouse.
    /// </summary>
    Task<Result<List<WarehouseStockResponse>>> ListAsync(WarehouseId warehouseId, CancellationToken cancellationToken);
}

public sealed class WarehouseStockService(
    IWarehouseStockRepository warehouseStockRepository,
    IStockReservationRepository stockReservationRepository) : IWarehouseStockService
{
    public async Task<Result<List<WarehouseStockResponse>>> ListAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId.Value == Guid.Empty) return Result.Fail<List<WarehouseStockResponse>>("Warehouse id is required.", ResultStatus.Invalid);

        var stockBalances = await warehouseStockRepository.ListAsync(warehouseId, cancellationToken);
        var responses = new List<WarehouseStockResponse>();

        foreach (var stock in stockBalances) {
            var activeReservedQuantity = await stockReservationRepository.GetActiveReservedQuantityAsync(stock.WarehouseId, stock.InventoryItemId, cancellationToken);
            var availableQuantity = stock.CalculateAvailableQuantity(activeReservedQuantity);

            responses.Add(new WarehouseStockResponse(
                stock.WarehouseId.Value,
                stock.InventoryItemId.Value,
                stock.OnHandQuantity.Value,
                activeReservedQuantity.Value,
                availableQuantity.Value));
        }

        return Result.Success(responses);
    }
}
