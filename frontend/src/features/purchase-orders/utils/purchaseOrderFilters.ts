import type { PurchaseOrderSummaryModel } from '../types/purchaseOrder.types';

type PurchaseOrderFilter = {
  warehouseId: string;
  showReadyToReserveOnly: boolean;
};

export const filterPurchaseOrders = (purchaseOrders: PurchaseOrderSummaryModel[], filter: PurchaseOrderFilter) =>
  purchaseOrders.filter((order) => {
    if (filter.warehouseId.length > 0 && order.warehouseId !== filter.warehouseId) return false;
    if (filter.showReadyToReserveOnly && (order.status !== 'Approved' || order.quantityRemaining <= 0)) return false;
    return true;
  });
