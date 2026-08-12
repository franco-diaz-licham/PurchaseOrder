export type WarehouseCommittedValue = {
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  warehouseDisplayName: string;
  reservedQuantity: number;
  reservationCount: number;
  committedValue: number;
  reservations: WarehouseCommittedReservation[];
};

export type WarehouseCommittedReservation = {
  stockReservationId: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  purchaseOrderLineId: string;
  inventoryItemId: string;
  sku: string;
  itemName: string;
  itemDisplayName: string;
  quantityReserved: number;
  unitCostSnapshot: number;
  committedValue: number;
};
