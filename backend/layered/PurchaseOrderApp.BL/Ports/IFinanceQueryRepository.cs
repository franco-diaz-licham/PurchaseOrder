using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IFinanceQueryRepository
{
    Task<List<WarehouseCommittedStockValueResponse>> ListWarehouseCommittedStockValuesAsync(CancellationToken cancellationToken);
}
