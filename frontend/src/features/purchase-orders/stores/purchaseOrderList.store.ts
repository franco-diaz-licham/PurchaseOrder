import { create } from 'zustand';

type PurchaseOrderListState = {
  selectedWarehouseId: string;
  showReadyToReserveOnly: boolean;
  setSelectedWarehouseId: (warehouseId: string) => void;
  setShowReadyToReserveOnly: (value: boolean) => void;
};

export const usePurchaseOrderListStore = create<PurchaseOrderListState>((set) => ({
  selectedWarehouseId: '',
  showReadyToReserveOnly: false,
  setSelectedWarehouseId: (warehouseId) => set({ selectedWarehouseId: warehouseId }),
  setShowReadyToReserveOnly: (value) => set({ showReadyToReserveOnly: value })
}));
