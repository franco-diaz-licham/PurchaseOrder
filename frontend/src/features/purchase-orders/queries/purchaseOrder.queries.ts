import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toAddPurchaseOrderLineRequestDto, toApprovedPurchaseOrderLines, toChangePurchaseOrderStatusRequestDto, toPurchaseOrder, toPurchaseOrders, toPurchaseOrderSummaries, toSubmitPurchaseOrderRequestDto } from '../mappers/purchaseOrder.mapper';
import { addPurchaseOrderLine, changePurchaseOrderStatus, getPurchaseOrder, listApprovedPurchaseOrderLines, listPurchaseOrders, listPurchaseOrderSummaries, submitPurchaseOrder } from '../services/purchaseOrder.services';
import type { AddPurchaseOrderLineCommand, ChangePurchaseOrderStatusCommand, SubmitPurchaseOrderCommand } from '../types/purchaseOrder.types';

export const purchaseOrderKeys = {
  all: ['purchase-orders'] as const,
  list: (warehouseId?: string) => ['purchase-orders', warehouseId ?? 'all'] as const,
  summaries: (warehouseId?: string) => ['purchase-orders', 'summary', warehouseId ?? 'all'] as const,
  detail: (purchaseOrderId: string) => ['purchase-orders', 'detail', purchaseOrderId] as const,
  approvedLines: (warehouseId: string) => ['approved-lines', warehouseId] as const
};

export const usePurchaseOrdersQuery = (warehouseId?: string) =>
  useQuery({
    queryKey: purchaseOrderKeys.list(warehouseId),
    queryFn: async () => toPurchaseOrders(await listPurchaseOrders(warehouseId))
  });

export const usePurchaseOrderSummariesQuery = (warehouseId?: string) =>
  useQuery({
    queryKey: purchaseOrderKeys.summaries(warehouseId),
    queryFn: async () => toPurchaseOrderSummaries(await listPurchaseOrderSummaries(warehouseId))
  });

export const usePurchaseOrderQuery = (purchaseOrderId: string | undefined) =>
  useQuery({
    queryKey: purchaseOrderKeys.detail(purchaseOrderId ?? ''),
    queryFn: async () => toPurchaseOrder(await getPurchaseOrder(purchaseOrderId ?? '')),
    enabled: Boolean(purchaseOrderId)
  });

export const useApprovedPurchaseOrderLinesQuery = (warehouseId: string) =>
  useQuery({
    queryKey: purchaseOrderKeys.approvedLines(warehouseId),
    queryFn: async () => toApprovedPurchaseOrderLines(await listApprovedPurchaseOrderLines(warehouseId)),
    enabled: warehouseId.length > 0
  });

export const useSubmitPurchaseOrderMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: SubmitPurchaseOrderCommand) => toPurchaseOrder(await submitPurchaseOrder(toSubmitPurchaseOrderRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
    }
  });
};

export const useAddPurchaseOrderLineMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: AddPurchaseOrderLineCommand) => toPurchaseOrder(await addPurchaseOrderLine(command.purchaseOrderId, toAddPurchaseOrderLineRequestDto(command))),
    onSuccess: async (purchaseOrder) => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.detail(purchaseOrder.id) });
    }
  });
};

export const usePurchaseOrderStatusMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: ChangePurchaseOrderStatusCommand) => toPurchaseOrder(await changePurchaseOrderStatus(command.purchaseOrderId, command.status, toChangePurchaseOrderStatusRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
    }
  });
};
