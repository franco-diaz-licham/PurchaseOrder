import { UilCheck, UilPlus, UilSync, UilTimes } from '@iconscout/react-unicons';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppSelect } from '@/components/ui/AppSelect';
import { useInventoryItemsQuery, useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { usePurchaseOrdersQuery, usePurchaseOrderStatusMutation, useSubmitPurchaseOrderMutation } from '../queries/purchaseOrder.queries';

type FormValues = {
  purchaseOrderNumber: string;
  warehouseId: string;
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

export const PurchaseOrdersPage = () => {
  const [warehouseFilter, setWarehouseFilter] = useState('');
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const purchaseOrdersQuery = usePurchaseOrdersQuery(warehouseFilter || undefined);
  const submitMutation = useSubmitPurchaseOrderMutation();
  const statusMutation = usePurchaseOrderStatusMutation();

  const { register, handleSubmit, reset } = useForm<FormValues>({
    defaultValues: {
      purchaseOrderNumber: '',
      warehouseId: '',
      inventoryItemId: '',
      quantityOrdered: 1,
      user: 'demo-user'
    }
  });

  const sortedOrders = useMemo(() => purchaseOrdersQuery.data ?? [], [purchaseOrdersQuery.data]);

  const submit = handleSubmit(async (values) => {
    await submitMutation.mutateAsync({
      purchaseOrderNumber: values.purchaseOrderNumber,
      warehouseId: values.warehouseId,
      user: values.user,
      lines: [{ inventoryItemId: values.inventoryItemId, quantityOrdered: Number(values.quantityOrdered) }]
    });
    reset({ purchaseOrderNumber: '', warehouseId: values.warehouseId, inventoryItemId: '', quantityOrdered: 1, user: values.user });
  });

  return (
    <section>
      <PageHeader description="Create purchase orders, review lines, and move records through the basic lifecycle." title="Purchase Orders">
        <AppSelect value={warehouseFilter} onChange={(event) => setWarehouseFilter(event.target.value)}>
          <option value="">All warehouses</option>
          {(warehousesQuery.data ?? []).map((warehouse) => (
            <option key={warehouse.id} value={warehouse.id}>
              {warehouse.displayName}
            </option>
          ))}
        </AppSelect>
      </PageHeader>

      <div className="grid gap-6 p-6 xl:grid-cols-[380px_1fr]">
        <form className="self-start rounded-md border bg-card p-4" onSubmit={submit}>
          <h2 className="text-base font-semibold">New purchase order</h2>
          <div className="mt-4 grid gap-3">
            <AppField label="PO number">
              <AppInput required {...register('purchaseOrderNumber')} placeholder="PO-1021" />
            </AppField>
            <AppField label="Warehouse">
              <AppSelect required {...register('warehouseId')}>
                <option value="">Select warehouse</option>
                {(warehousesQuery.data ?? []).map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.displayName}
                  </option>
                ))}
              </AppSelect>
            </AppField>
            <AppField label="Inventory item">
              <AppSelect required {...register('inventoryItemId')}>
                <option value="">Select item</option>
                {(itemsQuery.data ?? []).map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.displayName}
                  </option>
                ))}
              </AppSelect>
            </AppField>
            <AppField label="Quantity">
              <AppInput min="0.001" required step="0.001" type="number" {...register('quantityOrdered', { valueAsNumber: true })} />
            </AppField>
            <AppField label="User">
              <AppInput required {...register('user')} />
            </AppField>
            <AppButton disabled={submitMutation.isPending} type="submit">
              <UilPlus className="h-4 w-4" />
              Submit
            </AppButton>
          </div>
        </form>

        <div className="grid gap-3">
          {purchaseOrdersQuery.isError && <ErrorMessage message="Purchase orders could not be loaded." />}
          {sortedOrders.length === 0 && !purchaseOrdersQuery.isLoading && <EmptyState title="No purchase orders found." />}
          {sortedOrders.map((order) => {
            const warehouse = findWarehouse(warehousesQuery.data, order.warehouseId);
            return (
              <article className="rounded-md border bg-card" key={order.id}>
                <div className="flex flex-col gap-3 border-b p-4 md:flex-row md:items-center md:justify-between">
                  <div>
                    <div className="flex items-center gap-3">
                      <h2 className="text-base font-semibold">{order.number}</h2>
                      <StatusBadge status={order.status} />
                    </div>
                    <p className="mt-1 text-sm text-muted-foreground">{warehouse?.displayName ?? order.warehouseId}</p>
                  </div>
                  <div className="flex flex-wrap gap-2">
                    <AppButton appearance="secondary" disabled={statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: order.id, status: 'approve', user: 'demo-user' })} size="sm">
                      <UilCheck className="h-4 w-4" />
                      Approve
                    </AppButton>
                    <AppButton appearance="secondary" disabled={statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: order.id, status: 'close', user: 'demo-user' })} size="sm">
                      <UilSync className="h-4 w-4" />
                      Close
                    </AppButton>
                    <AppButton appearance="danger" disabled={statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: order.id, status: 'cancel', user: 'demo-user' })} size="sm">
                      <UilTimes className="h-4 w-4" />
                      Cancel
                    </AppButton>
                  </div>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full min-w-[700px] text-left text-sm">
                    <thead className="bg-muted text-xs uppercase text-muted-foreground">
                      <tr>
                        <th className="px-4 py-3">Item</th>
                        <th className="px-4 py-3">Ordered</th>
                        <th className="px-4 py-3">Reserved</th>
                        <th className="px-4 py-3">Remaining</th>
                      </tr>
                    </thead>
                    <tbody>
                      {order.lines.map((line) => {
                        const item = findInventoryItem(itemsQuery.data, line.inventoryItemId);
                        return (
                          <tr className="border-t" key={line.id}>
                            <td className="px-4 py-3">{item?.displayName ?? line.inventoryItemId}</td>
                            <td className="px-4 py-3">{line.quantityOrdered}</td>
                            <td className="px-4 py-3">{line.quantityReserved}</td>
                            <td className="px-4 py-3">{line.quantityRemaining}</td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              </article>
            );
          })}
        </div>
      </div>
    </section>
  );
};
