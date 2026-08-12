import type { CreateReservationRequestDto, ReleaseReservationRequestDto, ReservationDto } from '../types/reservation.api.types';
import type { CreateReservationModel, ReleaseReservationModel, ReservationModel } from '../types/reservation.types';

export const toReservation = (dto: ReservationDto): ReservationModel => ({
  id: dto.stockReservationId,
  purchaseOrderLineId: dto.purchaseOrderLineId,
  warehouseId: dto.warehouseId,
  inventoryItemId: dto.inventoryItemId,
  quantityReserved: dto.quantityReserved,
  unitCostSnapshot: dto.unitCostSnapshot,
  status: dto.status,
  reservedBy: dto.reservedBy,
  reservedAt: new Date(dto.reservedAt)
});

export const toReservations = (dtos: ReservationDto[]): ReservationModel[] => dtos.map(toReservation);

export const toCreateReservationRequestDto = (command: CreateReservationModel): CreateReservationRequestDto => ({
  purchaseOrderLineId: command.purchaseOrderLineId,
  warehouseId: command.warehouseId,
  quantity: command.quantity,
  user: command.user
});

export const toReleaseReservationRequestDto = (command: ReleaseReservationModel): ReleaseReservationRequestDto => ({
  quantity: command.quantity,
  user: command.user
});
