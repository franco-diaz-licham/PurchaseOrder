using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.Warehouses;

public sealed record ListWarehousesQuery;

public sealed class ListWarehousesQueryHandler(IWarehouseRepository warehouseRepository)
{
    public async Task<Result<List<WarehouseResponse>>> ExecuteAsync(ListWarehousesQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await warehouseRepository.ListResponsesAsync(cancellationToken));
    }
}
