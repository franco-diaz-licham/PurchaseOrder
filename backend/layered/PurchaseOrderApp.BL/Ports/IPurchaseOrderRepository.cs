using PurchaseOrderApp.BL.Models;

namespace PurchaseOrderApp.BL.Ports;

public interface IPurchaseOrderRepository
{
    Task<PurchaseOrder?> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken);
}
