import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { AddPurchaseOrderLineRequestDto, ApprovedPurchaseOrderLineDto, ChangePurchaseOrderStatusRequestDto, PurchaseOrderDto, PurchaseOrderSummaryDto, SubmitPurchaseOrderRequestDto } from '../types/purchaseOrder.api.types';

export const listPurchaseOrders = async (warehouseId?: string) => {
  const response = await http.get<ApiResponse<PurchaseOrderDto[]>>('/purchase-order', {
    params: warehouseId ? { warehouseId } : undefined
  });
  return response.data.data;
};

export const listPurchaseOrderSummaries = async (warehouseId?: string) => {
  const response = await http.get<ApiResponse<PurchaseOrderSummaryDto[]>>('/purchase-order/summary', {
    params: warehouseId ? { warehouseId } : undefined
  });
  return response.data.data;
};

export const getPurchaseOrder = async (purchaseOrderId: string) => {
  const response = await http.get<ApiResponse<PurchaseOrderDto>>(`/purchase-order/${purchaseOrderId}`);
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

export const addPurchaseOrderLine = async (purchaseOrderId: string, request: AddPurchaseOrderLineRequestDto) => {
  const response = await http.post<ApiResponse<PurchaseOrderDto>>(`/purchase-order/${purchaseOrderId}/lines`, request);
  return response.data.data;
};

export const changePurchaseOrderStatus = async (purchaseOrderId: string, status: 'approve' | 'close' | 'cancel', request: ChangePurchaseOrderStatusRequestDto) => {
  const response = await http.put<ApiResponse<PurchaseOrderDto>>(`/purchase-order/${purchaseOrderId}/${status}`, request);
  return response.data.data;
};
