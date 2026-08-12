import { describe, expect, test } from 'vitest';
import type { ReleaseReservationModel, ReservationModel } from '../types/reservation.types';
import { toReleaseReservationRequestDto, toReservation } from './reservation.mapper';

describe('reservation mapper', () => {
  test('maps reservation response to reservation model', () => {
    // Act
    const model = toReservation({
      stockReservationId: 'reservation-1',
      purchaseOrderLineId: 'line-1',
      warehouseId: 'warehouse-1',
      inventoryItemId: 'item-1',
      quantityReserved: 10.5,
      unitCostSnapshot: 4.25,
      status: 'Active',
      reservedBy: 'Franco Diaz',
      reservedAt: '2026-08-12T10:15:00Z'
    });

    // Assert
    expect(model).toEqual({
      id: 'reservation-1',
      purchaseOrderLineId: 'line-1',
      warehouseId: 'warehouse-1',
      inventoryItemId: 'item-1',
      quantityReserved: 10.5,
      unitCostSnapshot: 4.25,
      status: 'Active',
      reservedBy: 'Franco Diaz',
      reservedAt: new Date('2026-08-12T10:15:00Z')
    } satisfies ReservationModel);
  });

  test('maps release reservation model to request dto', () => {
    // Arrange
    const model: ReleaseReservationModel = {
      stockReservationId: 'reservation-1',
      quantity: 10.5,
      user: 'Franco Diaz'
    };

    // Act
    const dto = toReleaseReservationRequestDto(model);

    // Assert
    expect(dto).toEqual({
      quantity: 10.5,
      user: 'Franco Diaz'
    });
  });
});
