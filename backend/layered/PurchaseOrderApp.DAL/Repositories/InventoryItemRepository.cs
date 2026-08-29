using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.DAL.Repositories;

public sealed class InventoryItemRepository(PurchaseOrderDbContext db) : IInventoryItemRepository
{
    public Task<InventoryItem?> GetAsync(Guid inventoryItemId, CancellationToken cancellationToken)
    {
        return db.InventoryItems.SingleOrDefaultAsync(item => item.Id == inventoryItemId, cancellationToken);
    }

    public Task<List<InventoryItemResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        return db.InventoryItems
            .AsNoTracking()
            .OrderBy(item => item.Sku)
            .Select(item => new InventoryItemResponse(
                item.Id,
                item.Sku,
                item.Name,
                item.Category.ToString(),
                item.TrackingMode.ToString(),
                item.StandardCost))
            .ToListAsync(cancellationToken);
    }
}
