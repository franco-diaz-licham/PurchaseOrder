import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { WarehouseStockModel } from '@/features/catalog/types/catalog.types';
import { mockInventoryItems, mockPurchaseOrder, mockReservations, mockWarehouseStock } from '@/testUtils/mockData';
import { PurchaseOrderLinesTable } from './PurchaseOrderLinesTable';

const stockByItemId = new Map<string, WarehouseStockModel>(mockWarehouseStock.map((stock) => [stock.inventoryItemId, stock]));

const renderTable = (overrides: Partial<Parameters<typeof PurchaseOrderLinesTable>[0]> = {}) => {
  const props: Parameters<typeof PurchaseOrderLinesTable>[0] = {
    activeReservations: mockReservations,
    availableItemCount: 1,
    canChangeLines: true,
    canReserveStock: true,
    inventoryItems: mockInventoryItems,
    isAddingLine: false,
    isRemovingLine: false,
    purchaseOrder: mockPurchaseOrder,
    reservationUser: 'Franco Diaz',
    stockByItemId,
    onAddLine: vi.fn(),
    onEditLine: vi.fn(),
    onManageReservations: vi.fn(),
    onRemoveLine: vi.fn(),
    ...overrides
  };

  render(<PurchaseOrderLinesTable {...props} />);
  return props;
};

describe('PurchaseOrderLinesTable', () => {
  test('renders purchase order line and reservation information', () => {
    // Arrange / Act
    renderTable();

    // Assert
    expect(screen.getByText('BEAM-6M - 6m Spreader Beam [Unit]')).toBeInTheDocument();
    expect(screen.getByText('$1,320.00')).toBeInTheDocument();
    expect(screen.getByText('$13,200.00')).toBeInTheDocument();
    expect(screen.getByText('16')).toBeInTheDocument();
    expect(screen.getByText('1')).toBeInTheDocument();
  });

  test('raises add, edit, manage, and remove actions', async () => {
    // Arrange
    const user = userEvent.setup();
    const props = renderTable();

    // Act
    await user.click(screen.getByRole('button', { name: 'Add item' }));
    await user.click(screen.getByRole('button', { name: 'Edit line' }));
    await user.click(screen.getByRole('button', { name: 'Manage reservations' }));
    await user.click(screen.getByRole('button', { name: 'Remove line' }));

    // Assert
    expect(props.onAddLine).toHaveBeenCalledTimes(1);
    expect(props.onEditLine).toHaveBeenCalledWith('line-1');
    expect(props.onManageReservations).toHaveBeenCalledWith('line-1');
    expect(props.onRemoveLine).toHaveBeenCalledWith('line-1', 'Franco Diaz');
  });

  test('disables reservation management until the purchase order is approved', () => {
    // Arrange / Act
    renderTable({
      canReserveStock: false,
      purchaseOrder: {
        ...mockPurchaseOrder,
        status: 'Pending'
      }
    });

    // Assert
    expect(screen.getByText('Reservations are available after the purchase order is approved.')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: 'Manage reservations' })).toBeDisabled();
  });

  test('hides line and reservation actions when the purchase order is closed', () => {
    // Arrange / Act
    renderTable({
      canChangeLines: false,
      canReserveStock: false,
      purchaseOrder: {
        ...mockPurchaseOrder,
        status: 'Closed'
      }
    });

    // Assert
    expect(screen.getByText('This purchase order is read-only. Lines and reservations can no longer be changed.')).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Add item' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Edit line' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Manage reservations' })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: 'Remove line' })).not.toBeInTheDocument();
  });
});
