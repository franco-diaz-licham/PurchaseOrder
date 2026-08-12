import type { ChangeInventoryItemStandardCostRequestDto, InventoryItemDto, WarehouseDto, WarehouseStockDto } from '../types/catalog.api.types';
import type { ChangeInventoryItemStandardCostModel, InventoryItemModel, WarehouseModel, WarehouseStockModel } from '../types/catalog.types';

export const toWarehouse = (dto: WarehouseDto): WarehouseModel => ({
  id: dto.warehouseId,
  code: dto.code,
  name: dto.name,
  displayName: `${dto.code} - ${dto.name}`
});

export const toWarehouses = (dtos: WarehouseDto[]): WarehouseModel[] => dtos.map(toWarehouse);

export const toInventoryItem = (dto: InventoryItemDto): InventoryItemModel => ({
  id: dto.inventoryItemId,
  sku: dto.sku,
  name: dto.name,
  category: dto.category,
  trackingMode: dto.trackingMode,
  standardCost: dto.standardCost,
  displayName: `${dto.sku} - ${dto.name} [${dto.trackingMode}]`
});

export const toInventoryItems = (dtos: InventoryItemDto[]): InventoryItemModel[] => dtos.map(toInventoryItem);

export const toWarehouseStock = (dto: WarehouseStockDto): WarehouseStockModel => ({
  warehouseId: dto.warehouseId,
  inventoryItemId: dto.inventoryItemId,
  onHandQuantity: dto.onHandQuantity,
  activeReservedQuantity: dto.activeReservedQuantity,
  availableQuantity: dto.availableQuantity
});

export const toWarehouseStockList = (dtos: WarehouseStockDto[]): WarehouseStockModel[] => dtos.map(toWarehouseStock);

export const toChangeInventoryItemStandardCostRequestDto = (command: ChangeInventoryItemStandardCostModel): ChangeInventoryItemStandardCostRequestDto => ({
  standardCost: command.standardCost,
  user: command.user
});
