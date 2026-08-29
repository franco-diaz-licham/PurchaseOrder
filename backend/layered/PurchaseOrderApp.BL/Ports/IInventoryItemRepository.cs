using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetAsync(Guid inventoryItemId, CancellationToken cancellationToken);

    Task<List<InventoryItemResponse>> ListResponsesAsync(CancellationToken cancellationToken);
}
