import type { ApiResponse } from '@/lib/api/api.types';
import http from '@/lib/api/http';
import type { AddPurchaseOrderLineRequestDto, ChangePurchaseOrderStatusRequestDto, PurchaseOrderResponseDto, PurchaseOrderSummaryResponseDto, RemovePurchaseOrderLineRequestDto, SubmitPurchaseOrderRequestDto } from '../types/purchaseOrder.api.types';

export const listPurchaseOrderSummaries = async () => {
  const response = await http.get<ApiResponse<PurchaseOrderSummaryResponseDto[]>>('/purchase-order/summary');
  return response.data.data;
};

export const getPurchaseOrder = async (purchaseOrderId: string) => {
  const response = await http.get<ApiResponse<PurchaseOrderResponseDto>>(`/purchase-order/${purchaseOrderId}`);
  return response.data.data;
};

export const submitPurchaseOrder = async (request: SubmitPurchaseOrderRequestDto) => {
  const response = await http.post<ApiResponse<PurchaseOrderResponseDto>>('/purchase-order', request);
  return response.data.data;
};

export const addPurchaseOrderLine = async (purchaseOrderId: string, request: AddPurchaseOrderLineRequestDto) => {
  const response = await http.post<ApiResponse<PurchaseOrderResponseDto>>(`/purchase-order/${purchaseOrderId}/lines`, request);
  return response.data.data;
};

export const removePurchaseOrderLine = async (purchaseOrderId: string, purchaseOrderLineId: string, request: RemovePurchaseOrderLineRequestDto) => {
  const response = await http.delete<ApiResponse<PurchaseOrderResponseDto>>(`/purchase-order/${purchaseOrderId}/lines/${purchaseOrderLineId}`, {
    data: request
  });
  return response.data.data;
};

export const changePurchaseOrderStatus = async (purchaseOrderId: string, status: 'approve' | 'close' | 'cancel', request: ChangePurchaseOrderStatusRequestDto) => {
  const response = await http.put<ApiResponse<PurchaseOrderResponseDto>>(`/purchase-order/${purchaseOrderId}/${status}`, request);
  return response.data.data;
};
