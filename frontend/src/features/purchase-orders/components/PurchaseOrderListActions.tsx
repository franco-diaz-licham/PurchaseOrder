import { UilPlus } from '@iconscout/react-unicons';
import { AppButton } from '@/components/ui/AppButton';
import { AppCheckbox } from '@/components/ui/AppCheckbox';
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
    <AppCheckbox checked={showReadyToReserveOnly} label="Ready to reserve" onChange={onShowReadyToReserveOnlyChange} />
    <AppSelect
      options={warehouses.map((warehouse) => ({
        label: warehouse.displayName,
        value: warehouse.id
      }))}
      placeholder="All warehouses"
      value={warehouseFilter}
      onChange={(event) => onWarehouseFilterChange(event.target.value)}
    />
    <AppButton onClick={onAdd}>
      <UilPlus className="h-4 w-4" />
      New PO
    </AppButton>
  </div>
);
