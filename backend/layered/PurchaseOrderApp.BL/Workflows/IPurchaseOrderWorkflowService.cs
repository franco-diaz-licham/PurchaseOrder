using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public interface IPurchaseOrderWorkflowService
{
    Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken);

    Task<Result<List<PurchaseOrderSummaryResponse>>> ListSummariesAsync(CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> AddLineAsync(AddPurchaseOrderLineCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> UpdateLineAsync(UpdatePurchaseOrderLineCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> RemoveLineAsync(RemovePurchaseOrderLineCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> CloseAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);

    Task<Result<PurchaseOrderResponse>> CancelAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken);
}
