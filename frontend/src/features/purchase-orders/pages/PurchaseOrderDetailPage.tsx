import { UilCheck, UilPlus, UilSync, UilTimes } from '@iconscout/react-unicons';
import { useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import { useInventoryItemsQuery, useWarehousesQuery, useWarehouseStockQuery } from '@/features/catalog/queries/catalog.queries';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { useCreateReservationMutation, useReleaseReservationMutation, useReservationsQuery } from '@/features/reservations/queries/reservation.queries';
import { useFinanceNavigationStore } from '@/features/reports/stores/financeNavigation.store';
import { AddPurchaseOrderLineDialog, type AddPurchaseOrderLineFormValues } from '../components/AddPurchaseOrderLineDialog';
import { ManageReservationsDialog } from '../components/ManageReservationsDialog';
import { useAddPurchaseOrderLineMutation, usePurchaseOrderQuery, usePurchaseOrderStatusMutation, useRemovePurchaseOrderLineMutation } from '../queries/purchaseOrder.queries';

const money = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD'
});

export const PurchaseOrderDetailPage = () => {
  const { purchaseOrderId } = useParams();
  const navigate = useNavigate();
  const openedFinancePurchaseOrderId = useFinanceNavigationStore((state) => state.openedPurchaseOrderId);
  const clearOpenedFinancePurchaseOrder = useFinanceNavigationStore((state) => state.clearOpenedPurchaseOrder);
  const purchaseOrderQuery = usePurchaseOrderQuery(purchaseOrderId);
  const statusMutation = usePurchaseOrderStatusMutation();
  const addLineMutation = useAddPurchaseOrderLineMutation();
  const removeLineMutation = useRemovePurchaseOrderLineMutation();
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
  const existingLineItemIds = useMemo(() => new Set((purchaseOrder?.lines ?? []).map((line) => line.inventoryItemId)), [purchaseOrder?.lines]);
  const availableItemsToAdd = useMemo(() => (itemsQuery.data ?? []).filter((item) => !existingLineItemIds.has(item.id)), [existingLineItemIds, itemsQuery.data]);
  const [isAddLineOpen, setIsAddLineOpen] = useState(false);
  const [manageReservationsLineId, setManageReservationsLineId] = useState<string | null>(null);
  const [reservationUser, setReservationUser] = useState('Franco Diaz');
  const cameFromFinance = openedFinancePurchaseOrderId === purchaseOrderId;

  const manageReservationsLine = purchaseOrder?.lines.find((line) => line.id === manageReservationsLineId);
  const manageReservationsItem = manageReservationsLine ? findInventoryItem(itemsQuery.data, manageReservationsLine.inventoryItemId) : undefined;
  const manageReservationsStock = manageReservationsLine ? stockByItemId.get(manageReservationsLine.inventoryItemId) : undefined;
  const manageReservationsAvailableQuantity = manageReservationsStock?.availableQuantity ?? 0;
  const manageReservationsMaxQuantity = manageReservationsLine ? Math.min(manageReservationsLine.quantityRemaining, manageReservationsAvailableQuantity) : 0;
  const manageReservations = manageReservationsLine ? activeReservations.filter((reservation) => reservation.purchaseOrderLineId === manageReservationsLine.id) : [];

  const openAddLineDialog = () => {
    setIsAddLineOpen(true);
  };

  const closeAddLineDialog = () => {
    setIsAddLineOpen(false);
  };

  const addLine = async (values: AddPurchaseOrderLineFormValues) => {
    if (!purchaseOrder) return;

    await addLineMutation.mutateAsync({
      purchaseOrderId: purchaseOrder.id,
      inventoryItemId: values.inventoryItemId,
      quantityOrdered: Number(values.quantityOrdered),
      user: values.user
    });

    setIsAddLineOpen(false);
  };

  const closeManageReservationsDialog = () => {
    setManageReservationsLineId(null);
  };

  const reserveLine = async (quantity: number, user: string) => {
    if (!purchaseOrder) return;
    if (!manageReservationsLineId) return;

    await createReservationMutation.mutateAsync({
      purchaseOrderLineId: manageReservationsLineId,
      warehouseId: purchaseOrder.warehouseId,
      quantity,
      user
    });
  };

  const goBack = () => {
    if (cameFromFinance) {
      clearOpenedFinancePurchaseOrder();
      navigate('/finance');
      return;
    }

    navigate('/purchase-orders');
  };

  return (
    <section>
      <PageHeader description="Review the full purchase order aggregate and manage its lifecycle." title={purchaseOrder?.number ?? 'Purchase Order'}>
        <AppButton appearance="secondary" onClick={goBack}>
          Back
        </AppButton>
      </PageHeader>

      <div className="grid gap-4 p-6">
        {purchaseOrderQuery.isError && <ErrorMessage message="Purchase order could not be loaded." />}
        {statusMutation.isError && <ErrorMessage message="Purchase order status could not be changed." />}
        {addLineMutation.isError && <ErrorMessage message="Purchase order line could not be added." />}
        {removeLineMutation.isError && <ErrorMessage message="Purchase order line could not be removed." />}
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
                {canChangeLines && (
                  <AppButton disabled={addLineMutation.isPending || availableItemsToAdd.length === 0} onClick={openAddLineDialog} type="button">
                    <UilPlus className="h-4 w-4" />
                    Add line
                  </AppButton>
                )}
                <AppButton appearance="secondary" disabled={purchaseOrder.status !== 'Pending' || statusMutation.isPending} onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'approve', user: 'Franco Diaz' })}>
                  <UilCheck className="h-4 w-4" />
                  Approve
                </AppButton>
                <AppButton
                  appearance="secondary"
                  disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || statusMutation.isPending}
                  onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'close', user: 'Franco Diaz' })}
                >
                  <UilSync className="h-4 w-4" />
                  Close
                </AppButton>
                <AppButton
                  appearance="danger"
                  disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || statusMutation.isPending}
                  onClick={() => statusMutation.mutate({ purchaseOrderId: purchaseOrder.id, status: 'cancel', user: 'Franco Diaz' })}
                >
                  <UilTimes className="h-4 w-4" />
                  Cancel
                </AppButton>
              </div>
            </div>

            <div className="grid gap-3 border-b p-4 text-sm md:grid-cols-3">
              <div>
                <p className="text-xs uppercase text-muted-foreground">Subtotal</p>
                <p className="mt-1 font-semibold">{money.format(purchaseOrder.subtotalAmount)}</p>
              </div>
              <div>
                <p className="text-xs uppercase text-muted-foreground">GST</p>
                <p className="mt-1 font-semibold">{money.format(purchaseOrder.gstAmount)}</p>
              </div>
              <div>
                <p className="text-xs uppercase text-muted-foreground">Total</p>
                <p className="mt-1 font-semibold">{money.format(purchaseOrder.totalAmount)}</p>
              </div>
            </div>
          </article>
        )}

        {purchaseOrder && (
          <article className="rounded-md border bg-card">
            <div className="p-4">
              <div className="overflow-x-auto rounded-md border">
                <table className={`w-full text-left text-sm ${canReserveStock ? 'min-w-[1180px]' : 'min-w-[720px]'}`}>
                  <thead>
                    <tr className="border-b bg-card text-sm">
                      <th className="px-4 py-3 font-semibold" colSpan={canChangeLines ? 7 : 6}>
                        Purchase order lines
                      </th>
                      {canReserveStock && (
                        <th className="border-l px-4 py-3 font-semibold" colSpan={2}>
                          Reservations
                        </th>
                      )}
                    </tr>
                    <tr className="bg-muted text-xs uppercase text-muted-foreground">
                      <th className="px-4 py-3">Item</th>
                      <th className="px-4 py-3">Ordered</th>
                      <th className="px-4 py-3">Reserved</th>
                      <th className="px-4 py-3">Remaining</th>
                      <th className="px-4 py-3">Unit cost</th>
                      <th className="px-4 py-3">Total Amount</th>
                      {canChangeLines && <th className="px-4 py-3">Remove</th>}
                      {canReserveStock && <th className="border-l px-4 py-3">Available</th>}
                      {canReserveStock && <th className="px-4 py-3">Active</th>}
                    </tr>
                  </thead>
                  <tbody>
                    {purchaseOrder.lines.map((line) => {
                      const item = findInventoryItem(itemsQuery.data, line.inventoryItemId);
                      const stock = stockByItemId.get(line.inventoryItemId);
                      const lineReservations = activeReservations.filter((reservation) => reservation.purchaseOrderLineId === line.id);
                      return (
                        <tr className="border-t" key={line.id}>
                          <td className="px-4 py-3">{item?.displayName ?? line.inventoryItemId}</td>
                          <td className="px-4 py-3">{line.quantityOrdered}</td>
                          <td className="px-4 py-3">{line.quantityReserved}</td>
                          <td className="px-4 py-3">{line.quantityRemaining}</td>
                          <td className="px-4 py-3">{money.format(line.unitCost)}</td>
                          <td className="px-4 py-3">{money.format(line.lineAmount)}</td>
                          {canChangeLines && (
                            <td className="px-4 py-3">
                              <AppButton
                                appearance="secondary"
                                disabled={removeLineMutation.isPending}
                                onClick={() =>
                                  removeLineMutation.mutate({
                                    purchaseOrderId: purchaseOrder.id,
                                    purchaseOrderLineId: line.id,
                                    user: reservationUser
                                  })
                                }
                              >
                                <UilTimes className="h-4 w-4" />
                                Remove
                              </AppButton>
                            </td>
                          )}
                          {canReserveStock && <td className="border-l px-4 py-3">{stock ? stock.availableQuantity : 'Not stocked'}</td>}
                          {canReserveStock && (
                            <td className="px-4 py-3">
                              <div className="flex items-center justify-between gap-2">
                                {lineReservations.length === 0 && <span className="text-muted-foreground">None</span>}
                                {lineReservations.length > 0 && <span>{lineReservations.length}</span>}
                                <AppButton appearance="secondary" onClick={() => setManageReservationsLineId(line.id)}>
                                  Manage
                                </AppButton>
                              </div>
                            </td>
                          )}
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          </article>
        )}
      </div>

      {isAddLineOpen && <AddPurchaseOrderLineDialog inventoryItems={availableItemsToAdd} isSaving={addLineMutation.isPending} onCancel={closeAddLineDialog} onSubmit={addLine} />}

      {manageReservationsLine && (
        <ManageReservationsDialog
          availableQuantity={manageReservationsStock?.availableQuantity ?? null}
          isReleasing={releaseReservationMutation.isPending}
          isReserving={createReservationMutation.isPending}
          itemName={manageReservationsItem?.displayName ?? manageReservationsLine.inventoryItemId}
          trackingMode={manageReservationsItem?.trackingMode}
          line={manageReservationsLine}
          maxReserveQuantity={manageReservationsMaxQuantity}
          onCancel={closeManageReservationsDialog}
          onRelease={(reservation, quantity) => releaseReservationMutation.mutate({ stockReservationId: reservation.id, quantity, user: reservationUser })}
          onReserve={reserveLine}
          onUserChange={setReservationUser}
          reservations={manageReservations}
          user={reservationUser}
        />
      )}
    </section>
  );
};
