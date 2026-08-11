import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { ApprovedPurchaseOrderLineDto, ChangePurchaseOrderStatusRequestDto, PurchaseOrderDto, SubmitPurchaseOrderRequestDto } from '../types/purchaseOrder.api.types';

export const listPurchaseOrders = async (warehouseId?: string) => {
  const response = await http.get<ApiResponse<PurchaseOrderDto[]>>('/purchase-order', {
    params: warehouseId ? { warehouseId } : undefined
  });
  return response.data.data;
};

export const listApprovedPurchaseOrderLines = async (warehouseId: string) => {
  const response = await http.get<ApiResponse<ApprovedPurchaseOrderLineDto[]>>('/purchase-order/approved-lines', {
    params: { warehouseId }
  });
  return response.data.data;
};

export const submitPurchaseOrder = async (request: SubmitPurchaseOrderRequestDto) => {
  const response = await http.post<ApiResponse<PurchaseOrderDto>>('/purchase-order', request);
  return response.data.data;
};

export const changePurchaseOrderStatus = async (purchaseOrderId: string, status: 'approve' | 'close' | 'cancel', request: ChangePurchaseOrderStatusRequestDto) => {
  const response = await http.put<ApiResponse<PurchaseOrderDto>>(`/purchase-order/${purchaseOrderId}/${status}`, request);
  return response.data.data;
};
