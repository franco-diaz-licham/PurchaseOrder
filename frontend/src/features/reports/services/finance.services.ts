import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { WarehouseCommittedValueDto } from '../types/finance.api.types';

export const listWarehouseCommittedValues = async () => {
  const response = await http.get<ApiResponse<WarehouseCommittedValueDto[]>>('/Finance/warehouse-committed-values');
  return response.data.data;
};
