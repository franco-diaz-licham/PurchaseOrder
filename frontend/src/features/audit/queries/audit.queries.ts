import { useQuery } from '@tanstack/react-query';
import { toAuditLogEntries } from '../mappers/audit.mapper';
import { listAuditLog } from '../services/audit.services';

export const useAuditLogQuery = () =>
  useQuery({
    queryKey: ['audit-log'],
    queryFn: async () => toAuditLogEntries(await listAuditLog())
  });
