using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IWarehouseStockRepository
{
    Task<WarehouseStock?> GetForUpdateAsync(Guid warehouseId, Guid inventoryItemId, CancellationToken cancellationToken);

    Task<List<WarehouseStockResponse>> ListResponsesAsync(Guid warehouseId, CancellationToken cancellationToken);
}
