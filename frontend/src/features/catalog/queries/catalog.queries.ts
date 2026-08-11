import { useQuery } from '@tanstack/react-query';
import { toInventoryItems, toWarehouses } from '../mappers/catalog.mapper';
import { listInventoryItems, listWarehouses } from '../services/catalog.services';

export const catalogKeys = {
  warehouses: ['warehouses'] as const,
  inventoryItems: ['inventory-items'] as const
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
