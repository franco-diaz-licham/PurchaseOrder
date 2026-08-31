using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.Inventory;

public sealed record ListInventoryItemsQuery;

public sealed class ListInventoryItemsQueryHandler(IInventoryItemRepository inventoryItemRepository)
{
    public async Task<Result<List<InventoryItemResponse>>> ExecuteAsync(ListInventoryItemsQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await inventoryItemRepository.ListResponsesAsync(cancellationToken));
    }
}
