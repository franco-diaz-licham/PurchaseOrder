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
import { formatMoney } from '@/lib/formatMoney';
import { CreatePurchaseOrderDialog, type CreatePurchaseOrderFormValues } from '../components/CreatePurchaseOrderDialog';
import { usePurchaseOrderSummariesQuery, useSubmitPurchaseOrderMutation } from '../queries/purchaseOrder.queries';
import { usePurchaseOrderListStore } from '../stores/purchaseOrderList.store';

export const PurchaseOrdersPage = () => {
  const warehouseFilter = usePurchaseOrderListStore((state) => state.selectedWarehouseId);
  const showReadyToReserveOnly = usePurchaseOrderListStore((state) => state.showReadyToReserveOnly);
  const setWarehouseFilter = usePurchaseOrderListStore((state) => state.setSelectedWarehouseId);
  const setShowReadyToReserveOnly = usePurchaseOrderListStore((state) => state.setShowReadyToReserveOnly);
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const navigate = useNavigate();
  const warehousesQuery = useWarehousesQuery();
  const purchaseOrdersQuery = usePurchaseOrderSummariesQuery();
  const submitMutation = useSubmitPurchaseOrderMutation();
  const purchaseOrders = useMemo(() => {
    const orders = purchaseOrdersQuery.data ?? [];
    return orders.filter((order) => {
      if (warehouseFilter.length > 0 && order.warehouseId !== warehouseFilter) return false;
      if (showReadyToReserveOnly && (order.status !== 'Approved' || order.quantityRemaining <= 0)) return false;
      return true;
    });
  }, [purchaseOrdersQuery.data, showReadyToReserveOnly, warehouseFilter]);

  const closeCreateDialog = () => {
    setIsCreateOpen(false);
  };

  const createPurchaseOrder = async (values: CreatePurchaseOrderFormValues) => {
    const created = await submitMutation.mutateAsync({
      warehouseId: values.warehouseId,
      user: values.user,
      lines: []
    });

    closeCreateDialog();
    navigate(`/purchase-orders/${created.id}`);
  };

  return (
    <section>
      <PageHeader description="Review purchase order summaries and open a record to manage its lines." title="Purchase Orders">
        <div className="flex flex-wrap items-center gap-3">
          <label className="flex items-center gap-2 text-sm font-medium">
            <input checked={showReadyToReserveOnly} className="h-4 w-4 accent-primary" onChange={(event) => setShowReadyToReserveOnly(event.target.checked)} type="checkbox" />
            Ready to reserve
          </label>
          <AppSelect value={warehouseFilter} onChange={(event) => setWarehouseFilter(event.target.value)}>
            <option value="">All warehouses</option>
            {(warehousesQuery.data ?? []).map((warehouse) => (
              <option key={warehouse.id} value={warehouse.id}>
                {warehouse.displayName}
              </option>
            ))}
          </AppSelect>
          <AppButton onClick={() => setIsCreateOpen(true)}>
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
                    <th className="px-4 py-3">Total</th>
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
                        <td className="px-4 py-3 font-semibold">{formatMoney(order.totalAmount)}</td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {isCreateOpen && <CreatePurchaseOrderDialog isError={submitMutation.isError} isSaving={submitMutation.isPending} onCancel={closeCreateDialog} onSubmit={createPurchaseOrder} warehouses={warehousesQuery.data ?? []} />}
    </section>
  );
};
