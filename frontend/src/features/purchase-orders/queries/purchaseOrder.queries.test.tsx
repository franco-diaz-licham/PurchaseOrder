import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { renderHook, waitFor } from '@testing-library/react';
import type { PropsWithChildren } from 'react';
import { beforeEach, describe, expect, test, vi } from 'vitest';
import { catalogKeys } from '@/features/catalog/queries/catalog.queries';
import * as purchaseOrderServices from '../services/purchaseOrder.services';
import type { PurchaseOrderResponseDto, PurchaseOrderSummaryResponseDto } from '../types/purchaseOrder.api.types';
import {
  purchaseOrderKeys,
  useAddPurchaseOrderLineMutation,
  usePurchaseOrderQuery,
  usePurchaseOrderStatusMutation,
  usePurchaseOrderSummariesQuery,
  useRemovePurchaseOrderLineMutation,
  useSubmitPurchaseOrderMutation,
  useUpdatePurchaseOrderLineMutation
} from './purchaseOrder.queries';

vi.mock('../services/purchaseOrder.services', () => ({
  addPurchaseOrderLine: vi.fn(),
  changePurchaseOrderStatus: vi.fn(),
  getPurchaseOrder: vi.fn(),
  listPurchaseOrderSummaries: vi.fn(),
  removePurchaseOrderLine: vi.fn(),
  submitPurchaseOrder: vi.fn(),
  updatePurchaseOrderLine: vi.fn()
}));

const serviceMock = vi.mocked(purchaseOrderServices);

const purchaseOrderDto: PurchaseOrderResponseDto = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderNumber: 'PO-1021',
  warehouseId: 'warehouse-1',
  status: 'Pending',
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

const purchaseOrderSummaryDto: PurchaseOrderSummaryResponseDto = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderNumber: 'PO-1021',
  warehouseId: 'warehouse-1',
  status: 'Pending',
  lineCount: 1,
  quantityOrdered: 10,
  quantityReserved: 4,
  quantityRemaining: 6,
  subtotalAmount: 120,
  gstAmount: 12,
  totalAmount: 132
};

const createQueryClient = () =>
  new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false }
    }
  });

const createWrapper =
  (queryClient: QueryClient) =>
  ({ children }: PropsWithChildren) => <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;

describe('purchase order queries', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  test('maps purchase order summaries from the service response', async () => {
    // Arrange
    const queryClient = createQueryClient();
    serviceMock.listPurchaseOrderSummaries.mockResolvedValue([purchaseOrderSummaryDto]);

    // Act
    const { result } = renderHook(() => usePurchaseOrderSummariesQuery(), {
      wrapper: createWrapper(queryClient)
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(result.current.data).toEqual([
      {
        id: 'purchase-order-1',
        number: 'PO-1021',
        warehouseId: 'warehouse-1',
        status: 'Pending',
        lineCount: 1,
        quantityOrdered: 10,
        quantityReserved: 4,
        quantityRemaining: 6,
        subtotalAmount: 120,
        gstAmount: 12,
        totalAmount: 132
      }
    ]);
  });

  test('does not load a purchase order aggregate without an id', () => {
    // Arrange
    const queryClient = createQueryClient();

    // Act
    renderHook(() => usePurchaseOrderQuery(undefined), {
      wrapper: createWrapper(queryClient)
    });

    // Assert
    expect(serviceMock.getPurchaseOrder).not.toHaveBeenCalled();
  });

  test('maps a purchase order aggregate from the service response', async () => {
    // Arrange
    const queryClient = createQueryClient();
    serviceMock.getPurchaseOrder.mockResolvedValue(purchaseOrderDto);

    // Act
    const { result } = renderHook(() => usePurchaseOrderQuery('purchase-order-1'), {
      wrapper: createWrapper(queryClient)
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    // Assert
    expect(serviceMock.getPurchaseOrder).toHaveBeenCalledWith('purchase-order-1');
    expect(result.current.data?.lines[0]).toEqual({
      id: 'line-1',
      inventoryItemId: 'item-1',
      quantityOrdered: 10,
      quantityReserved: 4,
      quantityRemaining: 6,
      unitCost: 12,
      lineAmount: 120
    });
  });

  test('submit mutation maps the command and invalidates purchase order reads', async () => {
    // Arrange
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries').mockResolvedValue();
    serviceMock.submitPurchaseOrder.mockResolvedValue(purchaseOrderDto);

    const { result } = renderHook(() => useSubmitPurchaseOrderMutation(), {
      wrapper: createWrapper(queryClient)
    });

    // Act
    const purchaseOrder = await result.current.mutateAsync({
      warehouseId: 'warehouse-1',
      user: 'Franco Diaz',
      lines: [{ inventoryItemId: 'item-1', quantityOrdered: 10 }]
    });

    // Assert
    expect(serviceMock.submitPurchaseOrder).toHaveBeenCalledWith({
      warehouseId: 'warehouse-1',
      user: 'Franco Diaz',
      lines: [{ inventoryItemId: 'item-1', quantityOrdered: 10 }]
    });
    expect(purchaseOrder.id).toBe('purchase-order-1');
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.all });
  });

  test('add line mutation maps the command and invalidates the aggregate read', async () => {
    // Arrange
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries').mockResolvedValue();
    serviceMock.addPurchaseOrderLine.mockResolvedValue(purchaseOrderDto);

    const { result } = renderHook(() => useAddPurchaseOrderLineMutation(), {
      wrapper: createWrapper(queryClient)
    });

    // Act
    const purchaseOrder = await result.current.mutateAsync({
      purchaseOrderId: 'purchase-order-1',
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Franco Diaz'
    });

    // Assert
    expect(serviceMock.addPurchaseOrderLine).toHaveBeenCalledWith('purchase-order-1', {
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Franco Diaz'
    });
    expect(purchaseOrder.id).toBe('purchase-order-1');
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.all });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.detail('purchase-order-1') });
  });

  test('remove line mutation invalidates purchase order and stock-related reads', async () => {
    // Arrange
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries').mockResolvedValue();
    serviceMock.removePurchaseOrderLine.mockResolvedValue(purchaseOrderDto);

    const { result } = renderHook(() => useRemovePurchaseOrderLineMutation(), {
      wrapper: createWrapper(queryClient)
    });

    // Act
    await result.current.mutateAsync({
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderLineId: 'line-1',
      user: 'Franco Diaz'
    });

    // Assert
    expect(serviceMock.removePurchaseOrderLine).toHaveBeenCalledWith('purchase-order-1', 'line-1', {
      user: 'Franco Diaz'
    });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.all });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.detail('purchase-order-1') });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['reservations'] });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: catalogKeys.warehouseStock('warehouse-1') });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['finance'] });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: ['audit-log'] });
  });

  test('update line mutation maps the command and invalidates the aggregate read', async () => {
    // Arrange
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries').mockResolvedValue();
    serviceMock.updatePurchaseOrderLine.mockResolvedValue(purchaseOrderDto);

    const { result } = renderHook(() => useUpdatePurchaseOrderLineMutation(), {
      wrapper: createWrapper(queryClient)
    });

    // Act
    await result.current.mutateAsync({
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderLineId: 'line-1',
      quantityOrdered: 20,
      user: 'Franco Diaz'
    });

    // Assert
    expect(serviceMock.updatePurchaseOrderLine).toHaveBeenCalledWith('purchase-order-1', 'line-1', {
      quantityOrdered: 20,
      user: 'Franco Diaz'
    });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.all });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.detail('purchase-order-1') });
  });

  test('status mutation maps the command and invalidates purchase order reads', async () => {
    // Arrange
    const queryClient = createQueryClient();
    const invalidateQueries = vi.spyOn(queryClient, 'invalidateQueries').mockResolvedValue();
    serviceMock.changePurchaseOrderStatus.mockResolvedValue(purchaseOrderDto);

    const { result } = renderHook(() => usePurchaseOrderStatusMutation(), {
      wrapper: createWrapper(queryClient)
    });

    // Act
    await result.current.mutateAsync({
      purchaseOrderId: 'purchase-order-1',
      status: 'approve',
      user: 'Franco Diaz'
    });

    // Assert
    expect(serviceMock.changePurchaseOrderStatus).toHaveBeenCalledWith('purchase-order-1', 'approve', {
      user: 'Franco Diaz'
    });
    expect(invalidateQueries).toHaveBeenCalledWith({ queryKey: purchaseOrderKeys.all });
  });
});
