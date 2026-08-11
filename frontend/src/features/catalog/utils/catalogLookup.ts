import type { InventoryItem, Warehouse } from '../types/catalog.types';

export const findWarehouse = (warehouses: Warehouse[] | undefined, warehouseId: string) => warehouses?.find((warehouse) => warehouse.id === warehouseId);

export const findInventoryItem = (items: InventoryItem[] | undefined, inventoryItemId: string) => items?.find((item) => item.id === inventoryItemId);
