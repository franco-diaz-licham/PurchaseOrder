export type Reservation = {
  id: string;
  purchaseOrderLineId: string;
  warehouseId: string;
  inventoryItemId: string;
  quantityReserved: number;
  unitCostSnapshot: number;
  status: string;
};

export type CreateReservationCommand = {
  purchaseOrderLineId: string;
  warehouseId: string;
  quantity: number;
  user: string;
};

export type ReleaseReservationCommand = {
  stockReservationId: string;
  quantity: number;
  user: string;
};
