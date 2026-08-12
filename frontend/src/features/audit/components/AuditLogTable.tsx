import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
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
      <div className="overflow-x-auto">
        <table className="w-full min-w-225 text-left text-sm">
          <thead className="bg-muted text-xs uppercase text-muted-foreground">
            <tr>
              <th className="px-4 py-3">Time</th>
              <th className="px-4 py-3">Action</th>
              <th className="px-4 py-3">Warehouse</th>
              <th className="px-4 py-3">Item</th>
              <th className="px-4 py-3">Quantity</th>
              <th className="px-4 py-3">Available after</th>
              <th className="px-4 py-3">User</th>
            </tr>
          </thead>
          <tbody>
            {entries.map((entry) => {
              const warehouse = findWarehouse(warehouses, entry.warehouseId);
              const item = findInventoryItem(inventoryItems, entry.inventoryItemId);
              return (
                <tr className="border-t" key={entry.id}>
                  <td className="px-4 py-3">{entry.timestamp.toLocaleString()}</td>
                  <td className="px-4 py-3">{entry.action}</td>
                  <td className="px-4 py-3">{warehouse?.code ?? entry.warehouseId}</td>
                  <td className="px-4 py-3">{item?.displayName ?? entry.inventoryItemId}</td>
                  <td className="px-4 py-3">{entry.quantity}</td>
                  <td className="px-4 py-3">{entry.resultingAvailableQuantity}</td>
                  <td className="px-4 py-3">{entry.user}</td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  </div>
);
