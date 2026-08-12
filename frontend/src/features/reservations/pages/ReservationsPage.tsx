import { UilArchive, UilPlus } from '@iconscout/react-unicons';
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
import { useApprovedPurchaseOrderLinesQuery } from '@/features/purchase-orders/queries/purchaseOrder.queries';
import { useCreateReservationMutation, useReleaseReservationMutation, useReservationsQuery } from '../queries/reservation.queries';

type ReserveFormValues = {
  purchaseOrderLineId: string;
  quantity: number;
  user: string;
};

export const ReservationsPage = () => {
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const firstWarehouseId = warehousesQuery.data?.[0]?.id ?? '';
  const [warehouseId, setWarehouseId] = useState('');
  const selectedWarehouseId = warehouseId || firstWarehouseId;
  const [status, setStatus] = useState('Active');
  const reservationsQuery = useReservationsQuery(selectedWarehouseId || undefined, status || undefined);
  const approvedLinesQuery = useApprovedPurchaseOrderLinesQuery(selectedWarehouseId);
  const createMutation = useCreateReservationMutation();
  const releaseMutation = useReleaseReservationMutation();

  const { register, handleSubmit, reset } = useForm<ReserveFormValues>({
    defaultValues: {
      purchaseOrderLineId: '',
      quantity: 1,
      user: 'Franco Diaz'
    }
  });

  const approvedLines = useMemo(() => approvedLinesQuery.data ?? [], [approvedLinesQuery.data]);

  const submit = handleSubmit(async (values) => {
    await createMutation.mutateAsync({
      warehouseId: selectedWarehouseId,
      purchaseOrderLineId: values.purchaseOrderLineId,
      quantity: Number(values.quantity),
      user: values.user
    });
    reset({ purchaseOrderLineId: '', quantity: 1, user: values.user });
  });

  return (
    <section>
      <PageHeader description="Reserve available stock against approved purchase order lines, then release active reservations when needed." title="Reservations">
        <div className="flex gap-2">
          <AppSelect value={selectedWarehouseId} onChange={(event) => setWarehouseId(event.target.value)}>
            {(warehousesQuery.data ?? []).map((warehouse) => (
              <option key={warehouse.id} value={warehouse.id}>
                {warehouse.code}
              </option>
            ))}
          </AppSelect>
          <AppSelect value={status} onChange={(event) => setStatus(event.target.value)}>
            <option value="">All</option>
            <option value="Active">Active</option>
            <option value="Released">Released</option>
          </AppSelect>
        </div>
      </PageHeader>

      <div className="grid gap-6 p-6 xl:grid-cols-[420px_1fr]">
        <form className="self-start rounded-md border bg-card p-4" onSubmit={submit}>
          <h2 className="text-base font-semibold">Create reservation</h2>
          <div className="mt-4 grid gap-3">
            <AppField label="Warehouse">
              <AppSelect
                required
                value={selectedWarehouseId}
                onChange={(event) => {
                  setWarehouseId(event.target.value);
                  reset({ purchaseOrderLineId: '', quantity: 1, user: 'Franco Diaz' });
                }}
              >
                {(warehousesQuery.data ?? []).map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.displayName}
                  </option>
                ))}
              </AppSelect>
            </AppField>
            <AppField label="Approved line">
              <AppSelect required {...register('purchaseOrderLineId')}>
                <option value="">Select approved line</option>
                {approvedLines.map((line) => (
                  <option key={line.id} value={line.id}>
                    {line.displayName}
                  </option>
                ))}
              </AppSelect>
            </AppField>
            <AppField label="Quantity">
              <AppInput min="0.001" required step="0.001" type="number" {...register('quantity', { valueAsNumber: true })} />
            </AppField>
            <AppField label="User">
              <AppInput required {...register('user')} />
            </AppField>
            <AppButton disabled={createMutation.isPending} type="submit">
              <UilPlus className="h-4 w-4" />
              Reserve
            </AppButton>
          </div>
        </form>

        <div className="rounded-md border bg-card">
          {reservationsQuery.isError && <ErrorMessage message="Reservations could not be loaded." />}
          {(reservationsQuery.data ?? []).length === 0 && !reservationsQuery.isLoading && <EmptyState title="No reservations found." />}
          <div className="overflow-x-auto">
            <table className="w-full min-w-[900px] text-left text-sm">
              <thead className="bg-muted text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Warehouse</th>
                  <th className="px-4 py-3">Item</th>
                  <th className="px-4 py-3">Reserved</th>
                  <th className="px-4 py-3">Cost snapshot</th>
                  <th className="px-4 py-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {(reservationsQuery.data ?? []).map((reservation) => {
                  const warehouse = findWarehouse(warehousesQuery.data, reservation.warehouseId);
                  const item = findInventoryItem(itemsQuery.data, reservation.inventoryItemId);
                  return (
                    <tr className="border-t" key={reservation.id}>
                      <td className="px-4 py-3">
                        <StatusBadge status={reservation.status} />
                      </td>
                      <td className="px-4 py-3">{warehouse?.code ?? reservation.warehouseId}</td>
                      <td className="px-4 py-3">{item?.displayName ?? reservation.inventoryItemId}</td>
                      <td className="px-4 py-3">{reservation.quantityReserved}</td>
                      <td className="px-4 py-3">${reservation.unitCostSnapshot.toFixed(2)}</td>
                      <td className="px-4 py-3">
                        <AppButton
                          appearance="secondary"
                          disabled={reservation.status !== 'Active' || releaseMutation.isPending}
                          onClick={() => releaseMutation.mutate({ stockReservationId: reservation.id, quantity: reservation.quantityReserved, user: 'Franco Diaz' })}
                        >
                          <UilArchive className="h-4 w-4" />
                          Release
                        </AppButton>
                      </td>
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
