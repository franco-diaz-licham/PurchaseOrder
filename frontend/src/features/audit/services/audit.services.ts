import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { AuditLogEntryDto } from '../types/audit.api.types';

export const listAuditLog = async (warehouseId?: string) => {
  const response = await http.get<ApiResponse<AuditLogEntryDto[]>>('/AuditLog', {
    params: warehouseId ? { warehouseId } : undefined
  });
  return response.data.data;
};
