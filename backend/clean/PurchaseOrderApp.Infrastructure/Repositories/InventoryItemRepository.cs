using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Repositories;

public sealed class InventoryItemRepository(DatabaseContext db) : IInventoryItemRepository
{
    public Task<List<InventoryItemResponse>> ListResponsesAsync(CancellationToken cancellationToken)
    {
        return db.InventoryItems
            .AsNoTracking()
            .OrderBy(item => item.Sku)
            .Select(item => new InventoryItemResponse(
                item.Id.Value,
                item.Sku,
                item.Name,
                item.Category.ToString(),
                item.TrackingMode.ToString(),
                item.StandardCost.Value))
            .ToListAsync(cancellationToken);
    }

    public Task<InventoryItem?> GetAsync(InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        return db.InventoryItems.SingleOrDefaultAsync(item => item.Id == inventoryItemId, cancellationToken);
    }

}
