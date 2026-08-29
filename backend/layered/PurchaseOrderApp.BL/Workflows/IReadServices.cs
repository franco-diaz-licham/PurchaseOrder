using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public interface IAuditLogService
{
    Task<Result<List<AuditLogResponse>>> ListAsync(CancellationToken cancellationToken);
}

public interface IFinanceService
{
    Task<Result<List<WarehouseCommittedStockValueResponse>>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken);
}

public interface IWarehouseService
{
    Task<Result<List<WarehouseResponse>>> ListAsync(CancellationToken cancellationToken);
}

public interface IWarehouseStockService
{
    Task<Result<List<WarehouseStockResponse>>> ListAsync(Guid warehouseId, CancellationToken cancellationToken);
}
