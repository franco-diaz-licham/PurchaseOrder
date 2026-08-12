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

export type PurchaseOrderSummary = {
  id: string;
  number: string;
  warehouseId: string;
  status: string;
  lineCount: number;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
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
  warehouseId: string;
  user: string;
  lines: Array<{
    inventoryItemId: string;
    quantityOrdered: number;
  }>;
};

export type AddPurchaseOrderLineCommand = {
  purchaseOrderId: string;
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

export type ChangePurchaseOrderStatusCommand = {
  purchaseOrderId: string;
  status: 'approve' | 'close' | 'cancel';
  user: string;
};
