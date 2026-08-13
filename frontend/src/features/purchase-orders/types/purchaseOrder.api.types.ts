import type { PurchaseOrderStatus } from './purchaseOrder.types';

export type PurchaseOrderLineResponseDto = {
  purchaseOrderLineId: string;
  inventoryItemId: string;
  quantityOrdered: number;
  quantityReserved: number;
  quantityRemaining: number;
  unitCost: number;
  lineAmount: number;
};

export type PurchaseOrderResponseDto = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  warehouseId: string;
  status: PurchaseOrderStatus;
  subtotalAmount: number;
  gstAmount: number;
  totalAmount: number;
  lines: PurchaseOrderLineResponseDto[];
};

export type PurchaseOrderSummaryResponseDto = {
  purchaseOrderId: string;
  purchaseOrderNumber: string;
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

export type UpdatePurchaseOrderLineRequestDto = {
  quantityOrdered: number;
  user: string;
};

export type ChangePurchaseOrderStatusRequestDto = {
  user: string;
};
