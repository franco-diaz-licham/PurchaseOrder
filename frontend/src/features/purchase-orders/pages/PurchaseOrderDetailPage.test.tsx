import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';
import { mockInventoryItems, mockPendingPurchaseOrder, mockPurchaseOrder, mockReservations, mockWarehouses, mockWarehouseStock } from '@/testUtils/mockData';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';
import { PurchaseOrderDetailPage } from './PurchaseOrderDetailPage';

vi.mock('react-router-dom', () => {
  const routeParams = {
    purchaseOrderId: 'purchase-order-1'
  };

  return {
    routeParams,
    useParams: () => routeParams
  };
});

vi.mock('@/features/catalog/queries/catalog.queries', () => ({
  useInventoryItemsQuery: vi.fn(),
  useWarehousesQuery: vi.fn(),
  useWarehouseStockQuery: vi.fn()
}));

vi.mock('@/features/reservations/queries/reservation.queries', () => ({
  useCreateReservationMutation: vi.fn(),
  useReleaseReservationMutation: vi.fn(),
  useReservationsQuery: vi.fn()
}));

vi.mock('../queries/purchaseOrder.queries', () => ({
  useAddPurchaseOrderLineMutation: vi.fn(),
  usePurchaseOrderQuery: vi.fn(),
  usePurchaseOrderStatusMutation: vi.fn(),
  useRemovePurchaseOrderLineMutation: vi.fn(),
  useUpdatePurchaseOrderLineMutation: vi.fn()
}));

import { useInventoryItemsQuery, useWarehousesQuery, useWarehouseStockQuery } from '@/features/catalog/queries/catalog.queries';
import { useCreateReservationMutation, useReleaseReservationMutation, useReservationsQuery } from '@/features/reservations/queries/reservation.queries';
import * as router from 'react-router-dom';
import { useAddPurchaseOrderLineMutation, usePurchaseOrderQuery, usePurchaseOrderStatusMutation, useRemovePurchaseOrderLineMutation, useUpdatePurchaseOrderLineMutation } from '../queries/purchaseOrder.queries';

const routeParams = (router as unknown as { routeParams: { purchaseOrderId: string } }).routeParams;

const setupQueries = (order: PurchaseOrderModel | null = mockPurchaseOrder) => {
  vi.mocked(usePurchaseOrderQuery).mockReturnValue({
    data: order ?? undefined,
    error: null,
    isError: false,
    isLoading: false
  } as ReturnType<typeof usePurchaseOrderQuery>);

  vi.mocked(useWarehousesQuery).mockReturnValue({
    data: mockWarehouses,
    isLoading: false
  } as ReturnType<typeof useWarehousesQuery>);

  vi.mocked(useInventoryItemsQuery).mockReturnValue({
    data: mockInventoryItems,
    isLoading: false
  } as ReturnType<typeof useInventoryItemsQuery>);

  vi.mocked(useWarehouseStockQuery).mockReturnValue({
    data: mockWarehouseStock,
    isLoading: false
  } as ReturnType<typeof useWarehouseStockQuery>);

  vi.mocked(useReservationsQuery).mockReturnValue({
    data: mockReservations,
    isLoading: false
  } as ReturnType<typeof useReservationsQuery>);

  vi.mocked(usePurchaseOrderStatusMutation).mockReturnValue({
    error: null,
    isError: false,
    isPending: false,
    mutate: vi.fn()
  } as unknown as ReturnType<typeof usePurchaseOrderStatusMutation>);

  vi.mocked(useAddPurchaseOrderLineMutation).mockReturnValue({
    error: null,
    isError: false,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue(order ?? undefined)
  } as unknown as ReturnType<typeof useAddPurchaseOrderLineMutation>);

  vi.mocked(useRemovePurchaseOrderLineMutation).mockReturnValue({
    error: null,
    isError: false,
    isPending: false,
    mutate: vi.fn()
  } as unknown as ReturnType<typeof useRemovePurchaseOrderLineMutation>);

  vi.mocked(useUpdatePurchaseOrderLineMutation).mockReturnValue({
    error: null,
    isError: false,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue(order ?? undefined)
  } as unknown as ReturnType<typeof useUpdatePurchaseOrderLineMutation>);

  vi.mocked(useCreateReservationMutation).mockReturnValue({
    error: null,
    isError: false,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue(mockReservations[0])
  } as unknown as ReturnType<typeof useCreateReservationMutation>);

  vi.mocked(useReleaseReservationMutation).mockReturnValue({
    error: null,
    isError: false,
    isPending: false,
    mutate: vi.fn()
  } as unknown as ReturnType<typeof useReleaseReservationMutation>);
};

describe('PurchaseOrderDetailPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    routeParams.purchaseOrderId = 'purchase-order-1';
    setupQueries();
  });

  test('renders the purchase order aggregate details', () => {
    // Arrange / Act
    render(<PurchaseOrderDetailPage />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Purchase Order Details' })).toBeInTheDocument();
    expect(screen.getByText('PO-1021')).toBeInTheDocument();
    expect(screen.getByText('BEAM-6M - 6m Spreader Beam [Unit]')).toBeInTheDocument();
    expect(screen.getByText('$14,520.00')).toBeInTheDocument();
  });

  test('approves a pending purchase order from the page header', async () => {
    // Arrange
    const user = userEvent.setup();
    setupQueries(mockPendingPurchaseOrder);
    const statusMutation = usePurchaseOrderStatusMutation();

    render(<PurchaseOrderDetailPage />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Approve' }));

    // Assert
    expect(statusMutation.mutate).toHaveBeenCalledWith({
      purchaseOrderId: 'purchase-order-1',
      status: 'approve',
      user: 'Franco Diaz'
    });
  });

  test('adds a purchase order line from the page dialog', async () => {
    // Arrange
    const user = userEvent.setup();
    const addLineMutation = useAddPurchaseOrderLineMutation();

    render(<PurchaseOrderDetailPage />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Add item' }));
    await user.selectOptions(screen.getByLabelText('Inventory item'), 'item-2');
    await user.clear(screen.getByLabelText('Quantity (units)'));
    await user.type(screen.getByLabelText('Quantity (units)'), '3');
    await user.click(screen.getByRole('button', { name: 'Add line' }));

    // Assert
    await waitFor(() => {
      expect(addLineMutation.mutateAsync).toHaveBeenCalledWith({
        purchaseOrderId: 'purchase-order-1',
        inventoryItemId: 'item-2',
        quantityOrdered: 3,
        user: 'Franco Diaz'
      });
    });
  });

  test('updates a purchase order line from the page dialog', async () => {
    // Arrange
    const user = userEvent.setup();
    const updateLineMutation = useUpdatePurchaseOrderLineMutation();

    render(<PurchaseOrderDetailPage />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Edit line' }));
    await user.clear(screen.getByLabelText('Ordered quantity (units)'));
    await user.type(screen.getByLabelText('Ordered quantity (units)'), '12');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    // Assert
    await waitFor(() => {
      expect(updateLineMutation.mutateAsync).toHaveBeenCalledWith({
        purchaseOrderId: 'purchase-order-1',
        purchaseOrderLineId: 'line-1',
        quantityOrdered: 12,
        user: 'Franco Diaz'
      });
    });
  });

  test('reserves and releases stock from the page reservation dialog', async () => {
    // Arrange
    const user = userEvent.setup();
    const createReservationMutation = useCreateReservationMutation();
    const releaseReservationMutation = useReleaseReservationMutation();

    render(<PurchaseOrderDetailPage />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Manage reservations' }));
    await user.type(screen.getByLabelText('Quantity to reserve (units)'), '2');
    await user.click(screen.getByRole('button', { name: 'Reserve' }));
    await user.type(screen.getByLabelText('Quantity to release (units)'), '1');
    await user.click(screen.getByRole('button', { name: 'Release' }));

    // Assert
    await waitFor(() => {
      expect(createReservationMutation.mutateAsync).toHaveBeenCalledWith({
        purchaseOrderLineId: 'line-1',
        warehouseId: 'warehouse-nsw',
        quantity: 2,
        user: 'Franco Diaz'
      });
    });
    expect(releaseReservationMutation.mutate).toHaveBeenCalledWith({
      stockReservationId: 'reservation-1',
      quantity: 1,
      user: 'Franco Diaz'
    });
  });

  test('shows empty state when the purchase order was not found', () => {
    // Arrange
    setupQueries(null);

    // Act
    render(<PurchaseOrderDetailPage />);

    // Assert
    expect(screen.getByText('Purchase order was not found.')).toBeInTheDocument();
  });
});
