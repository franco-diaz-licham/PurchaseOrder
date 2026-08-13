import { useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorSummary } from '@/components/common/ErrorSummary';
import { PageLoadingIndicator } from '@/components/common/PageLoadingIndicator';
import { PageHeader } from '@/components/common/PageHeader';
import { useInventoryItemsQuery, useWarehousesQuery, useWarehouseStockQuery } from '@/features/catalog/queries/catalog.queries';
import { findInventoryItem, findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { useCreateReservationMutation, useReleaseReservationMutation, useReservationsQuery } from '@/features/reservations/queries/reservation.queries';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import { AddPurchaseOrderLineDialog, type AddPurchaseOrderLineFormValues } from '../components/AddPurchaseOrderLineDialog';
import { EditPurchaseOrderLineDialog, type EditPurchaseOrderLineFormValues } from '../components/EditPurchaseOrderLineDialog';
import { ManageReservationsDialog } from '../components/ManageReservationsDialog';
import { PurchaseOrderHeaderCard } from '../components/PurchaseOrderHeaderCard';
import { PurchaseOrderLinesTable } from '../components/PurchaseOrderLinesTable';
import { PurchaseOrderTotalsBar } from '../components/PurchaseOrderTotalsBar';
import { useAddPurchaseOrderLineMutation, usePurchaseOrderQuery, usePurchaseOrderStatusMutation, useRemovePurchaseOrderLineMutation, useUpdatePurchaseOrderLineMutation } from '../queries/purchaseOrder.queries';
import { getPurchaseOrderDetailErrorMessages } from '../utils/purchaseOrderDetailErrors';

export const PurchaseOrderDetailPage = () => {
  const { purchaseOrderId } = useParams();

  const purchaseOrderQuery = usePurchaseOrderQuery(purchaseOrderId);
  const warehousesQuery = useWarehousesQuery();
  const itemsQuery = useInventoryItemsQuery();

  const statusMutation = usePurchaseOrderStatusMutation();
  const addLineMutation = useAddPurchaseOrderLineMutation();
  const removeLineMutation = useRemovePurchaseOrderLineMutation();
  const updateLineMutation = useUpdatePurchaseOrderLineMutation();
  const createReservationMutation = useCreateReservationMutation();
  const releaseReservationMutation = useReleaseReservationMutation();

  const purchaseOrder = purchaseOrderQuery.data;
  const warehouseStockQuery = useWarehouseStockQuery(purchaseOrder?.warehouseId);
  const reservationsQuery = useReservationsQuery(purchaseOrder?.status === 'Approved');

  const warehouse = purchaseOrder ? findWarehouse(warehousesQuery.data, purchaseOrder.warehouseId) : undefined;

  const canChangeLines = purchaseOrder !== undefined && purchaseOrder.status !== 'Closed' && purchaseOrder.status !== 'Cancelled';
  const canReserveStock = purchaseOrder?.status === 'Approved';

  const activeReservations = useMemo(() => {
    const reservations = reservationsQuery.data ?? [];
    if (!purchaseOrder) return [];

    return reservations.filter((reservation) => reservation.warehouseId === purchaseOrder.warehouseId && reservation.status === 'Active');
  }, [purchaseOrder, reservationsQuery.data]);
  const stockByItemId = useMemo(() => new Map((warehouseStockQuery.data ?? []).map((stock) => [stock.inventoryItemId, stock])), [warehouseStockQuery.data]);
  const existingLineItemIds = useMemo(() => new Set((purchaseOrder?.lines ?? []).map((line) => line.inventoryItemId)), [purchaseOrder?.lines]);
  const availableItemsToAdd = useMemo(() => (itemsQuery.data ?? []).filter((item) => !existingLineItemIds.has(item.id)), [existingLineItemIds, itemsQuery.data]);

  const [isAddLineOpen, setIsAddLineOpen] = useState(false);
  const [editLineId, setEditLineId] = useState<string | null>(null);
  const [dismissedErrorKey, setDismissedErrorKey] = useState('');
  const [changingStatusAction, setChangingStatusAction] = useState<'approve' | 'close' | 'cancel' | null>(null);
  const [manageReservationsLineId, setManageReservationsLineId] = useState<string | null>(null);
  const [removingLineId, setRemovingLineId] = useState<string | null>(null);
  const [reservationUser, setReservationUser] = useState('Franco Diaz');

  const editLine = purchaseOrder?.lines.find((line) => line.id === editLineId);
  const editLineItem = editLine ? findInventoryItem(itemsQuery.data, editLine.inventoryItemId) : undefined;
  const manageReservationsLine = purchaseOrder?.lines.find((line) => line.id === manageReservationsLineId);
  const manageReservationsItem = manageReservationsLine ? findInventoryItem(itemsQuery.data, manageReservationsLine.inventoryItemId) : undefined;
  const manageReservationsStock = manageReservationsLine ? stockByItemId.get(manageReservationsLine.inventoryItemId) : undefined;
  const manageReservationsAvailableQuantity = manageReservationsStock?.availableQuantity ?? 0;
  const manageReservationsMaxQuantity = manageReservationsLine ? Math.min(manageReservationsLine.quantityRemaining, manageReservationsAvailableQuantity) : 0;
  const manageReservations = manageReservationsLine ? activeReservations.filter((reservation) => reservation.purchaseOrderLineId === manageReservationsLine.id) : [];
  const errorMessages = getPurchaseOrderDetailErrorMessages({
    purchaseOrderQuery,
    statusMutation,
    addLineMutation,
    removeLineMutation,
    updateLineMutation,
    createReservationMutation,
    releaseReservationMutation
  });
  const errorKey = errorMessages.join('\n');
  const visibleErrorMessages = errorKey === dismissedErrorKey ? [] : errorMessages;

  const isPageLoading = purchaseOrderQuery.isLoading || warehousesQuery.isLoading || itemsQuery.isLoading || warehouseStockQuery.isLoading || reservationsQuery.isLoading;

  const closeAddLineDialog = () => {
    setIsAddLineOpen(false);
  };

  const dismissErrors = () => {
    setDismissedErrorKey(errorKey);
    statusMutation.reset();
    addLineMutation.reset();
    removeLineMutation.reset();
    updateLineMutation.reset();
    createReservationMutation.reset();
    releaseReservationMutation.reset();
  };

  const approvePurchaseOrder = () => {
    if (!purchaseOrder) return;
    setChangingStatusAction('approve');
    statusMutation.mutate(
      { purchaseOrderId: purchaseOrder.id, status: 'approve', user: 'Franco Diaz' },
      {
        onSettled: () => setChangingStatusAction(null)
      }
    );
  };

  const closePurchaseOrder = () => {
    if (!purchaseOrder) return;
    setChangingStatusAction('close');
    statusMutation.mutate(
      { purchaseOrderId: purchaseOrder.id, status: 'close', user: 'Franco Diaz' },
      {
        onSettled: () => setChangingStatusAction(null)
      }
    );
  };

  const cancelPurchaseOrder = () => {
    if (!purchaseOrder) return;
    setChangingStatusAction('cancel');
    statusMutation.mutate(
      { purchaseOrderId: purchaseOrder.id, status: 'cancel', user: 'Franco Diaz' },
      {
        onSettled: () => setChangingStatusAction(null)
      }
    );
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

  const updateLine = async (values: EditPurchaseOrderLineFormValues) => {
    if (!purchaseOrder) return;
    if (!editLine) return;
    await updateLineMutation.mutateAsync({
      purchaseOrderId: purchaseOrder.id,
      purchaseOrderLineId: editLine.id,
      quantityOrdered: Number(values.quantityOrdered),
      user: values.user
    });

    setEditLineId(null);
  };

  const removeLine = (purchaseOrderLineId: string, user: string) => {
    if (!purchaseOrder) return;
    setRemovingLineId(purchaseOrderLineId);
    removeLineMutation.mutate(
      {
        purchaseOrderId: purchaseOrder.id,
        purchaseOrderLineId,
        user
      },
      {
        onSettled: () => setRemovingLineId(null)
      }
    );
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

  const releaseReservation = (reservation: ReservationModel, quantity: number) => {
    releaseReservationMutation.mutate({
      stockReservationId: reservation.id,
      quantity,
      user: reservationUser
    });
  };

  if (isPageLoading) return <PageLoadingIndicator />;

  return (
    <section>
      <PageHeader description="Review the full purchase order aggregate and manage its lifecycle." title="Purchase Order Details" />
      <div className="grid gap-4 p-6">
        <ErrorSummary messages={visibleErrorMessages} onDismiss={dismissErrors} />
        {!purchaseOrder && !purchaseOrderQuery.isLoading && !purchaseOrderQuery.isError && <EmptyState title="Purchase order was not found." />}
        {purchaseOrder && (
          <PurchaseOrderHeaderCard
            changingStatusAction={changingStatusAction}
            isChangingStatus={statusMutation.isPending}
            onApprove={approvePurchaseOrder}
            onCancel={cancelPurchaseOrder}
            onClose={closePurchaseOrder}
            purchaseOrder={purchaseOrder}
            warehouseDisplayName={warehouse?.displayName ?? purchaseOrder.warehouseId}
          />
        )}
        {purchaseOrder && <PurchaseOrderTotalsBar gstAmount={purchaseOrder.gstAmount} subtotalAmount={purchaseOrder.subtotalAmount} totalAmount={purchaseOrder.totalAmount} />}
        {purchaseOrder && (
          <PurchaseOrderLinesTable
            activeReservations={activeReservations}
            availableItemCount={availableItemsToAdd.length}
            canChangeLines={canChangeLines}
            canReserveStock={canReserveStock}
            inventoryItems={itemsQuery.data}
            isAddingLine={addLineMutation.isPending}
            isRemovingLine={removeLineMutation.isPending}
            onAddLine={() => setIsAddLineOpen(true)}
            onEditLine={setEditLineId}
            onManageReservations={setManageReservationsLineId}
            onRemoveLine={removeLine}
            purchaseOrder={purchaseOrder}
            reservationUser={reservationUser}
            removingLineId={removingLineId}
            stockByItemId={stockByItemId}
          />
        )}
      </div>
      {isAddLineOpen && <AddPurchaseOrderLineDialog inventoryItems={availableItemsToAdd} isSaving={addLineMutation.isPending} onCancel={closeAddLineDialog} onSubmit={addLine} />}
      {editLine && (
        <EditPurchaseOrderLineDialog
          isSaving={updateLineMutation.isPending}
          itemName={editLineItem?.displayName ?? editLine.inventoryItemId}
          onCancel={() => setEditLineId(null)}
          onSubmit={updateLine}
          quantityOrdered={editLine.quantityOrdered}
          quantityReserved={editLine.quantityReserved}
          trackingMode={editLineItem?.trackingMode}
        />
      )}
      {manageReservationsLine && (
        <ManageReservationsDialog
          availableQuantity={manageReservationsStock?.availableQuantity ?? null}
          isReleasing={releaseReservationMutation.isPending}
          isReserving={createReservationMutation.isPending}
          itemName={manageReservationsItem?.displayName ?? manageReservationsLine.inventoryItemId}
          trackingMode={manageReservationsItem?.trackingMode}
          line={manageReservationsLine}
          maxReserveQuantity={manageReservationsMaxQuantity}
          onCancel={() => setManageReservationsLineId(null)}
          onRelease={releaseReservation}
          onReserve={reserveLine}
          onUserChange={setReservationUser}
          reservations={manageReservations}
          user={reservationUser}
        />
      )}
    </section>
  );
};
