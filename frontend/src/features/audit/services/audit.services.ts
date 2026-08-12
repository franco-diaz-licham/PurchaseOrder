import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { AuditLogEntryDto } from '../types/audit.api.types';

export const listAuditLog = async () => {
  const response = await http.get<ApiResponse<AuditLogEntryDto[]>>('/audit-log');
  return response.data.data;
};
