import type { Mock } from 'vitest';
import { beforeEach, describe, expect, test, vi } from 'vitest';
import http from '@/lib/api/http';
import {
  mockAddPurchaseOrderLineRequestDto,
  mockChangePurchaseOrderStatusRequestDto,
  mockPurchaseOrderResponseDto,
  mockPurchaseOrderSummaryResponseDto,
  mockRemovePurchaseOrderLineRequestDto,
  mockSubmitPurchaseOrderRequestDto,
  mockUpdatePurchaseOrderLineRequestDto
} from '@/testUtils/mockData';
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

describe('purchase order services', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('loads purchase order summaries from the summary endpoint', async () => {
    // Arrange
    httpMock.get.mockResolvedValue({ data: { data: [mockPurchaseOrderSummaryResponseDto] } });

    // Act
    const result = await listPurchaseOrderSummaries();

    // Assert
    expect(httpMock.get).toHaveBeenCalledWith('/purchase-order/summary');
    expect(result).toEqual([mockPurchaseOrderSummaryResponseDto]);
  });

  test('loads a purchase order aggregate by id', async () => {
    // Arrange
    httpMock.get.mockResolvedValue({ data: { data: mockPurchaseOrderResponseDto } });

    // Act
    const result = await getPurchaseOrder('purchase-order-1');

    // Assert
    expect(httpMock.get).toHaveBeenCalledWith('/purchase-order/purchase-order-1');
    expect(result).toEqual(mockPurchaseOrderResponseDto);
  });

  test('submits a purchase order request', async () => {
    // Arrange
    httpMock.post.mockResolvedValue({ data: { data: mockPurchaseOrderResponseDto } });

    // Act
    const result = await submitPurchaseOrder(mockSubmitPurchaseOrderRequestDto);

    // Assert
    expect(httpMock.post).toHaveBeenCalledWith('/purchase-order', mockSubmitPurchaseOrderRequestDto);
    expect(result).toEqual(mockPurchaseOrderResponseDto);
  });

  test('adds a line to a purchase order', async () => {
    // Arrange
    httpMock.post.mockResolvedValue({ data: { data: mockPurchaseOrderResponseDto } });

    // Act
    const result = await addPurchaseOrderLine('purchase-order-1', mockAddPurchaseOrderLineRequestDto);

    // Assert
    expect(httpMock.post).toHaveBeenCalledWith('/purchase-order/purchase-order-1/lines', mockAddPurchaseOrderLineRequestDto);
    expect(result).toEqual(mockPurchaseOrderResponseDto);
  });

  test('removes a line from a purchase order with the user in the request body', async () => {
    // Arrange
    httpMock.delete.mockResolvedValue({ data: { data: mockPurchaseOrderResponseDto } });

    // Act
    const result = await removePurchaseOrderLine('purchase-order-1', 'line-1', mockRemovePurchaseOrderLineRequestDto);

    // Assert
    expect(httpMock.delete).toHaveBeenCalledWith('/purchase-order/purchase-order-1/lines/line-1', {
      data: mockRemovePurchaseOrderLineRequestDto
    });
    expect(result).toEqual(mockPurchaseOrderResponseDto);
  });

  test('updates a purchase order line quantity', async () => {
    // Arrange
    httpMock.put.mockResolvedValue({ data: { data: mockPurchaseOrderResponseDto } });

    // Act
    const result = await updatePurchaseOrderLine('purchase-order-1', 'line-1', mockUpdatePurchaseOrderLineRequestDto);

    // Assert
    expect(httpMock.put).toHaveBeenCalledWith('/purchase-order/purchase-order-1/lines/line-1', mockUpdatePurchaseOrderLineRequestDto);
    expect(result).toEqual(mockPurchaseOrderResponseDto);
  });

  test('changes a purchase order status', async () => {
    // Arrange
    httpMock.put.mockResolvedValue({ data: { data: mockPurchaseOrderResponseDto } });

    // Act
    const result = await changePurchaseOrderStatus('purchase-order-1', 'approve', mockChangePurchaseOrderStatusRequestDto);

    // Assert
    expect(httpMock.put).toHaveBeenCalledWith('/purchase-order/purchase-order-1/approve', mockChangePurchaseOrderStatusRequestDto);
    expect(result).toEqual(mockPurchaseOrderResponseDto);
  });
});
