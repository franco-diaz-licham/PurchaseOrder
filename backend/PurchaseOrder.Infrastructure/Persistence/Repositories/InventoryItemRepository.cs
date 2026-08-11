using Microsoft.EntityFrameworkCore;
using PurchaseOrder.Application.Ports;
using PurchaseOrder.Domain.Entities;
using PurchaseOrder.Domain.ValueObjects;

namespace PurchaseOrder.Infrastructure.Persistence.Repositories;

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
