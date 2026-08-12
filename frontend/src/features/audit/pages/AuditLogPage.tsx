import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { AppSelect } from '@/components/ui/AppSelect';
import { useInventoryItemsQuery, useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { useAuditLogQuery } from '../queries/audit.queries';
import { useAuditLogStore } from '../stores/auditLog.store';

export const AuditLogPage = () => {
  const warehouseId = useAuditLogStore((state) => state.selectedWarehouseId);
  const setWarehouseId = useAuditLogStore((state) => state.setSelectedWarehouseId);
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const auditQuery = useAuditLogQuery(warehouseId || undefined);

  return (
    <section>
      <PageHeader description="Every successful reserve and release action is recorded here with resulting available quantity." title="Audit Log">
        <AppSelect value={warehouseId} onChange={(event) => setWarehouseId(event.target.value)}>
          <option value="">All warehouses</option>
          {(warehousesQuery.data ?? []).map((warehouse) => (
            <option key={warehouse.id} value={warehouse.id}>
              {warehouse.displayName}
            </option>
          ))}
        </AppSelect>
      </PageHeader>

      <div className="p-6">
        <div className="rounded-md border bg-card">
          {auditQuery.isError && <ErrorMessage message="Audit log could not be loaded." />}
          {(auditQuery.data ?? []).length === 0 && !auditQuery.isLoading && <EmptyState title="No audit entries found." />}
          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] text-left text-sm">
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
                {(auditQuery.data ?? []).map((entry) => {
                  const warehouse = findWarehouse(warehousesQuery.data, entry.warehouseId);
                  const item = findInventoryItem(itemsQuery.data, entry.inventoryItemId);
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
    </section>
  );
};
