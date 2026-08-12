import { UilPlus } from '@iconscout/react-unicons';
import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import { AppSelect } from '@/components/ui/AppSelect';
import { useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { usePurchaseOrderSummariesQuery } from '../queries/purchaseOrder.queries';

export const PurchaseOrdersPage = () => {
  const [warehouseFilter, setWarehouseFilter] = useState('');
  const navigate = useNavigate();
  const warehousesQuery = useWarehousesQuery();
  const purchaseOrdersQuery = usePurchaseOrderSummariesQuery(warehouseFilter || undefined);
  const purchaseOrders = useMemo(() => purchaseOrdersQuery.data ?? [], [purchaseOrdersQuery.data]);

  return (
    <section>
      <PageHeader description="Review purchase order summaries and open a record to manage its lines." title="Purchase Orders">
        <div className="flex gap-2">
          <AppSelect value={warehouseFilter} onChange={(event) => setWarehouseFilter(event.target.value)}>
            <option value="">All warehouses</option>
            {(warehousesQuery.data ?? []).map((warehouse) => (
              <option key={warehouse.id} value={warehouse.id}>
                {warehouse.displayName}
              </option>
            ))}
          </AppSelect>
          <AppButton onClick={() => navigate('/purchase-orders/new')}>
            <UilPlus className="h-4 w-4" />
            Add
          </AppButton>
        </div>
      </PageHeader>

      <div className="p-6">
        <div className="rounded-md border bg-card">
          {purchaseOrdersQuery.isError && <ErrorMessage message="Purchase orders could not be loaded." />}
          {purchaseOrders.length === 0 && !purchaseOrdersQuery.isLoading && <EmptyState title="No purchase orders found." />}

          {purchaseOrders.length > 0 && (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[900px] text-left text-sm">
                <thead className="bg-muted text-xs uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3">PO number</th>
                    <th className="px-4 py-3">Warehouse</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3">Lines</th>
                    <th className="px-4 py-3">Ordered</th>
                    <th className="px-4 py-3">Reserved</th>
                    <th className="px-4 py-3">Remaining</th>
                  </tr>
                </thead>
                <tbody>
                  {purchaseOrders.map((order) => {
                    const warehouse = findWarehouse(warehousesQuery.data, order.warehouseId);
                    return (
                      <tr
                        className="cursor-pointer border-t hover:bg-muted/60"
                        key={order.id}
                        onClick={() => navigate(`/purchase-orders/${order.id}`)}
                        onKeyDown={(event) => {
                          if (event.key === 'Enter') navigate(`/purchase-orders/${order.id}`);
                        }}
                        tabIndex={0}
                      >
                        <td className="px-4 py-3 font-semibold">{order.number}</td>
                        <td className="px-4 py-3">{warehouse?.displayName ?? order.warehouseId}</td>
                        <td className="px-4 py-3">
                          <StatusBadge status={order.status} />
                        </td>
                        <td className="px-4 py-3">{order.lineCount}</td>
                        <td className="px-4 py-3">{order.quantityOrdered}</td>
                        <td className="px-4 py-3">{order.quantityReserved}</td>
                        <td className="px-4 py-3">{order.quantityRemaining}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </section>
  );
};
