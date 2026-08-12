export type Warehouse = {
  id: string;
  code: string;
  name: string;
  displayName: string;
};

export type InventoryItem = {
  id: string;
  sku: string;
  name: string;
  category: string;
  trackingMode: string;
  standardCost: number;
  displayName: string;
};

export type WarehouseStock = {
  warehouseId: string;
  inventoryItemId: string;
  onHandQuantity: number;
  activeReservedQuantity: number;
  availableQuantity: number;
};

export type ChangeInventoryItemStandardCostCommand = {
  inventoryItemId: string;
  standardCost: number;
  user: string;
};
