import { UilEye } from '@iconscout/react-unicons';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { AppButton } from '@/components/ui/AppButton';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
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
      <AppTableContainer maxHeight="calc(100vh - 18rem)">
        <AppTable>
          <AppTableHead sticky>
            <AppTableHeaderRow>
              <AppTableHeaderCell>Warehouse</AppTableHeaderCell>
              <AppTableHeaderCell align="right">Reserved qty</AppTableHeaderCell>
              <AppTableHeaderCell align="right">Reservations</AppTableHeaderCell>
              <AppTableHeaderCell align="right">Committed value</AppTableHeaderCell>
              <AppTableHeaderCell align="right" ariaLabel="Warehouse details" />
            </AppTableHeaderRow>
          </AppTableHead>
          <AppTableBody>
            {rows.map((row) => (
              <AppTableRow key={row.warehouseId}>
                <AppTableCell>{row.warehouseDisplayName}</AppTableCell>
                <AppTableCell align="right">{row.reservedQuantity}</AppTableCell>
                <AppTableCell align="right">{row.reservationCount}</AppTableCell>
                <AppTableCell align="right" className="font-semibold">
                  {formatMoney(row.committedValue)}
                </AppTableCell>
                <AppTableCell align="right">
                  <AppButton aria-label="View warehouse reservations" appearance="secondary" className="h-8 w-8 px-0" disabled={row.reservationCount === 0} onClick={() => onViewWarehouse(row.warehouseId)} title="View warehouse reservations">
                    <UilEye className="h-4 w-4 text-blue-700" />
                  </AppButton>
                </AppTableCell>
              </AppTableRow>
            ))}
          </AppTableBody>
        </AppTable>
      </AppTableContainer>
    )}
  </div>
);
