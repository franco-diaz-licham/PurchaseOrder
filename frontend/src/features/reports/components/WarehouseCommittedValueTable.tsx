import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { AppButton } from '@/components/ui/AppButton';
import { formatMoney } from '@/lib/formatMoney';
import type { WarehouseCommittedValueModel } from '../types/finance.types';

type WarehouseCommittedValueTableProps = {
  isError: boolean;
  isLoading: boolean;
  rows: WarehouseCommittedValueModel[];
  onViewWarehouse: (warehouseId: string) => void;
};

export const WarehouseCommittedValueTable = ({ isError, isLoading, rows, onViewWarehouse }: WarehouseCommittedValueTableProps) => (
  <div className="rounded-md border bg-card">
    {isError && <ErrorMessage message="Finance values could not be loaded." />}
    {rows.length === 0 && !isLoading && <EmptyState title="No committed reservation value found." />}
    {rows.length > 0 && (
      <table className="w-full text-left text-sm">
        <thead className="bg-muted text-xs uppercase text-muted-foreground">
          <tr>
            <th className="px-4 py-3">Warehouse</th>
            <th className="px-4 py-3">Reserved qty</th>
            <th className="px-4 py-3">Reservations</th>
            <th className="px-4 py-3">Committed value</th>
            <th className="px-4 py-3">Details</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr className="border-t" key={row.warehouseId}>
              <td className="px-4 py-3">{row.warehouseDisplayName}</td>
              <td className="px-4 py-3">{row.reservedQuantity}</td>
              <td className="px-4 py-3">{row.reservationCount}</td>
              <td className="px-4 py-3 font-semibold">{formatMoney(row.committedValue)}</td>
              <td className="px-4 py-3">
                <AppButton appearance="secondary" disabled={row.reservationCount === 0} onClick={() => onViewWarehouse(row.warehouseId)}>
                  View
                </AppButton>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    )}
  </div>
);
