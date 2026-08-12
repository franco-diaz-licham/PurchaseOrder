export type ReservationModel = {
  id: string;
  purchaseOrderLineId: string;
  warehouseId: string;
  inventoryItemId: string;
  quantityReserved: number;
  unitCostSnapshot: number;
  status: string;
  reservedBy: string;
  reservedAt: Date;
};

export type CreateReservationModel = {
  purchaseOrderLineId: string;
  warehouseId: string;
  quantity: number;
  user: string;
};

export type ReleaseReservationModel = {
  stockReservationId: string;
  quantity: number;
  user: string;
};
