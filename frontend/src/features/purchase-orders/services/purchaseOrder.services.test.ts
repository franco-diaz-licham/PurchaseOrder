import type { Mock } from 'vitest';
import { beforeEach, describe, expect, test, vi } from 'vitest';
import http from '@/lib/api/http';
import type { PurchaseOrderResponseDto, PurchaseOrderSummaryResponseDto } from '../types/purchaseOrder.api.types';
import { addPurchaseOrderLine, changePurchaseOrderStatus, getPurchaseOrder, listPurchaseOrderSummaries, removePurchaseOrderLine, submitPurchaseOrder, updatePurchaseOrderLine } from './purchaseOrder.services';

vi.mock('@/lib/api/http', () => ({
  default: {
    delete: vi.fn(),
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn()
  }
}));

const httpMock = http as unknown as {
  delete: Mock;
  get: Mock;
  post: Mock;
  put: Mock;
};

const purchaseOrderDto: PurchaseOrderResponseDto = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderNumber: 'PO-1021',
  warehouseId: 'warehouse-1',
  status: 'Pending',
  subtotalAmount: 100,
  gstAmount: 10,
  totalAmount: 110,
  lines: []
};

const purchaseOrderSummaryDto: PurchaseOrderSummaryResponseDto = {
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
};

describe('purchase order services', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('loads purchase order summaries from the summary endpoint', async () => {
    // Arrange
    httpMock.get.mockResolvedValue({ data: { data: [purchaseOrderSummaryDto] } });

    // Act
    const result = await listPurchaseOrderSummaries();

    // Assert
    expect(httpMock.get).toHaveBeenCalledWith('/purchase-order/summary');
    expect(result).toEqual([purchaseOrderSummaryDto]);
  });

  test('loads a purchase order aggregate by id', async () => {
    // Arrange
    httpMock.get.mockResolvedValue({ data: { data: purchaseOrderDto } });

    // Act
    const result = await getPurchaseOrder('purchase-order-1');

    // Assert
    expect(httpMock.get).toHaveBeenCalledWith('/purchase-order/purchase-order-1');
    expect(result).toEqual(purchaseOrderDto);
  });

  test('submits a purchase order request', async () => {
    // Arrange
    const request = {
      warehouseId: 'warehouse-1',
      user: 'Franco Diaz',
      lines: [{ inventoryItemId: 'item-1', quantityOrdered: 10 }]
    };
    httpMock.post.mockResolvedValue({ data: { data: purchaseOrderDto } });

    // Act
    const result = await submitPurchaseOrder(request);

    // Assert
    expect(httpMock.post).toHaveBeenCalledWith('/purchase-order', request);
    expect(result).toEqual(purchaseOrderDto);
  });

  test('adds a line to a purchase order', async () => {
    // Arrange
    const request = {
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Franco Diaz'
    };
    httpMock.post.mockResolvedValue({ data: { data: purchaseOrderDto } });

    // Act
    const result = await addPurchaseOrderLine('purchase-order-1', request);

    // Assert
    expect(httpMock.post).toHaveBeenCalledWith('/purchase-order/purchase-order-1/lines', request);
    expect(result).toEqual(purchaseOrderDto);
  });

  test('removes a line from a purchase order with the user in the request body', async () => {
    // Arrange
    const request = { user: 'Franco Diaz' };
    httpMock.delete.mockResolvedValue({ data: { data: purchaseOrderDto } });

    // Act
    const result = await removePurchaseOrderLine('purchase-order-1', 'line-1', request);

    // Assert
    expect(httpMock.delete).toHaveBeenCalledWith('/purchase-order/purchase-order-1/lines/line-1', {
      data: request
    });
    expect(result).toEqual(purchaseOrderDto);
  });

  test('updates a purchase order line quantity', async () => {
    // Arrange
    const request = {
      quantityOrdered: 20,
      user: 'Franco Diaz'
    };
    httpMock.put.mockResolvedValue({ data: { data: purchaseOrderDto } });

    // Act
    const result = await updatePurchaseOrderLine('purchase-order-1', 'line-1', request);

    // Assert
    expect(httpMock.put).toHaveBeenCalledWith('/purchase-order/purchase-order-1/lines/line-1', request);
    expect(result).toEqual(purchaseOrderDto);
  });

  test('changes a purchase order status', async () => {
    // Arrange
    const request = { user: 'Franco Diaz' };
    httpMock.put.mockResolvedValue({ data: { data: purchaseOrderDto } });

    // Act
    const result = await changePurchaseOrderStatus('purchase-order-1', 'approve', request);

    // Assert
    expect(httpMock.put).toHaveBeenCalledWith('/purchase-order/purchase-order-1/approve', request);
    expect(result).toEqual(purchaseOrderDto);
  });
});
