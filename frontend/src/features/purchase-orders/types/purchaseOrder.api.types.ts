export type PurchaseOrderLineDto = {
  purchaseOrderLineId: string;
  inventoryItemId: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
};

export type PurchaseOrderDto = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  status: string;
  lines: PurchaseOrderLineDto[];
};

export type ApprovedPurchaseOrderLineDto = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  purchaseOrderLineId: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  inventoryItemId: string;
  sku: string;
  itemName: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
};

export type SubmitPurchaseOrderRequestDto = {
  purchaseOrderNumber: string;
  warehouseId: string;
  user: string;
  lines: Array<{
    inventoryItemId: string;
    quantityOrdered: number;
  }>;
};

export type ChangePurchaseOrderStatusRequestDto = {
  user: string;
};
