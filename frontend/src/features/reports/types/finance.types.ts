export type WarehouseCommittedValueModel = {
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  warehouseDisplayName: string;
  reservedQuantity: number;
  reservationCount: number;
  committedValue: number;
  reservations: WarehouseCommittedReservationModel[];
};

export type WarehouseCommittedReservationModel = {
  stockReservationId: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  purchaseOrderLineId: string;
  inventoryItemId: string;
  sku: string;
  itemName: string;
  trackingMode: string;
  itemDisplayName: string;
  quantityReserved: number;
  unitCostSnapshot: number;
  committedValue: number;
};
