import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';
import { PurchaseOrderListActions } from './PurchaseOrderListActions';

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

describe('PurchaseOrderListActions', () => {
  test('raises filter and add actions', async () => {
    // Arrange
    const user = userEvent.setup();
    const onAdd = vi.fn();
    const onShowReadyToReserveOnlyChange = vi.fn();
    const onWarehouseFilterChange = vi.fn();

    render(<PurchaseOrderListActions showReadyToReserveOnly={false} warehouseFilter="" warehouses={warehouses} onAdd={onAdd} onShowReadyToReserveOnlyChange={onShowReadyToReserveOnlyChange} onWarehouseFilterChange={onWarehouseFilterChange} />);

    // Act
    await user.click(screen.getByLabelText('Ready to reserve'));
    await user.selectOptions(screen.getByRole('combobox'), 'warehouse-qld');
    await user.click(screen.getByRole('button', { name: 'New PO' }));

    // Assert
    expect(onShowReadyToReserveOnlyChange).toHaveBeenCalledWith(true);
    expect(onWarehouseFilterChange).toHaveBeenCalledWith('warehouse-qld');
    expect(onAdd).toHaveBeenCalledTimes(1);
  });

  test('renders the selected warehouse filter', () => {
    // Arrange / Act
    render(<PurchaseOrderListActions showReadyToReserveOnly warehouseFilter="warehouse-nsw" warehouses={warehouses} onAdd={vi.fn()} onShowReadyToReserveOnlyChange={vi.fn()} onWarehouseFilterChange={vi.fn()} />);

    // Assert
    expect(screen.getByLabelText('Ready to reserve')).toBeChecked();
    expect(screen.getByRole('combobox')).toHaveValue('warehouse-nsw');
  });
});
