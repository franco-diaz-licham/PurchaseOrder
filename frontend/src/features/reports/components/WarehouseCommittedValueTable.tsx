import { UilEye } from '@iconscout/react-unicons';
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
            <th className="px-4 py-3 text-right">Reserved qty</th>
            <th className="px-4 py-3 text-right">Reservations</th>
            <th className="px-4 py-3 text-right">Committed value</th>
            <th className="px-4 py-3 text-right" aria-label="Warehouse details" />
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr className="border-t" key={row.warehouseId}>
              <td className="px-4 py-3">{row.warehouseDisplayName}</td>
              <td className="px-4 py-3 text-right">{row.reservedQuantity}</td>
              <td className="px-4 py-3 text-right">{row.reservationCount}</td>
              <td className="px-4 py-3 text-right font-semibold">{formatMoney(row.committedValue)}</td>
              <td className="px-4 py-3 text-right">
                <AppButton aria-label="View warehouse reservations" appearance="secondary" className="h-8 w-8 px-0" disabled={row.reservationCount === 0} onClick={() => onViewWarehouse(row.warehouseId)} title="View warehouse reservations">
                  <UilEye className="h-4 w-4 text-blue-700" />
                </AppButton>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    )}
  </div>
);
