using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;

namespace PurchaseOrderApp.Infrastructure.Persistence.Repositories;

public sealed class InventoryItemRepository(DatabaseContext db) : IInventoryItemRepository
{
    public Task<InventoryItem?> GetAsync(InventoryItemId inventoryItemId, CancellationToken cancellationToken)
    {
        return db.InventoryItems.SingleOrDefaultAsync(item => item.Id == inventoryItemId, cancellationToken);
    }

    public async Task AddAsync(InventoryItem inventoryItem, CancellationToken cancellationToken)
    {
        await db.InventoryItems.AddAsync(inventoryItem, cancellationToken);
    }
}
