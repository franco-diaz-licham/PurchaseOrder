import { useQuery } from '@tanstack/react-query';
import { toWarehouseCommittedValues } from '../mappers/finance.mapper';
import { listWarehouseCommittedValues } from '../services/finance.services';

export const useWarehouseCommittedValuesQuery = () =>
  useQuery({
    queryKey: ['finance', 'report'],
    queryFn: async () => toWarehouseCommittedValues(await listWarehouseCommittedValues())
  });
