using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows.PurchaseOrders;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class PurchaseOrderWorkflowService(
    IPurchaseOrderRepository purchaseOrderRepository,
    SubmitPurchaseOrderWorkflow submitPurchaseOrder,
    AddPurchaseOrderLineWorkflow addPurchaseOrderLine,
    UpdatePurchaseOrderLineWorkflow updatePurchaseOrderLine,
    RemovePurchaseOrderLineWorkflow removePurchaseOrderLine,
    ChangePurchaseOrderStatusWorkflow changePurchaseOrderStatus) : IPurchaseOrderWorkflowService
{
    public Task<Result<PurchaseOrderResponse>> SubmitAsync(SubmitPurchaseOrderCommand command, CancellationToken cancellationToken) =>
        submitPurchaseOrder.ExecuteAsync(command, cancellationToken);

    public async Task<Result<PurchaseOrderResponse>> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        if (purchaseOrderId == Guid.Empty) return Result.Fail<PurchaseOrderResponse>("Purchase order id is required.");

        var purchaseOrder = await purchaseOrderRepository.GetResponseAsync(purchaseOrderId, cancellationToken);
        return purchaseOrder is null
            ? Result.Fail<PurchaseOrderResponse>("Purchase order was not found.", ResultStatus.NotFound)
            : Result.Success(purchaseOrder);
    }

    public async Task<Result<List<PurchaseOrderSummaryResponse>>> ListSummariesAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await purchaseOrderRepository.ListSummariesAsync(cancellationToken));
    }

    public Task<Result<PurchaseOrderResponse>> AddLineAsync(AddPurchaseOrderLineCommand command, CancellationToken cancellationToken) => addPurchaseOrderLine.ExecuteAsync(command, cancellationToken);

    public Task<Result<PurchaseOrderResponse>> UpdateLineAsync(UpdatePurchaseOrderLineCommand command, CancellationToken cancellationToken) => updatePurchaseOrderLine.ExecuteAsync(command, cancellationToken);

    public Task<Result<PurchaseOrderResponse>> RemoveLineAsync(RemovePurchaseOrderLineCommand command, CancellationToken cancellationToken) => removePurchaseOrderLine.ExecuteAsync(command, cancellationToken);

    public Task<Result<PurchaseOrderResponse>> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken) => changePurchaseOrderStatus.ApproveAsync(command, cancellationToken);

    public Task<Result<PurchaseOrderResponse>> CloseAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken) => changePurchaseOrderStatus.CloseAsync(command, cancellationToken);

    public Task<Result<PurchaseOrderResponse>> CancelAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken) => changePurchaseOrderStatus.CancelAsync(command, cancellationToken);
}
