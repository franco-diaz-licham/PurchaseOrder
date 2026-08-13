import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import { mockClosedPurchaseOrder, mockPendingPurchaseOrder, mockPurchaseOrder } from '@/testUtils/mockData';
import { PurchaseOrderHeaderCard } from './PurchaseOrderHeaderCard';

describe('PurchaseOrderHeaderCard', () => {
  test('allows a pending purchase order to be approved, closed, or cancelled', async () => {
    // Arrange
    const user = userEvent.setup();
    const onApprove = vi.fn();
    const onClose = vi.fn();
    const onCancel = vi.fn();

    render(<PurchaseOrderHeaderCard isChangingStatus={false} purchaseOrder={mockPendingPurchaseOrder} warehouseDisplayName="NSW - New South Wales" onApprove={onApprove} onCancel={onCancel} onClose={onClose} />);

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
    render(<PurchaseOrderHeaderCard isChangingStatus={false} purchaseOrder={mockPurchaseOrder} warehouseDisplayName="NSW - New South Wales" onApprove={vi.fn()} onCancel={vi.fn()} onClose={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Approve' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Close' })).toBeEnabled();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeEnabled();
  });

  test('shows a spinner on the lifecycle action currently being saved', () => {
    // Arrange / Act
    render(<PurchaseOrderHeaderCard changingStatusAction="close" isChangingStatus purchaseOrder={mockPurchaseOrder} warehouseDisplayName="NSW - New South Wales" onApprove={vi.fn()} onCancel={vi.fn()} onClose={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Close' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Close' }).querySelector('.animate-spin')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Cancel' }).querySelector('.animate-spin')).not.toBeInTheDocument();
  });

  test('hides lifecycle actions when the purchase order is closed', () => {
    // Arrange / Act
    render(<PurchaseOrderHeaderCard isChangingStatus={false} purchaseOrder={mockClosedPurchaseOrder} warehouseDisplayName="NSW - New South Wales" onApprove={vi.fn()} onCancel={vi.fn()} onClose={vi.fn()} />);

    // Assert
    expect(screen.queryByRole('button', { name: 'Approve' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Close' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Cancel' })).not.toBeInTheDocument();
  });
});
