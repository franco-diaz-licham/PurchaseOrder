using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Policies;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class InventoryItemService(
    IInventoryItemRepository inventoryItemRepository,
    InventoryQuantityPolicy inventoryQuantityPolicy,
    IUnitOfWork unitOfWork) : IInventoryItemService
{
    public async Task<Result<List<InventoryItemResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await inventoryItemRepository.ListResponsesAsync(cancellationToken));
    }

    public async Task<Result> ChangeStandardCostAsync(ChangeInventoryItemStandardCostCommand command, CancellationToken cancellationToken)
    {
        if (command.InventoryItemId == Guid.Empty) return Result.Fail("Inventory item id is required.");
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail("User is required.");

        var costRule = inventoryQuantityPolicy.CanUseCost(command.StandardCost);
        if (!costRule.IsSuccess) return costRule;

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try {
            var item = await inventoryItemRepository.GetAsync(command.InventoryItemId, cancellationToken);
            if (item is null) {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Fail("Inventory item was not found.", ResultStatus.NotFound);
            }

            item.StandardCost = command.StandardCost;
            item.UpdatedBy = command.User.Trim();
            item.UpdatedAt = command.OccurredAt;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
            return Result.Success();
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

public sealed class WarehouseService(IWarehouseRepository warehouseRepository) : IWarehouseService
{
    public async Task<Result<List<WarehouseResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await warehouseRepository.ListResponsesAsync(cancellationToken));
    }
}

public sealed class WarehouseStockService(IWarehouseStockRepository warehouseStockRepository) : IWarehouseStockService
{
    public async Task<Result<List<WarehouseStockResponse>>> ListAsync(Guid warehouseId, CancellationToken cancellationToken)
    {
        if (warehouseId == Guid.Empty) return Result.Fail<List<WarehouseStockResponse>>("Warehouse id is required.");
        return Result.Success(await warehouseStockRepository.ListResponsesAsync(warehouseId, cancellationToken));
    }
}

public sealed class AuditLogService(IAuditLogRepository auditLogRepository) : IAuditLogService
{
    public async Task<Result<List<AuditLogResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await auditLogRepository.ListResponsesAsync(cancellationToken));
    }
}

public sealed class FinanceService(IFinanceQueryRepository financeQueryRepository) : IFinanceService
{
    public async Task<Result<List<WarehouseCommittedStockValueResponse>>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await financeQueryRepository.ListWarehouseCommittedStockValuesAsync(cancellationToken));
    }
}
