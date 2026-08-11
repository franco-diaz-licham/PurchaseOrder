export type WarehouseDto = {
  warehouseId: string;
  code: string;
  name: string;
};

export type InventoryItemDto = {
  inventoryItemId: string;
  sku: string;
  name: string;
  category: string;
  trackingMode: string;
  standardCost: number;
};

export type ChangeInventoryItemStandardCostRequestDto = {
  standardCost: number;
  user: string;
};
