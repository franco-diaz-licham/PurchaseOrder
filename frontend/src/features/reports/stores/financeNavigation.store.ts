import { create } from 'zustand';

type FinanceNavigationState = {
  selectedWarehouseId: string | null;
  openedPurchaseOrderId: string | null;
  setSelectedWarehouseId: (warehouseId: string | null) => void;
  openPurchaseOrder: (purchaseOrderId: string, warehouseId: string) => void;
  clearOpenedPurchaseOrder: () => void;
};

export const useFinanceNavigationStore = create<FinanceNavigationState>((set) => ({
  selectedWarehouseId: null,
  openedPurchaseOrderId: null,
  setSelectedWarehouseId: (warehouseId) => set({ selectedWarehouseId: warehouseId }),
  openPurchaseOrder: (purchaseOrderId, warehouseId) =>
    set({
      openedPurchaseOrderId: purchaseOrderId,
      selectedWarehouseId: warehouseId
    }),
  clearOpenedPurchaseOrder: () => set({ openedPurchaseOrderId: null })
}));
