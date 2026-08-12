import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { catalogKeys } from '@/features/catalog/queries/catalog.queries';
import { toAddPurchaseOrderLineRequestDto, toChangePurchaseOrderStatusRequestDto, toPurchaseOrder, toPurchaseOrderSummaries, toRemovePurchaseOrderLineRequestDto, toSubmitPurchaseOrderRequestDto } from '../mappers/purchaseOrder.mapper';
import { addPurchaseOrderLine, changePurchaseOrderStatus, getPurchaseOrder, listPurchaseOrderSummaries, removePurchaseOrderLine, submitPurchaseOrder } from '../services/purchaseOrder.services';
import type { AddPurchaseOrderLineModel, ChangePurchaseOrderStatusModel, RemovePurchaseOrderLineModel, SubmitPurchaseOrderModel } from '../types/purchaseOrder.types';

export const purchaseOrderKeys = {
  all: ['purchase-orders'] as const,
  summaries: ['purchase-orders', 'summary'] as const,
  detail: (purchaseOrderId: string) => ['purchase-orders', 'detail', purchaseOrderId] as const
};

export const usePurchaseOrderSummariesQuery = () =>
  useQuery({
    queryKey: purchaseOrderKeys.summaries,
    queryFn: async () => toPurchaseOrderSummaries(await listPurchaseOrderSummaries())
  });

export const usePurchaseOrderQuery = (purchaseOrderId: string | undefined) =>
  useQuery({
    queryKey: purchaseOrderKeys.detail(purchaseOrderId ?? ''),
    queryFn: async () => toPurchaseOrder(await getPurchaseOrder(purchaseOrderId ?? '')),
    enabled: Boolean(purchaseOrderId)
  });

export const useSubmitPurchaseOrderMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: SubmitPurchaseOrderModel) => toPurchaseOrder(await submitPurchaseOrder(toSubmitPurchaseOrderRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
    }
  });
};

export const useAddPurchaseOrderLineMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: AddPurchaseOrderLineModel) => toPurchaseOrder(await addPurchaseOrderLine(command.purchaseOrderId, toAddPurchaseOrderLineRequestDto(command))),
    onSuccess: async (purchaseOrder) => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.detail(purchaseOrder.id) });
    }
  });
};

export const useRemovePurchaseOrderLineMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: RemovePurchaseOrderLineModel) => toPurchaseOrder(await removePurchaseOrderLine(command.purchaseOrderId, command.purchaseOrderLineId, toRemovePurchaseOrderLineRequestDto(command))),
    onSuccess: async (purchaseOrder) => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.detail(purchaseOrder.id) });
      await queryClient.invalidateQueries({ queryKey: ['reservations'] });
      await queryClient.invalidateQueries({ queryKey: catalogKeys.warehouseStock(purchaseOrder.warehouseId) });
      await queryClient.invalidateQueries({ queryKey: ['finance'] });
      await queryClient.invalidateQueries({ queryKey: ['audit-log'] });
    }
  });
};

export const usePurchaseOrderStatusMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: ChangePurchaseOrderStatusModel) => toPurchaseOrder(await changePurchaseOrderStatus(command.purchaseOrderId, command.status, toChangePurchaseOrderStatusRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
    }
  });
};
