import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';
import { PurchaseOrderHeaderCard } from './PurchaseOrderHeaderCard';

const createPurchaseOrder = (status: PurchaseOrderModel['status']): PurchaseOrderModel => ({
  id: 'purchase-order-1',
  number: 'PO-1021',
  warehouseId: 'warehouse-nsw',
  status,
  subtotalAmount: 100,
  gstAmount: 10,
  totalAmount: 110,
  lines: []
});

describe('PurchaseOrderHeaderCard', () => {
  test('allows a pending purchase order to be approved, closed, or cancelled', async () => {
    // Arrange
    const user = userEvent.setup();
    const onApprove = vi.fn();
    const onClose = vi.fn();
    const onCancel = vi.fn();

    render(<PurchaseOrderHeaderCard isChangingStatus={false} purchaseOrder={createPurchaseOrder('Pending')} warehouseDisplayName="NSW - New South Wales" onApprove={onApprove} onCancel={onCancel} onClose={onClose} />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Approve' }));
    await user.click(screen.getByRole('button', { name: 'Close' }));
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    // Assert
    expect(onApprove).toHaveBeenCalledTimes(1);
    expect(onClose).toHaveBeenCalledTimes(1);
    expect(onCancel).toHaveBeenCalledTimes(1);
  });

  test('disables approve when the purchase order is already approved', () => {
    // Arrange / Act
    render(<PurchaseOrderHeaderCard isChangingStatus={false} purchaseOrder={createPurchaseOrder('Approved')} warehouseDisplayName="NSW - New South Wales" onApprove={vi.fn()} onCancel={vi.fn()} onClose={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Approve' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Close' })).toBeEnabled();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  });

  test('hides lifecycle actions when the purchase order is closed', () => {
    // Arrange / Act
    render(<PurchaseOrderHeaderCard isChangingStatus={false} purchaseOrder={createPurchaseOrder('Closed')} warehouseDisplayName="NSW - New South Wales" onApprove={vi.fn()} onCancel={vi.fn()} onClose={vi.fn()} />);

    // Assert
    expect(screen.queryByRole('button', { name: 'Approve' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Close' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Cancel' })).not.toBeInTheDocument();
  });
});
