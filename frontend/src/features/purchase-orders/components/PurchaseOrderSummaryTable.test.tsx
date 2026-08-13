import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';
import type { PurchaseOrderSummaryModel } from '../types/purchaseOrder.types';
import { PurchaseOrderSummaryTable } from './PurchaseOrderSummaryTable';

const warehouses: WarehouseModel[] = [
  {
    id: 'warehouse-nsw',
    code: 'NSW',
    name: 'New South Wales',
    displayName: 'NSW - New South Wales'
  }
];

const purchaseOrders: PurchaseOrderSummaryModel[] = [
  {
    id: 'purchase-order-1',
    number: 'PO-1021',
    warehouseId: 'warehouse-nsw',
    status: 'Approved',
    lineCount: 2,
    quantityOrdered: 20,
    quantityReserved: 5,
    quantityRemaining: 15,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  }
];

describe('PurchaseOrderSummaryTable', () => {
  test('renders purchase order summaries with warehouse display names', () => {
    // Arrange / Act
    render(<PurchaseOrderSummaryTable isError={false} isLoading={false} purchaseOrders={purchaseOrders} warehouses={warehouses} onOpenPurchaseOrder={vi.fn()} />);

    // Assert
    expect(screen.getByText('PO-1021')).toBeInTheDocument();
    expect(screen.getByText('NSW - New South Wales')).toBeInTheDocument();
    expect(screen.getByText('Approved')).toBeInTheDocument();
    expect(screen.getByText('$110.00')).toBeInTheDocument();
  });

  test('opens a purchase order when a row is clicked', async () => {
    // Arrange
    const user = userEvent.setup();
    const onOpenPurchaseOrder = vi.fn();
    render(<PurchaseOrderSummaryTable isError={false} isLoading={false} purchaseOrders={purchaseOrders} warehouses={warehouses} onOpenPurchaseOrder={onOpenPurchaseOrder} />);

    // Act
    await user.click(screen.getByText('PO-1021'));

    // Assert
    expect(onOpenPurchaseOrder).toHaveBeenCalledTimes(1);
    expect(onOpenPurchaseOrder).toHaveBeenCalledWith('purchase-order-1');
  });

  test('opens a purchase order when enter is pressed on a row', async () => {
    // Arrange
    const user = userEvent.setup();
    const onOpenPurchaseOrder = vi.fn();
    render(<PurchaseOrderSummaryTable isError={false} isLoading={false} purchaseOrders={purchaseOrders} warehouses={warehouses} onOpenPurchaseOrder={onOpenPurchaseOrder} />);

    // Act
    await user.tab();
    await user.keyboard('{Enter}');

    // Assert
    expect(onOpenPurchaseOrder).toHaveBeenCalledWith('purchase-order-1');
  });

  test('shows empty and error states', () => {
    // Arrange / Act
    render(<PurchaseOrderSummaryTable isError isLoading={false} purchaseOrders={[]} warehouses={warehouses} onOpenPurchaseOrder={vi.fn()} />);

    // Assert
    expect(screen.getByText('Purchase orders could not be loaded.')).toBeInTheDocument();
    expect(screen.getByText('No purchase orders found.')).toBeInTheDocument();
  });
});
