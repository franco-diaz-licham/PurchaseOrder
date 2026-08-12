import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { WarehouseCommittedValueResponseDto } from '../types/finance.api.types';

export const listWarehouseCommittedValues = async () => {
  const response = await http.get<ApiResponse<WarehouseCommittedValueResponseDto[]>>('/finance/warehouse-committed-values');
  return response.data.data;
};
