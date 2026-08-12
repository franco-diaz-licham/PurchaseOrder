import type { AuditLogEntryDto } from '../types/audit.api.types';
import type { AuditLogEntryModel } from '../types/audit.types';

export const toAuditLogEntry = (dto: AuditLogEntryDto): AuditLogEntryModel => ({
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

export const toAuditLogEntries = (dtos: AuditLogEntryDto[]): AuditLogEntryModel[] => dtos.map(toAuditLogEntry);
