export type AuditLogEntryModel = {
  id: string;
  action: string;
  inventoryItemId: string;
  warehouseId: string;
  purchaseOrderLineId: string;
  stockReservationId: string;
  quantity: number;
  resultingAvailableQuantity: number;
  user: string;
  timestamp: Date;
};
