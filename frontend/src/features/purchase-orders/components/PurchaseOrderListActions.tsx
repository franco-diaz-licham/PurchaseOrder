import { UilPlus } from '@iconscout/react-unicons';
import { AppButton } from '@/components/ui/AppButton';
import { AppSelect } from '@/components/ui/AppSelect';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';

type PurchaseOrderListActionsProps = {
  showReadyToReserveOnly: boolean;
  warehouseFilter: string;
  warehouses: WarehouseModel[];
  onAdd: () => void;
  onShowReadyToReserveOnlyChange: (value: boolean) => void;
  onWarehouseFilterChange: (value: string) => void;
};

export const PurchaseOrderListActions = ({ showReadyToReserveOnly, warehouseFilter, warehouses, onAdd, onShowReadyToReserveOnlyChange, onWarehouseFilterChange }: PurchaseOrderListActionsProps) => (
  <div className="flex flex-wrap items-center gap-3">
    <label className="flex items-center gap-2 text-sm font-medium">
      <input checked={showReadyToReserveOnly} className="h-4 w-4 accent-primary" onChange={(event) => onShowReadyToReserveOnlyChange(event.target.checked)} type="checkbox" />
      Ready to reserve
    </label>
    <AppSelect value={warehouseFilter} onChange={(event) => onWarehouseFilterChange(event.target.value)}>
      <option value="">All warehouses</option>
      {warehouses.map((warehouse) => (
        <option key={warehouse.id} value={warehouse.id}>
          {warehouse.displayName}
        </option>
      ))}
    </AppSelect>
    <AppButton onClick={onAdd}>
      <UilPlus className="h-4 w-4" />
      Add
    </AppButton>
  </div>
);
