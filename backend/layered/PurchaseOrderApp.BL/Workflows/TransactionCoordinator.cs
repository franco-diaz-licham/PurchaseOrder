using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class TransactionCoordinator(IUnitOfWork unitOfWork)
{
    public async Task<Result<T>> ExecuteAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var result = await operation(cancellationToken);
            if (!result.IsSuccess) {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return result;
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return result;
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
