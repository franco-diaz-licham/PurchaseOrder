using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken);

    Task<PurchaseOrder?> GetByLineIdAsync(Guid purchaseOrderLineId, CancellationToken cancellationToken);

    Task<PurchaseOrderResponse?> GetResponseAsync(Guid purchaseOrderId, CancellationToken cancellationToken);

    Task<List<PurchaseOrderSummaryResponse>> ListSummariesAsync(CancellationToken cancellationToken);

    Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken);
}
