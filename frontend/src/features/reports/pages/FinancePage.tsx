import { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { AppButton } from '@/components/ui/AppButton';
import { useWarehouseCommittedValuesQuery } from '../queries/finance.queries';
import { useFinanceNavigationStore } from '../stores/financeNavigation.store';

const money = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD'
});

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

  return (
    <section>
      <PageHeader description="Committed value is calculated from active reservations using the standard cost captured at reservation time." title="Finance" />

      <div className="grid gap-4 p-6">
        <div className="grid gap-4 md:grid-cols-3">
          <div className="rounded-md border bg-card p-4">
            <div className="text-sm text-muted-foreground">Total committed value</div>
            <div className="mt-1 text-2xl font-semibold">{money.format(totalCommittedValue)}</div>
          </div>
          <div className="rounded-md border bg-card p-4">
            <div className="text-sm text-muted-foreground">Reserved quantity</div>
            <div className="mt-1 text-2xl font-semibold">{totalReservedQuantity}</div>
          </div>
          <div className="rounded-md border bg-card p-4">
            <div className="text-sm text-muted-foreground">Active reservations</div>
            <div className="mt-1 text-2xl font-semibold">{totalReservationCount}</div>
            <div className="mt-1 text-xs text-muted-foreground">Loaded {reportLoadedAt}</div>
          </div>
        </div>

        <div className="rounded-md border bg-card">
          {financeQuery.isError && <ErrorMessage message="Finance values could not be loaded." />}
          {(financeQuery.data ?? []).length === 0 && !financeQuery.isLoading && <EmptyState title="No committed reservation value found." />}
          {(financeQuery.data ?? []).length > 0 && (
            <table className="w-full text-left text-sm">
              <thead className="bg-muted text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-4 py-3">Warehouse</th>
                  <th className="px-4 py-3">Reserved qty</th>
                  <th className="px-4 py-3">Reservations</th>
                  <th className="px-4 py-3">Committed value</th>
                  <th className="px-4 py-3">Details</th>
                </tr>
              </thead>
              <tbody>
                {(financeQuery.data ?? []).map((row) => (
                  <tr className="border-t" key={row.warehouseId}>
                    <td className="px-4 py-3">{row.warehouseDisplayName}</td>
                    <td className="px-4 py-3">{row.reservedQuantity}</td>
                    <td className="px-4 py-3">{row.reservationCount}</td>
                    <td className="px-4 py-3 font-semibold">{money.format(row.committedValue)}</td>
                    <td className="px-4 py-3">
                      <AppButton appearance="secondary" disabled={row.reservationCount === 0} onClick={() => setSelectedWarehouseId(row.warehouseId)}>
                        View
                      </AppButton>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {selectedWarehouse && (
          <div className="rounded-md border bg-card">
            <div className="flex flex-col gap-2 border-b p-4 md:flex-row md:items-center md:justify-between">
              <div>
                <h2 className="text-base font-semibold">{selectedWarehouse.warehouseDisplayName}</h2>
                <p className="mt-1 text-sm text-muted-foreground">Reservation detail uses the unit cost captured when each reservation was created.</p>
              </div>
              <AppButton appearance="secondary" onClick={() => setSelectedWarehouseId(null)}>
                Close
              </AppButton>
            </div>
            <div className="overflow-x-auto">
              <table className="w-full min-w-[820px] text-left text-sm">
                <thead className="bg-muted text-xs uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3">PO number</th>
                    <th className="px-4 py-3">Item</th>
                    <th className="px-4 py-3">Qty reserved</th>
                    <th className="px-4 py-3">Unit cost at reservation</th>
                    <th className="px-4 py-3">Committed value</th>
                  </tr>
                </thead>
                <tbody>
                  {selectedWarehouse.reservations.map((reservation) => (
                    <tr className="border-t" key={reservation.stockReservationId}>
                      <td className="px-4 py-3">
                        <button className="font-medium text-primary underline-offset-4 hover:underline" type="button" onClick={() => openPurchaseOrderDetail(reservation.purchaseOrderId, selectedWarehouse.warehouseId)}>
                          {reservation.purchaseOrderNumber}
                        </button>
                      </td>
                      <td className="px-4 py-3">{reservation.itemDisplayName}</td>
                      <td className="px-4 py-3">{reservation.quantityReserved}</td>
                      <td className="px-4 py-3">{money.format(reservation.unitCostSnapshot)}</td>
                      <td className="px-4 py-3 font-semibold">{money.format(reservation.committedValue)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}
      </div>
    </section>
  );
};
