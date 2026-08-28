using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Enums;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class PurchaseOrderWorkflowService(
    IPurchaseOrderRepository purchaseOrderRepository,
    PurchaseOrderApprovalPolicy approvalPolicy,
    IUnitOfWork unitOfWork) : IPurchaseOrderWorkflowService
{
    public async Task<Result> ApproveAsync(ChangePurchaseOrderStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.PurchaseOrderId == Guid.Empty) return Result.Fail("Purchase order id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail("User is required.");

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var purchaseOrder = await purchaseOrderRepository.GetAsync(command.PurchaseOrderId, cancellationToken);
            if (purchaseOrder is null) {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Fail("Purchase order was not found.", ResultStatus.NotFound);
            }

            var approval = approvalPolicy.CanApprove(purchaseOrder);
            if (!approval.IsSuccess) {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return approval;
            }

            purchaseOrder.Status = PurchaseOrderStatus.Approved;
            purchaseOrder.UpdatedBy = command.User;
            purchaseOrder.UpdatedAt = command.OccurredAt;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
