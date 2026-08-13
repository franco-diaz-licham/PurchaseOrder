import { useMemo } from 'react';
import { PageLoadingIndicator } from '@/components/common/PageLoadingIndicator';
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

  const isLoading = auditQuery.isLoading || warehousesQuery.isLoading || itemsQuery.isLoading;

  if (isLoading) return <PageLoadingIndicator />;

  return (
    <section>
      <PageHeader description="Every successful reserve and release action is recorded here with resulting available quantity." title="Audit Log">
        <AppSelect
          options={(warehousesQuery.data ?? []).map((warehouse) => ({
            label: warehouse.displayName,
            value: warehouse.id
          }))}
          placeholder="All warehouses"
          value={warehouseId}
          onChange={(event) => setWarehouseId(event.target.value)}
        />
      </PageHeader>
      <AuditLogTable entries={auditEntries} inventoryItems={itemsQuery.data} isError={auditQuery.isError} isLoading={auditQuery.isLoading} warehouses={warehousesQuery.data} />
    </section>
  );
};
