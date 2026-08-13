import type { ReservationStatus } from './reservation.types';

export type ReservationResponseDto = {
  stockReservationId: string;
  purchaseOrderLineId: string;
  warehouseId: string;
  inventoryItemId: string;
  quantityReserved: number;
  unitCostSnapshot: number;
  status: ReservationStatus;
  reservedBy: string;
  reservedAt: string;
};

export type CreateReservationRequestDto = {
  purchaseOrderLineId: string;
  warehouseId: string;
  quantity: number;
  user: string;
};

export type ReleaseReservationRequestDto = {
  quantity: number;
  user: string;
};
