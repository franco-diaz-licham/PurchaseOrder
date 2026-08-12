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
import { useInventoryItemsQuery, useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { useCreateReservationMutation, useReleaseReservationMutation, useReservationsQuery } from '@/features/reservations/queries/reservation.queries';
import { useAddPurchaseOrderLineMutation, usePurchaseOrderQuery, usePurchaseOrderStatusMutation, useSubmitPurchaseOrderMutation } from '../queries/purchaseOrder.queries';

type CreateFormValues = {
  purchaseOrderNumber: string;
  warehouseId: string;
  user: string;
};

type AddLineFormValues = {
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

export const PurchaseOrderDetailPage = () => {
  const { purchaseOrderId } = useParams();
  const navigate = useNavigate();
  const isNew = purchaseOrderId === undefined;
  const purchaseOrderQuery = usePurchaseOrderQuery(isNew ? undefined : purchaseOrderId);
  const statusMutation = usePurchaseOrderStatusMutation();
  const submitMutation = useSubmitPurchaseOrderMutation();
  const addLineMutation = useAddPurchaseOrderLineMutation();
  const createReservationMutation = useCreateReservationMutation();
  const releaseReservationMutation = useReleaseReservationMutation();
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const purchaseOrder = purchaseOrderQuery.data;
  const warehouse = purchaseOrder ? findWarehouse(warehousesQuery.data, purchaseOrder.warehouseId) : undefined;
  const canChangeLines = purchaseOrder?.status === 'Draft';
  const canReserveStock = purchaseOrder?.status === 'Approved';
  const reservationsQuery = useReservationsQuery(purchaseOrder?.warehouseId, 'Active', canReserveStock);
  const activeReservations = useMemo(() => reservationsQuery.data ?? [], [reservationsQuery.data]);
  const [reserveQuantities, setReserveQuantities] = useState<Record<string, string>>({});
  const [reservationUser, setReservationUser] = useState('demo-user');

  const createForm = useForm<CreateFormValues>({
    defaultValues: {
      purchaseOrderNumber: '',
      warehouseId: '',
      user: 'demo-user'
    }
  });

  const addLineForm = useForm<AddLineFormValues>({
    defaultValues: {
      inventoryItemId: '',
      quantityOrdered: 1,
      user: 'demo-user'
    }
  });

  const createPurchaseOrder = createForm.handleSubmit(async (values) => {
    const created = await submitMutation.mutateAsync({
      purchaseOrderNumber: values.purchaseOrderNumber,
      warehouseId: values.warehouseId,
      user: values.user,
      lines: []
    });

    navigate(`/purchase-orders/${created.id}`);
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
      <PageHeader description="Review the full purchase order aggregate and manage its lifecycle." title={isNew ? 'New Purchase Order' : (purchaseOrder?.number ?? 'Purchase Order')}>
        <AppButton appearance="secondary" onClick={() => navigate('/purchase-orders')}>
          Back
        </AppButton>
      </PageHeader>

      <div className="grid gap-4 p-6">
        {isNew && (
          <form className="max-w-xl rounded-md border bg-card p-4" onSubmit={createPurchaseOrder}>
            <h2 className="text-base font-semibold">Create purchase order</h2>
            <div className="mt-4 grid gap-3">
              <AppField label="PO number">
                <AppInput required {...createForm.register('purchaseOrderNumber')} placeholder="PO-1021" />
              </AppField>
              <AppField label="Warehouse">
                <AppSelect required {...createForm.register('warehouseId')}>
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
              <AppButton disabled={submitMutation.isPending} type="submit">
                <UilPlus className="h-4 w-4" />
                Create
              </AppButton>
            </div>
          </form>
        )}

        {!isNew && purchaseOrderQuery.isError && <ErrorMessage message="Purchase order could not be loaded." />}
        {statusMutation.isError && <ErrorMessage message="Purchase order status could not be changed." />}
        {addLineMutation.isError && <ErrorMessage message="Purchase order line could not be added." />}
        {createReservationMutation.isError && <ErrorMessage message="Stock could not be reserved for this line." />}
        {releaseReservationMutation.isError && <ErrorMessage message="Reservation could not be released." />}
        {!isNew && !purchaseOrder && !purchaseOrderQuery.isLoading && !purchaseOrderQuery.isError && <EmptyState title="Purchase order was not found." />}

        {!isNew && purchaseOrder && (
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
                <AppButton appearance="secondary" disabled={purchaseOrder.status !== 'Draft' || statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'approve', user: 'demo-user' })} size="sm">
                  <UilCheck className="h-4 w-4" />
                  Approve
                </AppButton>
                <AppButton appearance="secondary" disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'close', user: 'demo-user' })} size="sm">
                  <UilSync className="h-4 w-4" />
                  Close
                </AppButton>
                <AppButton appearance="danger" disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'cancel', user: 'demo-user' })} size="sm">
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
                    <th className="px-4 py-3">Remaining</th>
                    {canReserveStock && <th className="px-4 py-3">Reserve</th>}
                    {canReserveStock && <th className="px-4 py-3">Active reservations</th>}
                  </tr>
                </thead>
                <tbody>
                  {purchaseOrder.lines.map((line) => {
                    const item = findInventoryItem(itemsQuery.data, line.inventoryItemId);
                    const lineReservations = activeReservations.filter((reservation) => reservation.purchaseOrderLineId === line.id);
                    const reserveQuantity = Number(reserveQuantities[line.id] || 0);
                    return (
                      <tr className="border-t" key={line.id}>
                        <td className="px-4 py-3">{item?.displayName ?? line.inventoryItemId}</td>
                        <td className="px-4 py-3">{line.quantityOrdered}</td>
                        <td className="px-4 py-3">{line.quantityReserved}</td>
                        <td className="px-4 py-3">{line.quantityRemaining}</td>
                        {canReserveStock && (
                          <td className="px-4 py-3">
                            <div className="flex gap-2">
                              <AppInput
                                className="w-28"
                                disabled={line.quantityRemaining <= 0}
                                max={line.quantityRemaining}
                                min="0.001"
                                onChange={(event) => setReserveQuantities((current) => ({ ...current, [line.id]: event.target.value }))}
                                step="0.001"
                                type="number"
                                value={reserveQuantities[line.id] ?? ''}
                              />
                              <AppButton disabled={line.quantityRemaining <= 0 || createReservationMutation.isPending || reserveQuantity <= 0 || reserveQuantity > line.quantityRemaining || reservationUser.trim().length === 0} onClick={() => void reserveLine(line.id)} size="sm">
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
