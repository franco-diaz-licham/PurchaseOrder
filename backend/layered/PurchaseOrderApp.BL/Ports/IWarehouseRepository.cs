using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetAsync(Guid warehouseId, CancellationToken cancellationToken);

    Task<List<WarehouseResponse>> ListResponsesAsync(CancellationToken cancellationToken);
}
