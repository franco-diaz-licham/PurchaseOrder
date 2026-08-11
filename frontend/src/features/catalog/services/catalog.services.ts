import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { InventoryItemDto, WarehouseDto } from '../types/catalog.api.types';

export const listWarehouses = async () => {
  const response = await http.get<ApiResponse<WarehouseDto[]>>('/Warehouse');
  return response.data.data;
};

export const listInventoryItems = async () => {
  const response = await http.get<ApiResponse<InventoryItemDto[]>>('/InventoryItem');
  return response.data.data;
};
