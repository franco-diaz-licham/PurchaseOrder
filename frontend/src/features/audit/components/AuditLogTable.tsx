import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
import type { InventoryItemModel, WarehouseModel } from '@/features/catalog/types/catalog.types';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import type { AuditLogEntryModel } from '../types/audit.types';

type AuditLogTableProps = {
  entries: AuditLogEntryModel[];
  inventoryItems?: InventoryItemModel[];
  isError: boolean;
  isLoading: boolean;
  warehouses?: WarehouseModel[];
};

export const AuditLogTable = ({ entries, inventoryItems, isError, isLoading, warehouses }: AuditLogTableProps) => (
  <div className="p-6">
    <div className="rounded-md border bg-card">
      {isError && <ErrorMessage message="Audit log could not be loaded." />}
      {entries.length === 0 && !isLoading && <EmptyState title="No audit entries found." />}
      <AppTableContainer maxHeight="calc(100vh - 10.5rem)">
        <AppTable minWidth="56.25rem">
          <AppTableHead sticky>
            <AppTableHeaderRow>
              <AppTableHeaderCell>Time</AppTableHeaderCell>
              <AppTableHeaderCell>Action</AppTableHeaderCell>
              <AppTableHeaderCell>Warehouse</AppTableHeaderCell>
              <AppTableHeaderCell>Item</AppTableHeaderCell>
              <AppTableHeaderCell align="right">Quantity</AppTableHeaderCell>
              <AppTableHeaderCell align="right">Available after</AppTableHeaderCell>
              <AppTableHeaderCell>User</AppTableHeaderCell>
            </AppTableHeaderRow>
          </AppTableHead>
          <AppTableBody>
            {entries.map((entry) => {
              const warehouse = findWarehouse(warehouses, entry.warehouseId);
              const item = findInventoryItem(inventoryItems, entry.inventoryItemId);
              return (
                <AppTableRow key={entry.id}>
                  <AppTableCell>{entry.timestamp.toLocaleString()}</AppTableCell>
                  <AppTableCell>{entry.action}</AppTableCell>
                  <AppTableCell>{warehouse?.code ?? entry.warehouseId}</AppTableCell>
                  <AppTableCell>{item?.displayName ?? entry.inventoryItemId}</AppTableCell>
                  <AppTableCell align="right">{entry.quantity}</AppTableCell>
                  <AppTableCell align="right">{entry.resultingAvailableQuantity}</AppTableCell>
                  <AppTableCell>{entry.user}</AppTableCell>
                </AppTableRow>
              );
            })}
          </AppTableBody>
        </AppTable>
      </AppTableContainer>
    </div>
  </div>
);
