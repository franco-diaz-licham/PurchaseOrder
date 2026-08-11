using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Application.UseCases;

public interface IWarehouseService
{
    Task<Result<List<WarehouseResponse>>> ListAsync(CancellationToken cancellationToken);
}

public sealed class WarehouseService(IWarehouseRepository warehouseRepository) : IWarehouseService
{
    public async Task<Result<List<WarehouseResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var warehouses = await warehouseRepository.ListAsync(cancellationToken);
        var response = warehouses
            .Select(warehouse => new WarehouseResponse(warehouse.Id.Value, warehouse.Code, warehouse.Name))
            .ToList();

        return Result.Success(response);
    }
}
