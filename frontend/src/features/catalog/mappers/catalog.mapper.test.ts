import { describe, expect, it } from 'vitest';
import { toInventoryItem } from './catalog.mapper';

describe('catalog mapper', () => {
  it('maps an inventory item dto to the UI model', () => {
    const item = toInventoryItem({
      inventoryItemId: 'inventory-item-1',
      sku: 'WIRE-ROPE',
      name: 'Hoist Wire Rope',
      category: 'BulkGoods',
      trackingMode: 'Weight',
      standardCost: 6.8
    });

    expect(item).toEqual({
      id: 'inventory-item-1',
      sku: 'WIRE-ROPE',
      name: 'Hoist Wire Rope',
      category: 'BulkGoods',
      trackingMode: 'Weight',
      standardCost: 6.8,
      displayName: 'WIRE-ROPE - Hoist Wire Rope [Weight]'
    });
  });
});
