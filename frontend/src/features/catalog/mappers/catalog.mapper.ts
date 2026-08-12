import type { ChangeInventoryItemStandardCostRequestDto, InventoryItemDto, WarehouseDto, WarehouseStockDto } from '../types/catalog.api.types';
import type { ChangeInventoryItemStandardCostCommand, InventoryItem, Warehouse, WarehouseStock } from '../types/catalog.types';

export const toWarehouse = (dto: WarehouseDto): Warehouse => ({
  id: dto.warehouseId,
  code: dto.code,
  name: dto.name,
  displayName: `${dto.code} - ${dto.name}`
});

export const toWarehouses = (dtos: WarehouseDto[]): Warehouse[] => dtos.map(toWarehouse);

export const toInventoryItem = (dto: InventoryItemDto): InventoryItem => ({
  id: dto.inventoryItemId,
  sku: dto.sku,
  name: dto.name,
  category: dto.category,
  trackingMode: dto.trackingMode,
  standardCost: dto.standardCost,
  displayName: `${dto.sku} - ${dto.name}`
});

export const toInventoryItems = (dtos: InventoryItemDto[]): InventoryItem[] => dtos.map(toInventoryItem);

export const toWarehouseStock = (dto: WarehouseStockDto): WarehouseStock => ({
  warehouseId: dto.warehouseId,
  inventoryItemId: dto.inventoryItemId,
  onHandQuantity: dto.onHandQuantity,
  activeReservedQuantity: dto.activeReservedQuantity,
  availableQuantity: dto.availableQuantity
});

export const toWarehouseStockList = (dtos: WarehouseStockDto[]): WarehouseStock[] => dtos.map(toWarehouseStock);

export const toChangeInventoryItemStandardCostRequestDto = (command: ChangeInventoryItemStandardCostCommand): ChangeInventoryItemStandardCostRequestDto => ({
  standardCost: command.standardCost,
  user: command.user
});
