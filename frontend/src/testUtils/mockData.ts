import type { InventoryItemModel, WarehouseModel, WarehouseStockModel } from '@/features/catalog/types/catalog.types';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import type { PurchaseOrderModel, PurchaseOrderSummaryModel } from '@/features/purchase-orders/types/purchaseOrder.types';

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
  }
];

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
