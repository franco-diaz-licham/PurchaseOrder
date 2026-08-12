import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { PageLoadingIndicator } from '@/components/common/PageLoadingIndicator';
import { PageHeader } from '@/components/common/PageHeader';
import { FinanceStatsHeader } from '../components/FinanceStatsHeader';
import { WarehouseCommittedValueTable } from '../components/WarehouseCommittedValueTable';
import { WarehouseReservationDetailTable } from '../components/WarehouseReservationDetailTable';
import { useWarehouseCommittedValuesQuery } from '../queries/finance.queries';
import { useFinanceNavigationStore } from '../stores/financeNavigation.store';

export const FinancePage = () => {
  const navigate = useNavigate();
  const financeQuery = useWarehouseCommittedValuesQuery();
  const selectedWarehouseId = useFinanceNavigationStore((state) => state.selectedWarehouseId);
  const setSelectedWarehouseId = useFinanceNavigationStore((state) => state.setSelectedWarehouseId);
  const openPurchaseOrder = useFinanceNavigationStore((state) => state.openPurchaseOrder);
  const reportLoadedAt = useMemo(() => new Date().toLocaleString(), []);
  const totalCommittedValue = (financeQuery.data ?? []).reduce((total, row) => total + row.committedValue, 0);
  const totalReservedQuantity = (financeQuery.data ?? []).reduce((total, row) => total + row.reservedQuantity, 0);
  const totalReservationCount = (financeQuery.data ?? []).reduce((total, row) => total + row.reservationCount, 0);
  const selectedWarehouse = (financeQuery.data ?? []).find((row) => row.warehouseId === selectedWarehouseId) ?? null;

  const openPurchaseOrderDetail = (purchaseOrderId: string, warehouseId: string) => {
    openPurchaseOrder(purchaseOrderId, warehouseId);
    navigate(`/purchase-orders/${purchaseOrderId}`);
  };

  if (financeQuery.isLoading) return <PageLoadingIndicator />;

  return (
    <section>
      <PageHeader description="Committed value is calculated from active reservations using the standard cost captured at reservation time." title="Finance" />
      <div className="grid gap-4 p-6">
        <FinanceStatsHeader reportLoadedAt={reportLoadedAt} totalCommittedValue={totalCommittedValue} totalReservationCount={totalReservationCount} totalReservedQuantity={totalReservedQuantity} />
        <WarehouseCommittedValueTable isError={financeQuery.isError} isLoading={financeQuery.isLoading} onViewWarehouse={setSelectedWarehouseId} rows={financeQuery.data ?? []} />
        {selectedWarehouse && <WarehouseReservationDetailTable onClose={() => setSelectedWarehouseId(null)} onOpenPurchaseOrder={openPurchaseOrderDetail} warehouse={selectedWarehouse} />}
      </div>
    </section>
  );
};
