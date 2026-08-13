import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { InventoryItemModel } from '@/features/catalog/types/catalog.types';
import { AddPurchaseOrderLineDialog } from './AddPurchaseOrderLineDialog';

const inventoryItems: InventoryItemModel[] = [
  {
    id: 'item-1',
    sku: 'BEAM-6M',
    name: '6m Spreader Beam',
    category: 'Hardware',
    trackingMode: 'Unit',
    standardCost: 1320,
    displayName: 'BEAM-6M - 6m Spreader Beam [Unit]'
  }
];

describe('AddPurchaseOrderLineDialog', () => {
  test('submits selected inventory item, quantity, and user', async () => {
    // Arrange
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(<AddPurchaseOrderLineDialog inventoryItems={inventoryItems} isSaving={false} onCancel={vi.fn()} onSubmit={onSubmit} />);

    // Act
    await user.selectOptions(screen.getByLabelText('Inventory item'), 'item-1');
    await user.clear(screen.getByLabelText('Quantity'));
    await user.type(screen.getByLabelText('Quantity'), '12.5');
    await user.clear(screen.getByLabelText('User'));
    await user.type(screen.getByLabelText('User'), 'Tara Smith');
    await user.click(screen.getByRole('button', { name: 'Add line' }));

    // Assert
    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(onSubmit).toHaveBeenCalledWith({
      inventoryItemId: 'item-1',
      quantityOrdered: 12.5,
      user: 'Tara Smith'
    });
  });

  test('disables submit when no inventory items are available', () => {
    // Arrange / Act
    render(<AddPurchaseOrderLineDialog inventoryItems={[]} isSaving={false} onCancel={vi.fn()} onSubmit={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Add line' })).toBeDisabled();
  });

  test('shows loading state while saving', () => {
    // Arrange / Act
    render(<AddPurchaseOrderLineDialog inventoryItems={inventoryItems} isSaving onCancel={vi.fn()} onSubmit={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Add line' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Add line' }).querySelector('.animate-spin')).toBeInTheDocument();
  });

  test('cancels the dialog', async () => {
    // Arrange
    const user = userEvent.setup();
    const onCancel = vi.fn();

    render(<AddPurchaseOrderLineDialog inventoryItems={inventoryItems} isSaving={false} onCancel={onCancel} onSubmit={vi.fn()} />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    // Assert
    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
