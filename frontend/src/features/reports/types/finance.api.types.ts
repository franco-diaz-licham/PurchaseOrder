import type { InventoryTrackingMode } from '@/features/catalog/types/catalog.types';

export type WarehouseCommittedValueResponseDto = {
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  reservedQuantity: number;
  reservationCount: number;
  committedValue: number;
  reservations: WarehouseCommittedReservationResponseDto[];
};

export type WarehouseCommittedReservationResponseDto = {
  stockReservationId: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  purchaseOrderLineId: string;
  inventoryItemId: string;
  sku: string;
  itemName: string;
  trackingMode: InventoryTrackingMode;
  quantityReserved: number;
  unitCostSnapshot: number;
  committedValue: number;
};
