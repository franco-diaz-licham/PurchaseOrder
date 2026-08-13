import { describe, expect, test } from 'vitest';
import { mockWarehouseCommittedValueResponseDto } from '@/testUtils/mockData';
import { toWarehouseCommittedValue } from './finance.mapper';

describe('finance mapper', () => {
  test('maps committed value response to the finance report model', () => {
    // Act
    const model = toWarehouseCommittedValue(mockWarehouseCommittedValueResponseDto);

    // Assert
    expect(model).toEqual({
      warehouseId: 'warehouse-nsw',
      warehouseCode: 'NSW',
      warehouseName: 'New South Wales',
      warehouseDisplayName: 'NSW - New South Wales',
      reservedQuantity: 25,
      reservationCount: 1,
      committedValue: 1200,
      reservations: [
        {
          stockReservationId: 'reservation-1',
          purchaseOrderId: 'purchase-order-1',
          purchaseOrderNumber: 'PO-1001',
          purchaseOrderLineId: 'line-1',
          inventoryItemId: 'item-1',
          sku: 'BEAM-6M',
          itemName: '6m Spreader Beam',
          trackingMode: 'Unit',
          itemDisplayName: 'BEAM-6M - 6m Spreader Beam [Unit]',
          quantityReserved: 25,
          unitCostSnapshot: 48,
          committedValue: 1200
        }
      ]
    });
  });
});
