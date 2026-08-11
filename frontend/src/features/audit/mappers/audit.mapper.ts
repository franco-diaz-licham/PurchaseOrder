import type { AuditLogEntryDto } from '../types/audit.api.types';
import type { AuditLogEntry } from '../types/audit.types';

export const toAuditLogEntry = (dto: AuditLogEntryDto): AuditLogEntry => ({
  id: dto.auditLogEntryId,
  action: dto.action,
  inventoryItemId: dto.inventoryItemId,
  warehouseId: dto.warehouseId,
  purchaseOrderLineId: dto.purchaseOrderLineId,
  stockReservationId: dto.stockReservationId,
  quantity: dto.quantity,
  resultingAvailableQuantity: dto.resultingAvailableQuantity,
  user: dto.user,
  timestamp: new Date(dto.timestamp)
});

export const toAuditLogEntries = (dtos: AuditLogEntryDto[]): AuditLogEntry[] => dtos.map(toAuditLogEntry);
