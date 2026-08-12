import type { WarehouseCommittedReservationDto, WarehouseCommittedValueDto } from '../types/finance.api.types';
import type { WarehouseCommittedReservationModel, WarehouseCommittedValueModel } from '../types/finance.types';

export const toWarehouseCommittedReservation = (dto: WarehouseCommittedReservationDto): WarehouseCommittedReservationModel => ({
  stockReservationId: dto.stockReservationId,
  purchaseOrderId: dto.purchaseOrderId,
  purchaseOrderNumber: dto.purchaseOrderNumber,
  purchaseOrderLineId: dto.purchaseOrderLineId,
  inventoryItemId: dto.inventoryItemId,
  sku: dto.sku,
  itemName: dto.itemName,
  trackingMode: dto.trackingMode,
  itemDisplayName: `${dto.sku} - ${dto.itemName} [${dto.trackingMode}]`,
  quantityReserved: dto.quantityReserved,
  unitCostSnapshot: dto.unitCostSnapshot,
  committedValue: dto.committedValue
});

export const toWarehouseCommittedValue = (dto: WarehouseCommittedValueDto): WarehouseCommittedValueModel => ({
  warehouseId: dto.warehouseId,
  warehouseCode: dto.warehouseCode,
  warehouseName: dto.warehouseName,
  warehouseDisplayName: `${dto.warehouseCode} - ${dto.warehouseName}`,
  reservedQuantity: dto.reservedQuantity,
  reservationCount: dto.reservationCount,
  committedValue: dto.committedValue,
  reservations: dto.reservations.map(toWarehouseCommittedReservation)
});

export const toWarehouseCommittedValues = (dtos: WarehouseCommittedValueDto[]): WarehouseCommittedValueModel[] => dtos.map(toWarehouseCommittedValue);
