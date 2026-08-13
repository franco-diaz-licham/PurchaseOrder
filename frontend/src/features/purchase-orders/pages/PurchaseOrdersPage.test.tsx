import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';
import { mockCreatedPurchaseOrder, mockPurchaseOrderSummaries, mockWarehouses } from '@/testUtils/mockData';
import { usePurchaseOrderListStore } from '../stores/purchaseOrderList.store';
import { PurchaseOrdersPage } from './PurchaseOrdersPage';

vi.mock('react-router-dom', () => {
  const navigate = vi.fn();

  return {
    navigate,
    useNavigate: () => navigate
  };
});

vi.mock('@/features/catalog/queries/catalog.queries', () => ({
  useWarehousesQuery: vi.fn()
}));

vi.mock('../queries/purchaseOrder.queries', () => ({
  usePurchaseOrderSummariesQuery: vi.fn(),
  useSubmitPurchaseOrderMutation: vi.fn()
}));

import { useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import * as router from 'react-router-dom';
import { usePurchaseOrderSummariesQuery, useSubmitPurchaseOrderMutation } from '../queries/purchaseOrder.queries';

const navigate = (router as unknown as { navigate: ReturnType<typeof vi.fn> }).navigate;

const setupQueries = () => {
  vi.mocked(useWarehousesQuery).mockReturnValue({
    data: mockWarehouses,
    isError: false,
    isLoading: false
  } as ReturnType<typeof useWarehousesQuery>);

  vi.mocked(usePurchaseOrderSummariesQuery).mockReturnValue({
    data: mockPurchaseOrderSummaries,
    isError: false,
    isLoading: false
  } as ReturnType<typeof usePurchaseOrderSummariesQuery>);

  vi.mocked(useSubmitPurchaseOrderMutation).mockReturnValue({
    isError: false,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue(mockCreatedPurchaseOrder)
  } as unknown as ReturnType<typeof useSubmitPurchaseOrderMutation>);
};

describe('PurchaseOrdersPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    usePurchaseOrderListStore.setState({
      selectedWarehouseId: '',
      showReadyToReserveOnly: false
    });
    setupQueries();
  });

  test('renders purchase order summaries from the query data', () => {
    // Arrange / Act
    render(<PurchaseOrdersPage />);

    // Assert
    expect(screen.getByRole('heading', { name: 'Purchase Orders' })).toBeInTheDocument();
    expect(screen.getByText('PO-1021')).toBeInTheDocument();
    expect(screen.getByText('PO-1022')).toBeInTheDocument();
  });

  test('navigates to the selected purchase order detail page', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<PurchaseOrdersPage />);

    // Act
    await user.click(screen.getByText('PO-1021'));

    // Assert
    expect(navigate).toHaveBeenCalledWith('/purchase-orders/purchase-order-1');
  });

  test('filters purchase orders from the page actions', async () => {
    // Arrange
    const user = userEvent.setup();
    render(<PurchaseOrdersPage />);

    // Act
    await user.selectOptions(screen.getByRole('combobox'), 'warehouse-qld');

    // Assert
    expect(screen.queryByText('PO-1021')).not.toBeInTheDocument();
    expect(screen.getByText('PO-1022')).toBeInTheDocument();
  });

  test('creates a purchase order and navigates to the created aggregate page', async () => {
    // Arrange
    const user = userEvent.setup();
    const submitMutation = useSubmitPurchaseOrderMutation();
    render(<PurchaseOrdersPage />);

    // Act
    await user.click(screen.getByRole('button', { name: 'New PO' }));
    await user.selectOptions(screen.getByLabelText('Warehouse'), 'warehouse-nsw');
    await user.click(screen.getByRole('button', { name: 'Create' }));

    // Assert
    await waitFor(() => {
      expect(submitMutation.mutateAsync).toHaveBeenCalledWith({
        warehouseId: 'warehouse-nsw',
        user: 'Franco Diaz',
        lines: []
      });
    });
    expect(navigate).toHaveBeenCalledWith('/purchase-orders/purchase-order-new');
  });

  test('shows the page loader while purchase orders are loading', () => {
    // Arrange
    vi.mocked(usePurchaseOrderSummariesQuery).mockReturnValue({
      data: undefined,
      isError: false,
      isLoading: true
    } as ReturnType<typeof usePurchaseOrderSummariesQuery>);

    // Act
    render(<PurchaseOrdersPage />);

    // Assert
    expect(screen.getByRole('status', { name: 'Loading data...' })).toBeInTheDocument();
  });
});
