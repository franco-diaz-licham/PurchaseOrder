using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;

namespace PurchaseOrderApp.BL.Workflows;

public interface IPurchaseOrderWorkflowService
{
    Task<Result> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);
}
