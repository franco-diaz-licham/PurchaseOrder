import { create } from 'zustand';

type AuditLogState = {
  selectedWarehouseId: string;
  setSelectedWarehouseId: (warehouseId: string) => void;
};

export const useAuditLogStore = create<AuditLogState>((set) => ({
  selectedWarehouseId: '',
  setSelectedWarehouseId: (warehouseId) => set({ selectedWarehouseId: warehouseId })
}));
