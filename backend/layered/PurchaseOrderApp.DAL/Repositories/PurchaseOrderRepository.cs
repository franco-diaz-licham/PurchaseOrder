using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class PurchaseOrderRepository(PurchaseOrderDbContext dbContext) : IPurchaseOrderRepository
{
    public Task<PurchaseOrder?> GetAsync(Guid purchaseOrderId, CancellationToken cancellationToken)
    {
        return dbContext.PurchaseOrders
            .Include(purchaseOrder => purchaseOrder.Lines)
            .SingleOrDefaultAsync(purchaseOrder => purchaseOrder.Id == purchaseOrderId, cancellationToken);
    }
}
