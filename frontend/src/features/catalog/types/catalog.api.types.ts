import type { InventoryItemCategory, InventoryTrackingMode } from './catalog.types';

export type WarehouseResponseDto = {
  warehouseId: string;
  code: string;
  name: string;
};

export type InventoryItemResponseDto = {
  inventoryItemId: string;
  sku: string;
  name: string;
  category: InventoryItemCategory;
  trackingMode: InventoryTrackingMode;
  standardCost: number;
};

export type WarehouseStockResponseDto = {
  warehouseId: string;
  inventoryItemId: string;
  onHandQuantity: number;
  activeReservedQuantity: number;
  availableQuantity: number;
};

export type ChangeInventoryItemStandardCostRequestDto = {
  standardCost: number;
  user: string;
};
