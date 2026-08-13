export type AuditAction = 'Reserve' | 'Release';

export type AuditLogEntryModel = {
  id: string;
  action: AuditAction;
  inventoryItemId: string;
  warehouseId: string;
  purchaseOrderLineId: string;
  stockReservationId: string;
  quantity: number;
  resultingAvailableQuantity: number;
  user: string;
  timestamp: Date;
};
