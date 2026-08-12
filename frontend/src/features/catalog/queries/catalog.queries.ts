import { useQuery } from '@tanstack/react-query';
import { toInventoryItems, toWarehouses, toWarehouseStockList } from '../mappers/catalog.mapper';
import { listInventoryItems, listWarehouses, listWarehouseStock } from '../services/catalog.services';

export const catalogKeys = {
  warehouses: ['warehouses'] as const,
  inventoryItems: ['inventory-items'] as const,
  warehouseStock: (warehouseId: string) => ['warehouse-stock', warehouseId] as const
};

export const useWarehousesQuery = () =>
  useQuery({
    queryKey: catalogKeys.warehouses,
    queryFn: async () => toWarehouses(await listWarehouses())
  });

export const useInventoryItemsQuery = () =>
  useQuery({
    queryKey: catalogKeys.inventoryItems,
    queryFn: async () => toInventoryItems(await listInventoryItems())
  });

export const useWarehouseStockQuery = (warehouseId: string | undefined) =>
  useQuery({
    queryKey: catalogKeys.warehouseStock(warehouseId ?? ''),
    queryFn: async () => toWarehouseStockList(await listWarehouseStock(warehouseId ?? '')),
    enabled: Boolean(warehouseId)
  });
