import type { AddPurchaseOrderLineRequestDto, ApprovedPurchaseOrderLineDto, ChangePurchaseOrderStatusRequestDto, PurchaseOrderDto, PurchaseOrderLineDto, PurchaseOrderSummaryDto, SubmitPurchaseOrderRequestDto } from '../types/purchaseOrder.api.types';
import type { AddPurchaseOrderLineCommand, ApprovedPurchaseOrderLine, ChangePurchaseOrderStatusCommand, PurchaseOrder, PurchaseOrderLine, PurchaseOrderSummary, SubmitPurchaseOrderCommand } from '../types/purchaseOrder.types';

export const toPurchaseOrderLine = (dto: PurchaseOrderLineDto): PurchaseOrderLine => ({
  id: dto.purchaseOrderLineId,
  inventoryItemId: dto.inventoryItemId,
  quantityOrdered: dto.quantityOrdered,
  quantityReserved: dto.quantityReserved,
  quantityRemaining: dto.quantityRemaining
});

export const toPurchaseOrder = (dto: PurchaseOrderDto): PurchaseOrder => ({
  id: dto.purchaseOrderId,
  number: dto.purchaseOrderNumber,
  warehouseId: dto.warehouseId,
  status: dto.status,
  lines: dto.lines.map(toPurchaseOrderLine)
});

export const toPurchaseOrders = (dtos: PurchaseOrderDto[]): PurchaseOrder[] => dtos.map(toPurchaseOrder);

export const toPurchaseOrderSummary = (dto: PurchaseOrderSummaryDto): PurchaseOrderSummary => ({
  id: dto.purchaseOrderId,
  number: dto.purchaseOrderNumber,
  warehouseId: dto.warehouseId,
  status: dto.status,
  lineCount: dto.lineCount,
  quantityOrdered: dto.quantityOrdered,
  quantityReserved: dto.quantityReserved,
  quantityRemaining: dto.quantityRemaining
});

export const toPurchaseOrderSummaries = (dtos: PurchaseOrderSummaryDto[]): PurchaseOrderSummary[] => dtos.map(toPurchaseOrderSummary);

export const toApprovedPurchaseOrderLine = (dto: ApprovedPurchaseOrderLineDto): ApprovedPurchaseOrderLine => ({
  id: dto.purchaseOrderLineId,
  purchaseOrderId: dto.purchaseOrderId,
  purchaseOrderNumber: dto.purchaseOrderNumber,
  warehouseId: dto.warehouseId,
  warehouseCode: dto.warehouseCode,
  warehouseName: dto.warehouseName,
  inventoryItemId: dto.inventoryItemId,
  sku: dto.sku,
  itemName: dto.itemName,
  quantityOrdered: dto.quantityOrdered,
  quantityReserved: dto.quantityReserved,
  quantityRemaining: dto.quantityRemaining,
  displayName: `${dto.purchaseOrderNumber} - ${dto.sku} - remaining ${dto.quantityRemaining}`
});

export const toApprovedPurchaseOrderLines = (dtos: ApprovedPurchaseOrderLineDto[]): ApprovedPurchaseOrderLine[] => dtos.map(toApprovedPurchaseOrderLine);

export const toSubmitPurchaseOrderRequestDto = (command: SubmitPurchaseOrderCommand): SubmitPurchaseOrderRequestDto => ({
  purchaseOrderNumber: command.purchaseOrderNumber,
  warehouseId: command.warehouseId,
  user: command.user,
  lines: command.lines.map((line) => ({
    inventoryItemId: line.inventoryItemId,
    quantityOrdered: line.quantityOrdered
  }))
});

export const toAddPurchaseOrderLineRequestDto = (command: AddPurchaseOrderLineCommand): AddPurchaseOrderLineRequestDto => ({
  inventoryItemId: command.inventoryItemId,
  quantityOrdered: command.quantityOrdered,
  user: command.user
});

export const toChangePurchaseOrderStatusRequestDto = (command: ChangePurchaseOrderStatusCommand): ChangePurchaseOrderStatusRequestDto => ({
  user: command.user
});
