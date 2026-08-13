import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import { mockPurchaseOrderSummaries, mockWarehouses } from '@/testUtils/mockData';
import { PurchaseOrderSummaryTable } from './PurchaseOrderSummaryTable';

describe('PurchaseOrderSummaryTable', () => {
  test('renders purchase order summaries with warehouse display names', () => {
    // Arrange / Act
    render(<PurchaseOrderSummaryTable isError={false} isLoading={false} purchaseOrders={mockPurchaseOrderSummaries} warehouses={mockWarehouses} onOpenPurchaseOrder={vi.fn()} />);

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
    render(<PurchaseOrderSummaryTable isError={false} isLoading={false} purchaseOrders={mockPurchaseOrderSummaries} warehouses={mockWarehouses} onOpenPurchaseOrder={onOpenPurchaseOrder} />);

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
    render(<PurchaseOrderSummaryTable isError={false} isLoading={false} purchaseOrders={mockPurchaseOrderSummaries} warehouses={mockWarehouses} onOpenPurchaseOrder={onOpenPurchaseOrder} />);

    // Act
    await user.tab();
    await user.keyboard('{Enter}');

    // Assert
    expect(onOpenPurchaseOrder).toHaveBeenCalledWith('purchase-order-1');
  });

  test('shows empty and error states', () => {
    // Arrange / Act
    render(<PurchaseOrderSummaryTable isError isLoading={false} purchaseOrders={[]} warehouses={mockWarehouses} onOpenPurchaseOrder={vi.fn()} />);

    // Assert
    expect(screen.getByText('Purchase orders could not be loaded.')).toBeInTheDocument();
    expect(screen.getByText('No purchase orders found.')).toBeInTheDocument();
  });
});
