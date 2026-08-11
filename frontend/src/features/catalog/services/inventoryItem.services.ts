import type { ApiMessage } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { ChangeInventoryItemStandardCostRequestDto } from '../types/catalog.api.types';

export const changeInventoryItemStandardCost = async (inventoryItemId: string, request: ChangeInventoryItemStandardCostRequestDto) => {
  const response = await http.put<ApiMessage>(`/inventory-item/${inventoryItemId}/standard-cost`, request);
  return response.data;
};
