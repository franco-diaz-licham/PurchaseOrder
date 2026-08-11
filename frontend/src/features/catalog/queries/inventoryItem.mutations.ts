import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toChangeInventoryItemStandardCostRequestDto } from '../mappers/catalog.mapper';
import { changeInventoryItemStandardCost } from '../services/inventoryItem.services';
import type { ChangeInventoryItemStandardCostCommand } from '../types/catalog.types';
import { catalogKeys } from './catalog.queries';

export const useChangeInventoryItemStandardCostMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (command: ChangeInventoryItemStandardCostCommand) =>
      changeInventoryItemStandardCost(command.inventoryItemId, toChangeInventoryItemStandardCostRequestDto(command)),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: catalogKeys.inventoryItems });
    }
  });
};
