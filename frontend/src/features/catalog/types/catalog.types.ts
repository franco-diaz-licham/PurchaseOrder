export type WarehouseModel = {
  id: string;
  code: string;
  name: string;
  displayName: string;
};

export type InventoryItemModel = {
  id: string;
  sku: string;
  name: string;
  category: string;
  trackingMode: string;
  standardCost: number;
  displayName: string;
};

export type WarehouseStockModel = {
  warehouseId: string;
  inventoryItemId: string;
  onHandQuantity: number;
  activeReservedQuantity: number;
  availableQuantity: number;
};

export type ChangeInventoryItemStandardCostModel = {
  inventoryItemId: string;
  standardCost: number;
  user: string;
};
