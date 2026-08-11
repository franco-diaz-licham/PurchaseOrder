using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;

namespace PurchaseOrderApp.Application.Helpers;

public static class TransactionResult
{
    public static async Task<Result<T>> RollBackNotFoundAsync<T>(IUnitOfWork unitOfWork, string message, CancellationToken cancellationToken)
    {
        await unitOfWork.RollbackTransactionAsync(cancellationToken);
        return Result.Fail<T>(message, ResultStatus.NotFound);
    }
}
