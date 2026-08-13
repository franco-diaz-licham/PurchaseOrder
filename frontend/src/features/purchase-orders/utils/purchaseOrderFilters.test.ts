import { describe, expect, test } from 'vitest';
import { mockPurchaseOrderSummariesForFiltering } from '@/testUtils/mockData';
import { filterPurchaseOrders } from './purchaseOrderFilters';

describe('purchase order filters', () => {
  test('filters purchase orders by warehouse', () => {
    // Arrange
    const filter = {
      warehouseId: 'warehouse-nsw',
      showReadyToReserveOnly: false
    };

    // Act
    const filtered = filterPurchaseOrders(mockPurchaseOrderSummariesForFiltering, filter);

    // Assert
    expect(filtered.map((order) => order.id)).toEqual(['purchase-order-1', 'purchase-order-2']);
  });

  test('filters purchase orders ready to reserve', () => {
    // Arrange
    const filter = {
      warehouseId: '',
      showReadyToReserveOnly: true
    };

    // Act
    const filtered = filterPurchaseOrders(mockPurchaseOrderSummariesForFiltering, filter);

    // Assert
    expect(filtered.map((order) => order.id)).toEqual(['purchase-order-1']);
  });

  test('filters purchase orders by warehouse and ready to reserve state', () => {
    // Arrange
    const filter = {
      warehouseId: 'warehouse-nsw',
      showReadyToReserveOnly: true
    };

    // Act
    const filtered = filterPurchaseOrders(mockPurchaseOrderSummariesForFiltering, filter);

    // Assert
    expect(filtered.map((order) => order.id)).toEqual(['purchase-order-1']);
  });

  test('returns all purchase orders when no filter is selected', () => {
    // Arrange
    const filter = {
      warehouseId: '',
      showReadyToReserveOnly: false
    };

    // Act
    const filtered = filterPurchaseOrders(mockPurchaseOrderSummariesForFiltering, filter);

    // Assert
    expect(filtered).toEqual(mockPurchaseOrderSummariesForFiltering);
  });
});
