import { describe, expect, test } from 'vitest';
import {
  mockAddPurchaseOrderLineModel,
  mockChangePurchaseOrderStatusModel,
  mockPurchaseOrderSummaryResponseDto,
  mockPurchaseOrderSummaryResponseDtos,
  mockPurchaseOrderWithLinesResponseDto,
  mockRemovePurchaseOrderLineModel,
  mockSubmitPurchaseOrderModel,
  mockUpdatePurchaseOrderLineModel
} from '@/testUtils/mockData';
import {
  toAddPurchaseOrderLineRequestDto,
  toChangePurchaseOrderStatusRequestDto,
  toPurchaseOrder,
  toPurchaseOrderSummaries,
  toPurchaseOrderSummary,
  toRemovePurchaseOrderLineRequestDto,
  toSubmitPurchaseOrderRequestDto,
  toUpdatePurchaseOrderLineRequestDto
} from './purchaseOrder.mapper';

describe('purchase order mapper', () => {
  test('maps a purchase order response to a purchase order model', () => {
    // Act
    const model = toPurchaseOrder(mockPurchaseOrderWithLinesResponseDto);

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
    // Act
    const model = toPurchaseOrderSummary(mockPurchaseOrderSummaryResponseDto);

    // Assert
    expect(model).toEqual({
      id: 'purchase-order-1',
      number: 'PO-1021',
      warehouseId: 'warehouse-1',
      status: 'Pending',
      lineCount: 1,
      quantityOrdered: 10,
      quantityReserved: 0,
      quantityRemaining: 10,
      subtotalAmount: 100,
      gstAmount: 10,
      totalAmount: 110
    });
  });

  test('maps purchase order summary responses to purchase order summary models', () => {
    // Act
    const models = toPurchaseOrderSummaries(mockPurchaseOrderSummaryResponseDtos);

    // Assert
    expect(models).toHaveLength(2);
    expect(models[0].number).toBe('PO-1021');
    expect(models[1].number).toBe('PO-1022');
  });

  test('maps submit purchase order model to request dto', () => {
    // Act
    const dto = toSubmitPurchaseOrderRequestDto(mockSubmitPurchaseOrderModel);

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

  test('maps add line model to request dto', () => {
    // Act
    const dto = toAddPurchaseOrderLineRequestDto(mockAddPurchaseOrderLineModel);

    // Assert
    expect(dto).toEqual({
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Franco Diaz'
    });
  });

  test('maps remove line model to request dto', () => {
    // Act
    const dto = toRemovePurchaseOrderLineRequestDto(mockRemovePurchaseOrderLineModel);

    // Assert
    expect(dto).toEqual({
      user: 'Franco Diaz'
    });
  });

  test('maps update line model to request dto', () => {
    // Act
    const dto = toUpdatePurchaseOrderLineRequestDto(mockUpdatePurchaseOrderLineModel);

    // Assert
    expect(dto).toEqual({
      quantityOrdered: 20,
      user: 'Franco Diaz'
    });
  });

  test('maps status change model to request dto', () => {
    // Act
    const dto = toChangePurchaseOrderStatusRequestDto(mockChangePurchaseOrderStatusModel);

    // Assert
    expect(dto).toEqual({
      user: 'Franco Diaz'
    });
  });
});
