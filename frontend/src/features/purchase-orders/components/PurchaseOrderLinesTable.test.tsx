import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { InventoryItemModel, WarehouseStockModel } from '@/features/catalog/types/catalog.types';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';
import { PurchaseOrderLinesTable } from './PurchaseOrderLinesTable';

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

const purchaseOrder: PurchaseOrderModel = {
  id: 'purchase-order-1',
  number: 'PO-1021',
  warehouseId: 'warehouse-nsw',
  status: 'Approved',
  subtotalAmount: 13200,
  gstAmount: 1320,
  totalAmount: 14520,
  lines: [
    {
      id: 'line-1',
      inventoryItemId: 'item-1',
      quantityOrdered: 10,
      quantityReserved: 4,
      quantityRemaining: 6,
      unitCost: 1320,
      lineAmount: 13200
    }
  ]
};

const reservation: ReservationModel = {
  id: 'reservation-1',
  purchaseOrderLineId: 'line-1',
  warehouseId: 'warehouse-nsw',
  inventoryItemId: 'item-1',
  quantityReserved: 4,
  unitCostSnapshot: 1320,
  status: 'Active',
  reservedBy: 'Franco Diaz',
  reservedAt: new Date('2026-08-12T10:15:00Z')
};

const stockByItemId = new Map<string, WarehouseStockModel>([
  [
    'item-1',
    {
      warehouseId: 'warehouse-nsw',
      inventoryItemId: 'item-1',
      onHandQuantity: 20,
      activeReservedQuantity: 4,
      availableQuantity: 16
    }
  ]
]);

const renderTable = (overrides: Partial<Parameters<typeof PurchaseOrderLinesTable>[0]> = {}) => {
  const props: Parameters<typeof PurchaseOrderLinesTable>[0] = {
    activeReservations: [reservation],
    availableItemCount: 1,
    canChangeLines: true,
    canReserveStock: true,
    inventoryItems,
    isAddingLine: false,
    isRemovingLine: false,
    purchaseOrder,
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
        ...purchaseOrder,
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
        ...purchaseOrder,
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
