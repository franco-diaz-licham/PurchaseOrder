import { describe, expect, test } from 'vitest';
import { mockReleaseReservationModel, mockReservationResponseDto } from '@/testUtils/mockData';
import type { ReservationModel } from '../types/reservation.types';
import { toReleaseReservationRequestDto, toReservation } from './reservation.mapper';

describe('reservation mapper', () => {
  test('maps reservation response to reservation model', () => {
    // Act
    const model = toReservation(mockReservationResponseDto);

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
    // Act
    const dto = toReleaseReservationRequestDto(mockReleaseReservationModel);

    // Assert
    expect(dto).toEqual({
      quantity: 10.5,
      user: 'Franco Diaz'
    });
  });
});
