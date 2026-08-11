import type { CreateReservationRequestDto, ReleaseReservationRequestDto, ReservationDto } from '../types/reservation.api.types';
import type { CreateReservationCommand, ReleaseReservationCommand, Reservation } from '../types/reservation.types';

export const toReservation = (dto: ReservationDto): Reservation => ({
  id: dto.stockReservationId,
  purchaseOrderLineId: dto.purchaseOrderLineId,
  warehouseId: dto.warehouseId,
  inventoryItemId: dto.inventoryItemId,
  quantityReserved: dto.quantityReserved,
  unitCostSnapshot: dto.unitCostSnapshot,
  status: dto.status
});

export const toReservations = (dtos: ReservationDto[]): Reservation[] => dtos.map(toReservation);

export const toCreateReservationRequestDto = (command: CreateReservationCommand): CreateReservationRequestDto => ({
  purchaseOrderLineId: command.purchaseOrderLineId,
  warehouseId: command.warehouseId,
  quantity: command.quantity,
  user: command.user
});

export const toReleaseReservationRequestDto = (command: ReleaseReservationCommand): ReleaseReservationRequestDto => ({
  quantity: command.quantity,
  user: command.user
});
