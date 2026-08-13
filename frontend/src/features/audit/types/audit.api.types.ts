import type { AuditAction } from './audit.types';

export type AuditLogEntryResponseDto = {
  auditLogEntryId: string;
  action: AuditAction;
  inventoryItemId: string;
  warehouseId: string;
  purchaseOrderLineId: string;
  stockReservationId: string;
  quantity: number;
  resultingAvailableQuantity: number;
  user: string;
  timestamp: string;
};
