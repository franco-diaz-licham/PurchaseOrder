using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.Warehouses;

public sealed record ListWarehouseStockQuery(Guid WarehouseId);

public sealed class ListWarehouseStockQueryHandler(IWarehouseStockRepository warehouseStockRepository)
{
    public async Task<Result<List<WarehouseStockResponse>>> ExecuteAsync(ListWarehouseStockQuery query, CancellationToken cancellationToken)
    {
        if (query.WarehouseId == Guid.Empty) return Result.Fail<List<WarehouseStockResponse>>("Warehouse id is required.");

        return Result.Success(await warehouseStockRepository.ListResponsesAsync(query.WarehouseId, cancellationToken));
    }
}
