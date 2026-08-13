export type PurchaseOrderStatus = 'Pending' | 'Approved' | 'Closed' | 'Cancelled';

export type PurchaseOrderLineModel = {
  id: string;
  inventoryItemId: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
  unitCost: number;
  lineAmount: number;
};

export type PurchaseOrderModel = {
  id: string;
  number: string;
  warehouseId: string;
  status: PurchaseOrderStatus;
  subtotalAmount: number;
  gstAmount: number;
  totalAmount: number;
  lines: PurchaseOrderLineModel[];
};

export type PurchaseOrderSummaryModel = {
  id: string;
  number: string;
  warehouseId: string;
  status: PurchaseOrderStatus;
  lineCount: number;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
  subtotalAmount: number;
  gstAmount: number;
  totalAmount: number;
};

export type SubmitPurchaseOrderModel = {
  warehouseId: string;
  user: string;
  lines: Array<{
    inventoryItemId: string;
    quantityOrdered: number;
  }>;
};

export type AddPurchaseOrderLineModel = {
  purchaseOrderId: string;
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

export type RemovePurchaseOrderLineModel = {
  purchaseOrderId: string;
  purchaseOrderLineId: string;
  user: string;
};

export type UpdatePurchaseOrderLineModel = {
  purchaseOrderId: string;
  purchaseOrderLineId: string;
  quantityOrdered: number;
  user: string;
};

export type ChangePurchaseOrderStatusModel = {
  purchaseOrderId: string;
  status: 'approve' | 'close' | 'cancel';
  user: string;
};
