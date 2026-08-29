using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public interface IInventoryItemService
{
    Task<Result<List<InventoryItemResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<Result> ChangeStandardCostAsync(ChangeInventoryItemStandardCostCommand command, CancellationToken cancellationToken);
}
