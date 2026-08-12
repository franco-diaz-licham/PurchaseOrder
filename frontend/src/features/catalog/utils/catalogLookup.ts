import type { InventoryItemModel, WarehouseModel } from '../types/catalog.types';

export const findWarehouse = (warehouses: WarehouseModel[] | undefined, warehouseId: string) => warehouses?.find((warehouse) => warehouse.id === warehouseId);

export const findInventoryItem = (items: InventoryItemModel[] | undefined, inventoryItemId: string) => items?.find((item) => item.id === inventoryItemId);
