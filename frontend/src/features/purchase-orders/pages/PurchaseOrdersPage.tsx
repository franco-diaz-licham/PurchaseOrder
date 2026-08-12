import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageLoadingIndicator } from '@/components/common/PageLoadingIndicator';
import { PageHeader } from '@/components/common/PageHeader';
import { useWarehousesQuery } from '@/features/catalog/queries/catalog.queries';
import { CreatePurchaseOrderDialog, type CreatePurchaseOrderFormValues } from '../components/CreatePurchaseOrderDialog';
import { PurchaseOrderListActions } from '../components/PurchaseOrderListActions';
import { PurchaseOrderSummaryTable } from '../components/PurchaseOrderSummaryTable';
import { usePurchaseOrderSummariesQuery, useSubmitPurchaseOrderMutation } from '../queries/purchaseOrder.queries';
import { usePurchaseOrderListStore } from '../stores/purchaseOrderList.store';
import { filterPurchaseOrders } from '../utils/purchaseOrderFilters';

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
    return filterPurchaseOrders(purchaseOrdersQuery.data ?? [], {
      warehouseId: warehouseFilter,
      showReadyToReserveOnly
    });
  }, [purchaseOrdersQuery.data, showReadyToReserveOnly, warehouseFilter]);
  const isLoading = purchaseOrdersQuery.isLoading || warehousesQuery.isLoading;

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

  if (isLoading) return <PageLoadingIndicator />;

  return (
    <section>
      <PageHeader description="Review purchase order summaries and open a record to manage its lines." title="Purchase Orders">
        <PurchaseOrderListActions
          onAdd={() => setIsCreateOpen(true)}
          onShowReadyToReserveOnlyChange={setShowReadyToReserveOnly}
          onWarehouseFilterChange={setWarehouseFilter}
          showReadyToReserveOnly={showReadyToReserveOnly}
          warehouseFilter={warehouseFilter}
          warehouses={warehousesQuery.data ?? []}
        />
      </PageHeader>
      <PurchaseOrderSummaryTable isError={purchaseOrdersQuery.isError} isLoading={purchaseOrdersQuery.isLoading} onOpenPurchaseOrder={(id) => navigate(`/purchase-orders/${id}`)} purchaseOrders={purchaseOrders} warehouses={warehousesQuery.data} />
      {isCreateOpen && <CreatePurchaseOrderDialog isError={submitMutation.isError} isSaving={submitMutation.isPending} onCancel={closeCreateDialog} onSubmit={createPurchaseOrder} warehouses={warehousesQuery.data ?? []} />}
    </section>
  );
};
