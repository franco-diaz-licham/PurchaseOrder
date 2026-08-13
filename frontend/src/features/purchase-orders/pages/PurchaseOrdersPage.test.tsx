import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, test, vi } from 'vitest';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';
import type { PurchaseOrderModel, PurchaseOrderSummaryModel } from '../types/purchaseOrder.types';
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

const warehouses: WarehouseModel[] = [
  {
    id: 'warehouse-nsw',
    code: 'NSW',
    name: 'New South Wales',
    displayName: 'NSW - New South Wales'
  },
  {
    id: 'warehouse-qld',
    code: 'QLD',
    name: 'Queensland',
    displayName: 'QLD - Queensland'
  }
];

const purchaseOrders: PurchaseOrderSummaryModel[] = [
  {
    id: 'purchase-order-1',
    number: 'PO-1021',
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
    number: 'PO-1022',
    warehouseId: 'warehouse-qld',
    status: 'Pending',
    lineCount: 1,
    quantityOrdered: 5,
    quantityReserved: 0,
    quantityRemaining: 5,
    subtotalAmount: 50,
    gstAmount: 5,
    totalAmount: 55
  }
];

const createdPurchaseOrder: PurchaseOrderModel = {
  id: 'purchase-order-new',
  number: 'PO-1023',
  warehouseId: 'warehouse-nsw',
  status: 'Pending',
  subtotalAmount: 0,
  gstAmount: 0,
  totalAmount: 0,
  lines: []
};

const setupQueries = () => {
  vi.mocked(useWarehousesQuery).mockReturnValue({
    data: warehouses,
    isError: false,
    isLoading: false
  } as ReturnType<typeof useWarehousesQuery>);

  vi.mocked(usePurchaseOrderSummariesQuery).mockReturnValue({
    data: purchaseOrders,
    isError: false,
    isLoading: false
  } as ReturnType<typeof usePurchaseOrderSummariesQuery>);

  vi.mocked(useSubmitPurchaseOrderMutation).mockReturnValue({
    isError: false,
    isPending: false,
    mutateAsync: vi.fn().mockResolvedValue(createdPurchaseOrder)
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
