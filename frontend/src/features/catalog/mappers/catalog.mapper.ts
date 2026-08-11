import type { ChangeInventoryItemStandardCostRequestDto, InventoryItemDto, WarehouseDto } from '../types/catalog.api.types';
import type { ChangeInventoryItemStandardCostCommand, InventoryItem, Warehouse } from '../types/catalog.types';

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

export const toChangeInventoryItemStandardCostRequestDto = (command: ChangeInventoryItemStandardCostCommand): ChangeInventoryItemStandardCostRequestDto => ({
  standardCost: command.standardCost,
  user: command.user
});
