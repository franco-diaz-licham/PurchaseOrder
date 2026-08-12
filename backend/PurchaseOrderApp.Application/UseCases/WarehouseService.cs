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
        var response = await warehouseRepository.ListResponsesAsync(cancellationToken);
        return Result.Success(response);
    }
}
