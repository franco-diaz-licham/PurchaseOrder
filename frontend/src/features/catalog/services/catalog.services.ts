import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { InventoryItemDto, WarehouseDto, WarehouseStockDto } from '../types/catalog.api.types';

export const listWarehouses = async () => {
  const response = await http.get<ApiResponse<WarehouseDto[]>>('/warehouse');
  return response.data.data;
};

export const listInventoryItems = async () => {
  const response = await http.get<ApiResponse<InventoryItemDto[]>>('/inventory-item');
  return response.data.data;
};

export const listWarehouseStock = async (warehouseId: string) => {
  const response = await http.get<ApiResponse<WarehouseStockDto[]>>(`/warehouse-stock/warehouse/${warehouseId}`);
  return response.data.data;
};
