import { describe, expect, test } from 'vitest';
import type { PurchaseOrderSummaryModel } from '../types/purchaseOrder.types';
import { filterPurchaseOrders } from './purchaseOrderFilters';

const purchaseOrders: PurchaseOrderSummaryModel[] = [
  {
    id: 'purchase-order-1',
    number: 'PO-1001',
    warehouseId: 'warehouse-nsw',
    status: 'Approved',
    lineCount: 1,
    quantityOrdered: 10,
    quantityReserved: 4,
    quantityRemaining: 6,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  },
  {
    id: 'purchase-order-2',
    number: 'PO-1002',
    warehouseId: 'warehouse-nsw',
    status: 'Approved',
    lineCount: 1,
    quantityOrdered: 10,
    quantityReserved: 10,
    quantityRemaining: 0,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  },
  {
    id: 'purchase-order-3',
    number: 'PO-1003',
    warehouseId: 'warehouse-qld',
    status: 'Pending',
    lineCount: 1,
    quantityOrdered: 10,
    quantityReserved: 0,
    quantityRemaining: 10,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  }
];

describe('purchase order filters', () => {
  test('filters purchase orders by warehouse', () => {
    // Arrange
    const filter = {
      warehouseId: 'warehouse-nsw',
      showReadyToReserveOnly: false
    };

    // Act
    const filtered = filterPurchaseOrders(purchaseOrders, filter);

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
    const filtered = filterPurchaseOrders(purchaseOrders, filter);

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
    const filtered = filterPurchaseOrders(purchaseOrders, filter);

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
    const filtered = filterPurchaseOrders(purchaseOrders, filter);

    // Assert
    expect(filtered).toEqual(purchaseOrders);
  });
});
