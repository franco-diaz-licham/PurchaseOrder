import { describe, expect, test } from 'vitest';
import type { PurchaseOrderResponseDto, PurchaseOrderSummaryResponseDto } from '../types/purchaseOrder.api.types';
import type { SubmitPurchaseOrderModel } from '../types/purchaseOrder.types';
import { toPurchaseOrder, toPurchaseOrderSummary, toSubmitPurchaseOrderRequestDto } from './purchaseOrder.mapper';

describe('purchase order mapper', () => {
  test('maps a purchase order response to a purchase order model', () => {
    // Arrange
    const dto: PurchaseOrderResponseDto = {
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderNumber: 'PO-1021',
      warehouseId: 'warehouse-1',
      status: 'Approved',
      subtotalAmount: 120,
      gstAmount: 12,
      totalAmount: 132,
      lines: [
        {
          purchaseOrderLineId: 'line-1',
          inventoryItemId: 'item-1',
          quantityOrdered: 10,
          quantityReserved: 4,
          quantityRemaining: 6,
          unitCost: 12,
          lineAmount: 120
        }
      ]
    };

    // Act
    const model = toPurchaseOrder(dto);

    // Assert
    expect(model).toEqual({
      id: 'purchase-order-1',
      number: 'PO-1021',
      warehouseId: 'warehouse-1',
      status: 'Approved',
      subtotalAmount: 120,
      gstAmount: 12,
      totalAmount: 132,
      lines: [
        {
          id: 'line-1',
          inventoryItemId: 'item-1',
          quantityOrdered: 10,
          quantityReserved: 4,
          quantityRemaining: 6,
          unitCost: 12,
          lineAmount: 120
        }
      ]
    });
  });

  test('maps a purchase order summary response to a purchase order summary model', () => {
    // Arrange
    const dto: PurchaseOrderSummaryResponseDto = {
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderNumber: 'PO-1021',
      warehouseId: 'warehouse-1',
      status: 'Pending',
      lineCount: 2,
      quantityOrdered: 15,
      quantityReserved: 5,
      quantityRemaining: 10,
      subtotalAmount: 200,
      gstAmount: 20,
      totalAmount: 220
    };

    // Act
    const model = toPurchaseOrderSummary(dto);

    // Assert
    expect(model).toEqual({
      id: 'purchase-order-1',
      number: 'PO-1021',
      warehouseId: 'warehouse-1',
      status: 'Pending',
      lineCount: 2,
      quantityOrdered: 15,
      quantityReserved: 5,
      quantityRemaining: 10,
      subtotalAmount: 200,
      gstAmount: 20,
      totalAmount: 220
    });
  });

  test('maps submit purchase order model to request dto', () => {
    // Arrange
    const model: SubmitPurchaseOrderModel = {
      warehouseId: 'warehouse-1',
      user: 'Franco Diaz',
      lines: [
        {
          inventoryItemId: 'item-1',
          quantityOrdered: 10
        }
      ]
    };

    // Act
    const dto = toSubmitPurchaseOrderRequestDto(model);

    // Assert
    expect(dto).toEqual({
      warehouseId: 'warehouse-1',
      user: 'Franco Diaz',
      lines: [
        {
          inventoryItemId: 'item-1',
          quantityOrdered: 10
        }
      ]
    });
  });
});
