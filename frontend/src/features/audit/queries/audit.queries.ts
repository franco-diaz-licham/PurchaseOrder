import { useQuery } from '@tanstack/react-query';
import { toAuditLogEntries } from '../mappers/audit.mapper';
import { listAuditLog } from '../services/audit.services';

export const useAuditLogQuery = (warehouseId?: string) =>
  useQuery({
    queryKey: ['audit-log', warehouseId ?? 'all'],
    queryFn: async () => toAuditLogEntries(await listAuditLog(warehouseId))
  });
