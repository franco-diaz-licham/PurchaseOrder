import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import type { PurchaseOrderLineModel } from '../types/purchaseOrder.types';
import { ManageReservationsDialog } from './ManageReservationsDialog';

const line: PurchaseOrderLineModel = {
  id: 'line-1',
  inventoryItemId: 'item-1',
  quantityOrdered: 20,
  quantityReserved: 5,
  quantityRemaining: 15,
  unitCost: 10,
  lineAmount: 200
};

const reservation: ReservationModel = {
  id: 'reservation-1',
  purchaseOrderLineId: 'line-1',
  warehouseId: 'warehouse-1',
  inventoryItemId: 'item-1',
  quantityReserved: 5,
  unitCostSnapshot: 10,
  status: 'Active',
  reservedBy: 'Franco Diaz',
  reservedAt: new Date('2026-08-12T10:15:00Z')
};

describe('ManageReservationsDialog', () => {
  test('submits a valid reservation quantity', async () => {
    // Arrange
    const user = userEvent.setup();
    const onReserve = vi.fn().mockResolvedValue(undefined);

    render(
      <ManageReservationsDialog
        availableQuantity={15}
        isReleasing={false}
        isReserving={false}
        itemName="BEAM-6M - 6m Spreader Beam [Unit]"
        line={line}
        maxReserveQuantity={15}
        reservations={[]}
        trackingMode="Unit"
        user="Franco Diaz"
        onCancel={vi.fn()}
        onRelease={vi.fn()}
        onReserve={onReserve}
        onUserChange={vi.fn()}
      />
    );

    // Act
    await user.type(screen.getByLabelText('Quantity to reserve'), '4');
    await user.click(screen.getByRole('button', { name: 'Reserve' }));

    // Assert
    expect(onReserve).toHaveBeenCalledTimes(1);
    expect(onReserve).toHaveBeenCalledWith(4, 'Franco Diaz');
  });

  test('keeps reserve disabled when the quantity is over the allowed amount', async () => {
    // Arrange
    const user = userEvent.setup();

    render(
      <ManageReservationsDialog
        availableQuantity={15}
        isReleasing={false}
        isReserving={false}
        itemName="BEAM-6M - 6m Spreader Beam [Unit]"
        line={line}
        maxReserveQuantity={15}
        reservations={[]}
        trackingMode="Unit"
        user="Franco Diaz"
        onCancel={vi.fn()}
        onRelease={vi.fn()}
        onReserve={vi.fn()}
        onUserChange={vi.fn()}
      />
    );

    // Act
    await user.type(screen.getByLabelText('Quantity to reserve'), '16');

    // Assert
    expect(screen.getByRole('button', { name: 'Reserve' })).toBeDisabled();
  });

  test('submits a release quantity for an active reservation', async () => {
    // Arrange
    const user = userEvent.setup();
    const onRelease = vi.fn();

    render(
      <ManageReservationsDialog
        availableQuantity={15}
        isReleasing={false}
        isReserving={false}
        itemName="BEAM-6M - 6m Spreader Beam [Unit]"
        line={line}
        maxReserveQuantity={15}
        reservations={[reservation]}
        trackingMode="Unit"
        user="Franco Diaz"
        onCancel={vi.fn()}
        onRelease={onRelease}
        onReserve={vi.fn()}
        onUserChange={vi.fn()}
      />
    );

    // Act
    await user.type(screen.getByLabelText('Quantity to release'), '2');
    await user.click(screen.getByRole('button', { name: 'Release' }));

    // Assert
    expect(onRelease).toHaveBeenCalledTimes(1);
    expect(onRelease).toHaveBeenCalledWith(reservation, 2);
  });
});
