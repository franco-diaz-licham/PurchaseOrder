export type PurchaseOrderLine = {
  id: string;
  inventoryItemId: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
};

export type PurchaseOrder = {
  id: string;
  number: string;
  warehouseId: string;
  status: string;
  lines: PurchaseOrderLine[];
};

export type ApprovedPurchaseOrderLine = {
  id: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  warehouseCode: string;
  warehouseName: string;
  inventoryItemId: string;
  sku: string;
  itemName: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
  displayName: string;
};

export type SubmitPurchaseOrderCommand = {
  purchaseOrderNumber: string;
  warehouseId: string;
  user: string;
  lines: Array<{
    inventoryItemId: string;
    quantityOrdered: number;
  }>;
};

export type ChangePurchaseOrderStatusCommand = {
  purchaseOrderId: string;
  status: 'approve' | 'close' | 'cancel';
  user: string;
};
