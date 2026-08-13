import { describe, expect, test } from 'vitest';
import type { PurchaseOrderResponseDto, PurchaseOrderSummaryResponseDto } from '../types/purchaseOrder.api.types';
import type { AddPurchaseOrderLineModel, ChangePurchaseOrderStatusModel, RemovePurchaseOrderLineModel, SubmitPurchaseOrderModel, UpdatePurchaseOrderLineModel } from '../types/purchaseOrder.types';
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

  test('maps purchase order summary responses to purchase order summary models', () => {
    // Arrange
    const dtos: PurchaseOrderSummaryResponseDto[] = [
      {
        purchaseOrderId: 'purchase-order-1',
        purchaseOrderNumber: 'PO-1021',
        warehouseId: 'warehouse-1',
        status: 'Pending',
        lineCount: 1,
        quantityOrdered: 10,
        quantityReserved: 0,
        quantityRemaining: 10,
        subtotalAmount: 100,
        gstAmount: 10,
        totalAmount: 110
      },
      {
        purchaseOrderId: 'purchase-order-2',
        purchaseOrderNumber: 'PO-1022',
        warehouseId: 'warehouse-2',
        status: 'Approved',
        lineCount: 2,
        quantityOrdered: 20,
        quantityReserved: 5,
        quantityRemaining: 15,
        subtotalAmount: 200,
        gstAmount: 20,
        totalAmount: 220
      }
    ];

    // Act
    const models = toPurchaseOrderSummaries(dtos);

    // Assert
    expect(models).toHaveLength(2);
    expect(models[0].number).toBe('PO-1021');
    expect(models[1].number).toBe('PO-1022');
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

  test('maps add line model to request dto', () => {
    // Arrange
    const model: AddPurchaseOrderLineModel = {
      purchaseOrderId: 'purchase-order-1',
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Franco Diaz'
    };

    // Act
    const dto = toAddPurchaseOrderLineRequestDto(model);

    // Assert
    expect(dto).toEqual({
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Franco Diaz'
    });
  });

  test('maps remove line model to request dto', () => {
    // Arrange
    const model: RemovePurchaseOrderLineModel = {
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderLineId: 'line-1',
      user: 'Franco Diaz'
    };

    // Act
    const dto = toRemovePurchaseOrderLineRequestDto(model);

    // Assert
    expect(dto).toEqual({
      user: 'Franco Diaz'
    });
  });

  test('maps update line model to request dto', () => {
    // Arrange
    const model: UpdatePurchaseOrderLineModel = {
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderLineId: 'line-1',
      quantityOrdered: 20,
      user: 'Franco Diaz'
    };

    // Act
    const dto = toUpdatePurchaseOrderLineRequestDto(model);

    // Assert
    expect(dto).toEqual({
      quantityOrdered: 20,
      user: 'Franco Diaz'
    });
  });

  test('maps status change model to request dto', () => {
    // Arrange
    const model: ChangePurchaseOrderStatusModel = {
      purchaseOrderId: 'purchase-order-1',
      status: 'approve',
      user: 'Franco Diaz'
    };

    // Act
    const dto = toChangePurchaseOrderStatusRequestDto(model);

    // Assert
    expect(dto).toEqual({
      user: 'Franco Diaz'
    });
  });
});
