import { UilPlus, UilTimes } from '@iconscout/react-unicons';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate } from 'react-router-dom';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppSelect } from '@/components/ui/AppSelect';
import { useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { usePurchaseOrderSummariesQuery, useSubmitPurchaseOrderMutation } from '../queries/purchaseOrder.queries';

type CreateFormValues = {
  warehouseId: string;
  user: string;
};

export const PurchaseOrdersPage = () => {
  const [warehouseFilter, setWarehouseFilter] = useState('');
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const navigate = useNavigate();
  const warehousesQuery = useWarehousesQuery();
  const purchaseOrdersQuery = usePurchaseOrderSummariesQuery();
  const submitMutation = useSubmitPurchaseOrderMutation();
  const purchaseOrders = useMemo(() => {
    const orders = purchaseOrdersQuery.data ?? [];
    if (warehouseFilter.length === 0) return orders;
    return orders.filter((order) => order.warehouseId === warehouseFilter);
  }, [purchaseOrdersQuery.data, warehouseFilter]);

  const createForm = useForm<CreateFormValues>({
    defaultValues: {
      warehouseId: '',
      user: 'demo-user'
    }
  });

  const closeCreateDialog = () => {
    setIsCreateOpen(false);
    createForm.reset({ warehouseId: '', user: 'demo-user' });
  };

  const createPurchaseOrder = createForm.handleSubmit(async (values) => {
    const created = await submitMutation.mutateAsync({
      warehouseId: values.warehouseId,
      user: values.user,
      lines: []
    });

    closeCreateDialog();
    navigate(`/purchase-orders/${created.id}`);
  });

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

      {isCreateOpen && (
        <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="create-po-title">
          <form className="w-full max-w-lg rounded-md border bg-card shadow-lg" onSubmit={createPurchaseOrder}>
            <div className="flex items-center justify-between border-b p-4">
              <h2 className="text-base font-semibold" id="create-po-title">
                New purchase order
              </h2>
              <AppButton appearance="ghost" onClick={closeCreateDialog} size="sm">
                <UilTimes className="h-4 w-4" />
              </AppButton>
            </div>
            <div className="grid gap-3 p-4">
              {submitMutation.isError && <ErrorMessage message="Purchase order could not be created." />}
              <AppField label="Warehouse">
                <AppSelect autoFocus required {...createForm.register('warehouseId')}>
                  <option value="">Select warehouse</option>
                  {(warehousesQuery.data ?? []).map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>
                      {warehouse.displayName}
                    </option>
                  ))}
                </AppSelect>
              </AppField>
              <AppField label="User">
                <AppInput required {...createForm.register('user')} />
              </AppField>
            </div>
            <div className="flex justify-end gap-2 border-t p-4">
              <AppButton appearance="secondary" onClick={closeCreateDialog}>
                Cancel
              </AppButton>
              <AppButton disabled={submitMutation.isPending} type="submit">
                Create
              </AppButton>
            </div>
          </form>
        </div>
      )}
    </section>
  );
};
