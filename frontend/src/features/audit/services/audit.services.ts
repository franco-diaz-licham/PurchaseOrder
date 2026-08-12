import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { AuditLogEntryResponseDto } from '../types/audit.api.types';

export const listAuditLog = async () => {
  const response = await http.get<ApiResponse<AuditLogEntryResponseDto[]>>('/audit-log');
  return response.data.data;
};
