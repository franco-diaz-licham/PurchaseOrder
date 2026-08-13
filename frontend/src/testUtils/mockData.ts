import type { InventoryItemResponseDto } from '@/features/catalog/types/catalog.api.types';
import type { InventoryItemModel, WarehouseModel, WarehouseStockModel } from '@/features/catalog/types/catalog.types';
import type { WarehouseCommittedValueResponseDto } from '@/features/reports/types/finance.api.types';
import type { ReleaseReservationRequestDto, ReservationResponseDto } from '@/features/reservations/types/reservation.api.types';
import type { ReleaseReservationModel, ReservationModel } from '@/features/reservations/types/reservation.types';
import type {
  AddPurchaseOrderLineRequestDto,
  ChangePurchaseOrderStatusRequestDto,
  PurchaseOrderResponseDto,
  PurchaseOrderSummaryResponseDto,
  RemovePurchaseOrderLineRequestDto,
  SubmitPurchaseOrderRequestDto,
  UpdatePurchaseOrderLineRequestDto
} from '@/features/purchase-orders/types/purchaseOrder.api.types';
import type {
  AddPurchaseOrderLineModel,
  ChangePurchaseOrderStatusModel,
  PurchaseOrderModel,
  PurchaseOrderSummaryModel,
  RemovePurchaseOrderLineModel,
  SubmitPurchaseOrderModel,
  UpdatePurchaseOrderLineModel
} from '@/features/purchase-orders/types/purchaseOrder.types';

export const mockWarehouses: WarehouseModel[] = [
  {
    id: 'warehouse-nsw',
    code: 'NSW',
    name: 'New South Wales',
    displayName: 'NSW - New South Wales'
  },
  {
    id: 'warehouse-qld',
    code: 'QLD',
    name: 'Queensland',
    displayName: 'QLD - Queensland'
  }
];

export const mockInventoryItems: InventoryItemModel[] = [
  {
    id: 'item-1',
    sku: 'BEAM-6M',
    name: '6m Spreader Beam',
    category: 'Hardware',
    trackingMode: 'Unit',
    standardCost: 1320,
    displayName: 'BEAM-6M - 6m Spreader Beam [Unit]'
  },
  {
    id: 'item-2',
    sku: 'PAD-OUTRIG',
    name: 'Outrigger Pad',
    category: 'Hardware',
    trackingMode: 'Unit',
    standardCost: 92,
    displayName: 'PAD-OUTRIG - Outrigger Pad [Unit]'
  },
  {
    id: 'item-3',
    sku: 'GREASE-EP2',
    name: 'EP2 Crane Grease',
    category: 'BulkGoods',
    trackingMode: 'Weight',
    standardCost: 18,
    displayName: 'GREASE-EP2 - EP2 Crane Grease [Weight]'
  }
];

export const mockInventoryItemResponseDto: InventoryItemResponseDto = {
  inventoryItemId: 'inventory-item-1',
  sku: 'WIRE-ROPE',
  name: 'Hoist Wire Rope',
  category: 'BulkGoods',
  trackingMode: 'Weight',
  standardCost: 6.8
};

export const mockPurchaseOrderResponseDto: PurchaseOrderResponseDto = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderNumber: 'PO-1021',
  warehouseId: 'warehouse-1',
  status: 'Pending',
  subtotalAmount: 100,
  gstAmount: 10,
  totalAmount: 110,
  lines: []
};

export const mockPurchaseOrderWithLinesResponseDto: PurchaseOrderResponseDto = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderNumber: 'PO-1021',
  warehouseId: 'warehouse-1',
  status: 'Approved',
  subtotalAmount: 120,
  gstAmount: 12,
  totalAmount: 132,
  lines: [
    {
      purchaseOrderLineId: 'line-1',
      inventoryItemId: 'item-1',
      quantityOrdered: 10,
      quantityReserved: 4,
      quantityRemaining: 6,
      unitCost: 12,
      lineAmount: 120
    }
  ]
};

export const mockPurchaseOrderSummaryResponseDto: PurchaseOrderSummaryResponseDto = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderNumber: 'PO-1021',
  warehouseId: 'warehouse-1',
  status: 'Pending',
  lineCount: 1,
  quantityOrdered: 10,
  quantityReserved: 0,
  quantityRemaining: 10,
  subtotalAmount: 100,
  gstAmount: 10,
  totalAmount: 110
};

export const mockPurchaseOrderSummaryWithReservationsResponseDto: PurchaseOrderSummaryResponseDto = {
  ...mockPurchaseOrderSummaryResponseDto,
  lineCount: 1,
  quantityReserved: 4,
  quantityRemaining: 6,
  subtotalAmount: 120,
  gstAmount: 12,
  totalAmount: 132
};

export const mockPurchaseOrderSummaryResponseDtos: PurchaseOrderSummaryResponseDto[] = [
  mockPurchaseOrderSummaryResponseDto,
  {
    purchaseOrderId: 'purchase-order-2',
    purchaseOrderNumber: 'PO-1022',
    warehouseId: 'warehouse-2',
    status: 'Approved',
    lineCount: 2,
    quantityOrdered: 20,
    quantityReserved: 5,
    quantityRemaining: 15,
    subtotalAmount: 200,
    gstAmount: 20,
    totalAmount: 220
  }
];

export const mockReservationResponseDto: ReservationResponseDto = {
  stockReservationId: 'reservation-1',
  purchaseOrderLineId: 'line-1',
  warehouseId: 'warehouse-1',
  inventoryItemId: 'item-1',
  quantityReserved: 10.5,
  unitCostSnapshot: 4.25,
  status: 'Active',
  reservedBy: 'Franco Diaz',
  reservedAt: '2026-08-12T10:15:00Z'
};

export const mockWarehouseCommittedValueResponseDto: WarehouseCommittedValueResponseDto = {
  warehouseId: 'warehouse-nsw',
  warehouseCode: 'NSW',
  warehouseName: 'New South Wales',
  reservedQuantity: 25,
  reservationCount: 1,
  committedValue: 1200,
  reservations: [
    {
      stockReservationId: 'reservation-1',
      purchaseOrderId: 'purchase-order-1',
      purchaseOrderNumber: 'PO-1001',
      purchaseOrderLineId: 'line-1',
      inventoryItemId: 'item-1',
      sku: 'BEAM-6M',
      itemName: '6m Spreader Beam',
      trackingMode: 'Unit',
      quantityReserved: 25,
      unitCostSnapshot: 48,
      committedValue: 1200
    }
  ]
};

export const mockSubmitPurchaseOrderModel: SubmitPurchaseOrderModel = {
  warehouseId: 'warehouse-1',
  user: 'Franco Diaz',
  lines: [
    {
      inventoryItemId: 'item-1',
      quantityOrdered: 10
    }
  ]
};

export const mockAddPurchaseOrderLineModel: AddPurchaseOrderLineModel = {
  purchaseOrderId: 'purchase-order-1',
  inventoryItemId: 'item-1',
  quantityOrdered: 12.5,
  user: 'Franco Diaz'
};

export const mockRemovePurchaseOrderLineModel: RemovePurchaseOrderLineModel = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderLineId: 'line-1',
  user: 'Franco Diaz'
};

export const mockUpdatePurchaseOrderLineModel: UpdatePurchaseOrderLineModel = {
  purchaseOrderId: 'purchase-order-1',
  purchaseOrderLineId: 'line-1',
  quantityOrdered: 20,
  user: 'Franco Diaz'
};

export const mockChangePurchaseOrderStatusModel: ChangePurchaseOrderStatusModel = {
  purchaseOrderId: 'purchase-order-1',
  status: 'approve',
  user: 'Franco Diaz'
};

export const mockReleaseReservationModel: ReleaseReservationModel = {
  stockReservationId: 'reservation-1',
  quantity: 10.5,
  user: 'Franco Diaz'
};

export const mockSubmitPurchaseOrderRequestDto: SubmitPurchaseOrderRequestDto = {
  warehouseId: 'warehouse-1',
  user: 'Franco Diaz',
  lines: [{ inventoryItemId: 'item-1', quantityOrdered: 10 }]
};

export const mockAddPurchaseOrderLineRequestDto: AddPurchaseOrderLineRequestDto = {
  inventoryItemId: 'item-1',
  quantityOrdered: 12.5,
  user: 'Franco Diaz'
};

export const mockRemovePurchaseOrderLineRequestDto: RemovePurchaseOrderLineRequestDto = {
  user: 'Franco Diaz'
};

export const mockUpdatePurchaseOrderLineRequestDto: UpdatePurchaseOrderLineRequestDto = {
  quantityOrdered: 20,
  user: 'Franco Diaz'
};

export const mockChangePurchaseOrderStatusRequestDto: ChangePurchaseOrderStatusRequestDto = {
  user: 'Franco Diaz'
};

export const mockReleaseReservationRequestDto: ReleaseReservationRequestDto = {
  quantity: 10.5,
  user: 'Franco Diaz'
};

export const mockPurchaseOrder: PurchaseOrderModel = {
  id: 'purchase-order-1',
  number: 'PO-1021',
  warehouseId: 'warehouse-nsw',
  status: 'Approved',
  subtotalAmount: 13200,
  gstAmount: 1320,
  totalAmount: 14520,
  lines: [
    {
      id: 'line-1',
      inventoryItemId: 'item-1',
      quantityOrdered: 10,
      quantityReserved: 4,
      quantityRemaining: 6,
      unitCost: 1320,
      lineAmount: 13200
    }
  ]
};

export const mockCreatedPurchaseOrder: PurchaseOrderModel = {
  id: 'purchase-order-new',
  number: 'PO-1023',
  warehouseId: 'warehouse-nsw',
  status: 'Pending',
  subtotalAmount: 0,
  gstAmount: 0,
  totalAmount: 0,
  lines: []
};

export const mockPendingPurchaseOrder: PurchaseOrderModel = {
  ...mockPurchaseOrder,
  status: 'Pending',
  lines: []
};

export const mockClosedPurchaseOrder: PurchaseOrderModel = {
  ...mockPurchaseOrder,
  status: 'Closed',
  lines: []
};

export const mockPurchaseOrderSummaries: PurchaseOrderSummaryModel[] = [
  {
    id: 'purchase-order-1',
    number: 'PO-1021',
    warehouseId: 'warehouse-nsw',
    status: 'Approved',
    lineCount: 1,
    quantityOrdered: 10,
    quantityReserved: 4,
    quantityRemaining: 6,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  },
  {
    id: 'purchase-order-2',
    number: 'PO-1022',
    warehouseId: 'warehouse-qld',
    status: 'Pending',
    lineCount: 1,
    quantityOrdered: 5,
    quantityReserved: 0,
    quantityRemaining: 5,
    subtotalAmount: 50,
    gstAmount: 5,
    totalAmount: 55
  }
];

export const mockPurchaseOrderSummariesForFiltering: PurchaseOrderSummaryModel[] = [
  mockPurchaseOrderSummaries[0],
  {
    id: 'purchase-order-2',
    number: 'PO-1002',
    warehouseId: 'warehouse-nsw',
    status: 'Approved',
    lineCount: 1,
    quantityOrdered: 10,
    quantityReserved: 10,
    quantityRemaining: 0,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  },
  {
    id: 'purchase-order-3',
    number: 'PO-1003',
    warehouseId: 'warehouse-qld',
    status: 'Pending',
    lineCount: 1,
    quantityOrdered: 10,
    quantityReserved: 0,
    quantityRemaining: 10,
    subtotalAmount: 100,
    gstAmount: 10,
    totalAmount: 110
  }
];

export const mockWarehouseStock: WarehouseStockModel[] = [
  {
    warehouseId: 'warehouse-nsw',
    inventoryItemId: 'item-1',
    onHandQuantity: 20,
    activeReservedQuantity: 4,
    availableQuantity: 16
  },
  {
    warehouseId: 'warehouse-nsw',
    inventoryItemId: 'item-2',
    onHandQuantity: 50,
    activeReservedQuantity: 0,
    availableQuantity: 50
  }
];

export const mockReservations: ReservationModel[] = [
  {
    id: 'reservation-1',
    purchaseOrderLineId: 'line-1',
    warehouseId: 'warehouse-nsw',
    inventoryItemId: 'item-1',
    quantityReserved: 4,
    unitCostSnapshot: 1320,
    status: 'Active',
    reservedBy: 'Franco Diaz',
    reservedAt: new Date('2026-08-12T10:15:00Z')
  }
];
