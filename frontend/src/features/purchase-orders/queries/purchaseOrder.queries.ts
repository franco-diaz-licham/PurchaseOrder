import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toApprovedPurchaseOrderLines, toChangePurchaseOrderStatusRequestDto, toPurchaseOrder, toPurchaseOrders, toSubmitPurchaseOrderRequestDto } from '../mappers/purchaseOrder.mapper';
import { changePurchaseOrderStatus, listApprovedPurchaseOrderLines, listPurchaseOrders, submitPurchaseOrder } from '../services/purchaseOrder.services';
import type { ChangePurchaseOrderStatusCommand, SubmitPurchaseOrderCommand } from '../types/purchaseOrder.types';

export const purchaseOrderKeys = {
  all: ['purchase-orders'] as const,
  list: (warehouseId?: string) => ['purchase-orders', warehouseId ?? 'all'] as const,
  approvedLines: (warehouseId: string) => ['approved-lines', warehouseId] as const
};

export const usePurchaseOrdersQuery = (warehouseId?: string) =>
  useQuery({
    queryKey: purchaseOrderKeys.list(warehouseId),
    queryFn: async () => toPurchaseOrders(await listPurchaseOrders(warehouseId))
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

export const usePurchaseOrderStatusMutation = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (command: ChangePurchaseOrderStatusCommand) => toPurchaseOrder(await changePurchaseOrderStatus(command.purchaseOrderId, command.status, toChangePurchaseOrderStatusRequestDto(command))),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: purchaseOrderKeys.all });
    }
  });
};
