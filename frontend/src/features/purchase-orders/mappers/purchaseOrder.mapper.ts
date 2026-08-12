import type {
  AddPurchaseOrderLineRequestDto,
  ChangePurchaseOrderStatusRequestDto,
  PurchaseOrderResponseDto,
  PurchaseOrderLineResponseDto,
  PurchaseOrderSummaryResponseDto,
  RemovePurchaseOrderLineRequestDto,
  SubmitPurchaseOrderRequestDto,
  UpdatePurchaseOrderLineRequestDto
} from '../types/purchaseOrder.api.types';
import type {
  AddPurchaseOrderLineModel,
  ChangePurchaseOrderStatusModel,
  PurchaseOrderModel,
  PurchaseOrderLineModel,
  PurchaseOrderSummaryModel,
  RemovePurchaseOrderLineModel,
  SubmitPurchaseOrderModel,
  UpdatePurchaseOrderLineModel
} from '../types/purchaseOrder.types';

export const toPurchaseOrderLine = (dto: PurchaseOrderLineResponseDto): PurchaseOrderLineModel => ({
  id: dto.purchaseOrderLineId,
  inventoryItemId: dto.inventoryItemId,
  quantityOrdered: dto.quantityOrdered,
  quantityReserved: dto.quantityReserved,
  quantityRemaining: dto.quantityRemaining,
  unitCost: dto.unitCost,
  lineAmount: dto.lineAmount
});

export const toPurchaseOrder = (dto: PurchaseOrderResponseDto): PurchaseOrderModel => ({
  id: dto.purchaseOrderId,
  number: dto.purchaseOrderNumber,
  warehouseId: dto.warehouseId,
  status: dto.status,
  subtotalAmount: dto.subtotalAmount,
  gstAmount: dto.gstAmount,
  totalAmount: dto.totalAmount,
  lines: dto.lines.map(toPurchaseOrderLine)
});

export const toPurchaseOrderSummary = (dto: PurchaseOrderSummaryResponseDto): PurchaseOrderSummaryModel => ({
  id: dto.purchaseOrderId,
  number: dto.purchaseOrderNumber,
  warehouseId: dto.warehouseId,
  status: dto.status,
  lineCount: dto.lineCount,
  quantityOrdered: dto.quantityOrdered,
  quantityReserved: dto.quantityReserved,
  quantityRemaining: dto.quantityRemaining,
  subtotalAmount: dto.subtotalAmount,
  gstAmount: dto.gstAmount,
  totalAmount: dto.totalAmount
});

export const toPurchaseOrderSummaries = (dtos: PurchaseOrderSummaryResponseDto[]): PurchaseOrderSummaryModel[] => dtos.map(toPurchaseOrderSummary);

export const toSubmitPurchaseOrderRequestDto = (command: SubmitPurchaseOrderModel): SubmitPurchaseOrderRequestDto => ({
  warehouseId: command.warehouseId,
  user: command.user,
  lines: command.lines.map((line) => ({
    inventoryItemId: line.inventoryItemId,
    quantityOrdered: line.quantityOrdered
  }))
});

export const toAddPurchaseOrderLineRequestDto = (command: AddPurchaseOrderLineModel): AddPurchaseOrderLineRequestDto => ({
  inventoryItemId: command.inventoryItemId,
  quantityOrdered: command.quantityOrdered,
  user: command.user
});

export const toRemovePurchaseOrderLineRequestDto = (command: RemovePurchaseOrderLineModel): RemovePurchaseOrderLineRequestDto => ({
  user: command.user
});

export const toUpdatePurchaseOrderLineRequestDto = (command: UpdatePurchaseOrderLineModel): UpdatePurchaseOrderLineRequestDto => ({
  quantityOrdered: command.quantityOrdered,
  user: command.user
});

export const toChangePurchaseOrderStatusRequestDto = (command: ChangePurchaseOrderStatusModel): ChangePurchaseOrderStatusRequestDto => ({
  user: command.user
});
