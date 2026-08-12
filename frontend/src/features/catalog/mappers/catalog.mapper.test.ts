import { describe, expect, it } from 'vitest';
import { toInventoryItem } from './catalog.mapper';

describe('catalog mapper', () => {
  it('maps an inventory item dto to the UI model', () => {
    const item = toInventoryItem({
      inventoryItemId: 'inventory-item-1',
      sku: 'RICE-BULK',
      name: 'Bulk Rice',
      category: 'Food',
      trackingMode: 'Weight',
      standardCost: 1.75
    });

    expect(item).toEqual({
      id: 'inventory-item-1',
      sku: 'RICE-BULK',
      name: 'Bulk Rice',
      category: 'Food',
      trackingMode: 'Weight',
      standardCost: 1.75,
      displayName: 'RICE-BULK - Bulk Rice [Weight]'
    });
  });
});
