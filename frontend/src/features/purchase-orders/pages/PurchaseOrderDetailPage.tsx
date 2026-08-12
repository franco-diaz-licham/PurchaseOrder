import { UilCheck, UilPlus, UilSync, UilTimes } from '@iconscout/react-unicons';
import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { useNavigate, useParams } from 'react-router-dom';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppSelect } from '@/components/ui/AppSelect';
import { useInventoryItemsQuery, useWarehousesQuery, useWarehouseStockQuery } from '@/features/catalog/queries/catalog.queries';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { useCreateReservationMutation, useReleaseReservationMutation, useReservationsQuery } from '@/features/reservations/queries/reservation.queries';
import { useAddPurchaseOrderLineMutation, usePurchaseOrderQuery, usePurchaseOrderStatusMutation } from '../queries/purchaseOrder.queries';

type AddLineFormValues = {
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

export const PurchaseOrderDetailPage = () => {
  const { purchaseOrderId } = useParams();
  const navigate = useNavigate();
  const purchaseOrderQuery = usePurchaseOrderQuery(purchaseOrderId);
  const statusMutation = usePurchaseOrderStatusMutation();
  const addLineMutation = useAddPurchaseOrderLineMutation();
  const createReservationMutation = useCreateReservationMutation();
  const releaseReservationMutation = useReleaseReservationMutation();
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const purchaseOrder = purchaseOrderQuery.data;
  const warehouseStockQuery = useWarehouseStockQuery(purchaseOrder?.warehouseId);
  const warehouse = purchaseOrder ? findWarehouse(warehousesQuery.data, purchaseOrder.warehouseId) : undefined;
  const canChangeLines = purchaseOrder !== undefined && purchaseOrder.status !== 'Closed' && purchaseOrder.status !== 'Cancelled';
  const canReserveStock = purchaseOrder?.status === 'Approved';
  const reservationsQuery = useReservationsQuery(purchaseOrder?.warehouseId, 'Active', canReserveStock);
  const activeReservations = useMemo(() => reservationsQuery.data ?? [], [reservationsQuery.data]);
  const stockByItemId = useMemo(() => new Map((warehouseStockQuery.data ?? []).map((stock) => [stock.inventoryItemId, stock])), [warehouseStockQuery.data]);
  const [reserveQuantities, setReserveQuantities] = useState<Record<string, string>>({});
  const [reservationUser, setReservationUser] = useState('demo-user');

  const addLineForm = useForm<AddLineFormValues>({
    defaultValues: {
      inventoryItemId: '',
      quantityOrdered: 1,
      user: 'demo-user'
    }
  });

  const addLine = addLineForm.handleSubmit(async (values) => {
    if (!purchaseOrder) return;

    await addLineMutation.mutateAsync({
      purchaseOrderId: purchaseOrder.id,
      inventoryItemId: values.inventoryItemId,
      quantityOrdered: Number(values.quantityOrdered),
      user: values.user
    });

    addLineForm.reset({ inventoryItemId: '', quantityOrdered: 1, user: values.user });
  });

  const reserveLine = async (lineId: string) => {
    if (!purchaseOrder) return;

    await createReservationMutation.mutateAsync({
      purchaseOrderLineId: lineId,
      warehouseId: purchaseOrder.warehouseId,
      quantity: Number(reserveQuantities[lineId] || 0),
      user: reservationUser
    });

    setReserveQuantities((current) => ({ ...current, [lineId]: '' }));
  };

  return (
    <section>
      <PageHeader description="Review the full purchase order aggregate and manage its lifecycle." title={purchaseOrder?.number ?? 'Purchase Order'}>
        <AppButton appearance="secondary" onClick={() => navigate('/purchase-orders')}>
          Back
        </AppButton>
      </PageHeader>

      <div className="grid gap-4 p-6">
        {purchaseOrderQuery.isError && <ErrorMessage message="Purchase order could not be loaded." />}
        {statusMutation.isError && <ErrorMessage message="Purchase order status could not be changed." />}
        {addLineMutation.isError && <ErrorMessage message="Purchase order line could not be added." />}
        {createReservationMutation.isError && <ErrorMessage message="Stock could not be reserved for this line." />}
        {releaseReservationMutation.isError && <ErrorMessage message="Reservation could not be released." />}
        {!purchaseOrder && !purchaseOrderQuery.isLoading && !purchaseOrderQuery.isError && <EmptyState title="Purchase order was not found." />}

        {purchaseOrder && (
          <article className="rounded-md border bg-card">
            <div className="flex flex-col gap-3 border-b p-4 md:flex-row md:items-center md:justify-between">
              <div>
                <div className="flex items-center gap-3">
                  <h2 className="text-base font-semibold">{purchaseOrder.number}</h2>
                  <StatusBadge status={purchaseOrder.status} />
                </div>
                <p className="mt-1 text-sm text-muted-foreground">{warehouse?.displayName ?? purchaseOrder.warehouseId}</p>
              </div>
              <div className="flex flex-wrap gap-2">
                <AppButton appearance="secondary" disabled={purchaseOrder.status !== 'Pending' || statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'approve', user: 'demo-user' })} size="sm">
                  <UilCheck className="h-4 w-4" />
                  Approve
                </AppButton>
                <AppButton
                  appearance="secondary"
                  disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || statusMutation.isPending}
                  onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'close', user: 'demo-user' })}
                  size="sm"
                >
                  <UilSync className="h-4 w-4" />
                  Close
                </AppButton>
                <AppButton
                  appearance="danger"
                  disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || statusMutation.isPending}
                  onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'cancel', user: 'demo-user' })}
                  size="sm"
                >
                  <UilTimes className="h-4 w-4" />
                  Cancel
                </AppButton>
              </div>
            </div>

            {canChangeLines && (
              <form className="grid gap-3 border-b p-4 md:grid-cols-[1fr_160px_180px_auto]" onSubmit={addLine}>
                <AppField label="Inventory item">
                  <AppSelect required {...addLineForm.register('inventoryItemId')}>
                    <option value="">Select item</option>
                    {(itemsQuery.data ?? []).map((item) => (
                      <option key={item.id} value={item.id}>
                        {item.displayName}
                      </option>
                    ))}
                  </AppSelect>
                </AppField>
                <AppField label="Quantity">
                  <AppInput min="0.001" required step="0.001" type="number" {...addLineForm.register('quantityOrdered', { valueAsNumber: true })} />
                </AppField>
                <AppField label="User">
                  <AppInput required {...addLineForm.register('user')} />
                </AppField>
                <AppButton className="self-end" disabled={addLineMutation.isPending} type="submit">
                  <UilPlus className="h-4 w-4" />
                  Add line
                </AppButton>
              </form>
            )}

            <div className="overflow-x-auto">
              <table className="w-full min-w-[760px] text-left text-sm">
                <thead className="bg-muted text-xs uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3">Item</th>
                    <th className="px-4 py-3">Ordered</th>
                    <th className="px-4 py-3">Reserved</th>
                    {canReserveStock && <th className="px-4 py-3">Available</th>}
                    <th className="px-4 py-3">Remaining</th>
                    {canReserveStock && <th className="px-4 py-3">Reserve</th>}
                    {canReserveStock && <th className="px-4 py-3">Active reservations</th>}
                  </tr>
                </thead>
                <tbody>
                  {purchaseOrder.lines.map((line) => {
                    const item = findInventoryItem(itemsQuery.data, line.inventoryItemId);
                    const stock = stockByItemId.get(line.inventoryItemId);
                    const lineReservations = activeReservations.filter((reservation) => reservation.purchaseOrderLineId === line.id);
                    const reserveQuantity = Number(reserveQuantities[line.id] || 0);
                    const availableQuantity = stock?.availableQuantity ?? 0;
                    const maxReserveQuantity = Math.min(line.quantityRemaining, availableQuantity);
                    return (
                      <tr className="border-t" key={line.id}>
                        <td className="px-4 py-3">{item?.displayName ?? line.inventoryItemId}</td>
                        <td className="px-4 py-3">{line.quantityOrdered}</td>
                        <td className="px-4 py-3">{line.quantityReserved}</td>
                        {canReserveStock && <td className="px-4 py-3">{stock?.availableQuantity ?? 'Not stocked'}</td>}
                        <td className="px-4 py-3">{line.quantityRemaining}</td>
                        {canReserveStock && (
                          <td className="px-4 py-3">
                            <div className="flex gap-2">
                              <AppInput
                                className="w-28"
                                disabled={maxReserveQuantity <= 0}
                                max={maxReserveQuantity}
                                min="0.001"
                                onChange={(event) => setReserveQuantities((current) => ({ ...current, [line.id]: event.target.value }))}
                                step="0.001"
                                type="number"
                                value={reserveQuantities[line.id] ?? ''}
                              />
                              <AppButton
                                disabled={maxReserveQuantity <= 0 || createReservationMutation.isPending || reserveQuantity <= 0 || reserveQuantity > maxReserveQuantity || reservationUser.trim().length === 0}
                                onClick={() => void reserveLine(line.id)}
                                size="sm"
                              >
                                Reserve
                              </AppButton>
                            </div>
                          </td>
                        )}
                        {canReserveStock && (
                          <td className="px-4 py-3">
                            <div className="grid gap-2">
                              {lineReservations.length === 0 && <span className="text-muted-foreground">None</span>}
                              {lineReservations.map((reservation) => (
                                <div className="flex items-center justify-between gap-2" key={reservation.id}>
                                  <span>
                                    {reservation.quantityReserved} at ${reservation.unitCostSnapshot.toFixed(2)}
                                  </span>
                                  <AppButton
                                    appearance="secondary"
                                    disabled={releaseReservationMutation.isPending || reservationUser.trim().length === 0}
                                    onClick={() => releaseReservationMutation.mutate({ stockReservationId: reservation.id, quantity: reservation.quantityReserved, user: reservationUser })}
                                    size="sm"
                                  >
                                    Release
                                  </AppButton>
                                </div>
                              ))}
                            </div>
                          </td>
                        )}
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            {canReserveStock && (
              <div className="border-t p-4">
                <AppField label="Reservation user">
                  <AppInput className="max-w-xs" onChange={(event) => setReservationUser(event.target.value)} value={reservationUser} />
                </AppField>
              </div>
            )}
          </article>
        )}
      </div>
    </section>
  );
};
