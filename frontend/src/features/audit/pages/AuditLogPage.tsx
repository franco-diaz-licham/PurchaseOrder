import { useMemo } from 'react';
import { PageHeader } from '@/components/common/PageHeader';
import { AppSelect } from '@/components/ui/AppSelect';
import { useInventoryItemsQuery, useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { AuditLogTable } from '../components/AuditLogTable';
import { useAuditLogQuery } from '../queries/audit.queries';
import { useAuditLogStore } from '../stores/auditLog.store';

export const AuditLogPage = () => {
  const warehouseId = useAuditLogStore((state) => state.selectedWarehouseId);
  const setWarehouseId = useAuditLogStore((state) => state.setSelectedWarehouseId);
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const auditQuery = useAuditLogQuery();
  const auditEntries = useMemo(() => {
    const entries = auditQuery.data ?? [];
    if (!warehouseId) return entries;
    return entries.filter((entry) => entry.warehouseId === warehouseId);
  }, [auditQuery.data, warehouseId]);

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
      <AuditLogTable entries={auditEntries} inventoryItems={itemsQuery.data} isError={auditQuery.isError} isLoading={auditQuery.isLoading} warehouses={warehousesQuery.data} />
    </section>
  );
};
