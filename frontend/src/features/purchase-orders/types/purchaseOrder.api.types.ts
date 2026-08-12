export type PurchaseOrderLineDto = {
  purchaseOrderLineId: string;
  inventoryItemId: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
  unitCost: number;
  lineAmount: number;
};

export type PurchaseOrderDto = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  status: string;
  subtotalAmount: number;
  gstAmount: number;
  totalAmount: number;
  lines: PurchaseOrderLineDto[];
};

export type PurchaseOrderSummaryDto = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  status: string;
  lineCount: number;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
  subtotalAmount: number;
  gstAmount: number;
  totalAmount: number;
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
  warehouseId: string;
  user: string;
  lines: Array<{
    inventoryItemId: string;
    quantityOrdered: number;
  }>;
};

export type AddPurchaseOrderLineRequestDto = {
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

export type RemovePurchaseOrderLineRequestDto = {
  user: string;
};

export type ChangePurchaseOrderStatusRequestDto = {
  user: string;
};
