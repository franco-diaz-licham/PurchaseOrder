using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Application.UseCases;

/// <summary>
/// Coordinates warehouse stock balance reads.
/// </summary>
public interface IWarehouseStockService
{
    /// <summary>
    /// Lists on-hand, reserved, and available stock for one warehouse.
    /// </summary>
    Task<Result<List<WarehouseStockResponse>>> ListAsync(WarehouseId warehouseId, CancellationToken cancellationToken);
}

public sealed class WarehouseStockService(IWarehouseStockRepository warehouseStockRepository) : IWarehouseStockService
{
    public async Task<Result<List<WarehouseStockResponse>>> ListAsync(WarehouseId warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId.Value == Guid.Empty) return Result.Fail<List<WarehouseStockResponse>>("Warehouse id is required.", ResultStatus.Invalid);

        var responses = await warehouseStockRepository.ListResponsesAsync(warehouseId, cancellationToken);
        return Result.Success(responses);
    }
}
