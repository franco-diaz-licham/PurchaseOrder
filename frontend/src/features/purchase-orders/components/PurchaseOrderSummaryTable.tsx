import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';
import { findWarehouse } from '@/features/catalog/utils/catalogLookup';
import { formatMoney } from '@/lib/formatMoney';
import type { PurchaseOrderSummaryModel } from '../types/purchaseOrder.types';

type PurchaseOrderSummaryTableProps = {
  isError: boolean;
  isLoading: boolean;
  purchaseOrders: PurchaseOrderSummaryModel[];
  warehouses?: WarehouseModel[];
  onOpenPurchaseOrder: (purchaseOrderId: string) => void;
};

export const PurchaseOrderSummaryTable = ({ isError, isLoading, purchaseOrders, warehouses, onOpenPurchaseOrder }: PurchaseOrderSummaryTableProps) => (
  <div className="p-6">
    <div className="rounded-md border bg-card">
      {isError && <ErrorMessage message="Purchase orders could not be loaded." />}
      {purchaseOrders.length === 0 && !isLoading && <EmptyState title="No purchase orders found." />}

      {purchaseOrders.length > 0 && (
        <AppTableContainer maxHeight="calc(100vh - 10.5rem)">
          <AppTable minWidth="56.25rem">
            <AppTableHead sticky>
              <AppTableHeaderRow>
                <AppTableHeaderCell>PO number</AppTableHeaderCell>
                <AppTableHeaderCell>Warehouse</AppTableHeaderCell>
                <AppTableHeaderCell>Status</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Lines</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Ordered</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Reserved</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Remaining</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Total</AppTableHeaderCell>
              </AppTableHeaderRow>
            </AppTableHead>
            <AppTableBody>
              {purchaseOrders.map((order) => {
                const warehouse = findWarehouse(warehouses, order.warehouseId);
                return (
                  <AppTableRow
                    interactive
                    key={order.id}
                    onClick={() => onOpenPurchaseOrder(order.id)}
                    onKeyDown={(event) => {
                      if (event.key === 'Enter') onOpenPurchaseOrder(order.id);
                    }}
                    tabIndex={0}
                  >
                    <AppTableCell className="font-semibold">{order.number}</AppTableCell>
                    <AppTableCell>{warehouse?.displayName ?? order.warehouseId}</AppTableCell>
                    <AppTableCell>
                      <StatusBadge status={order.status} />
                    </AppTableCell>
                    <AppTableCell align="right">{order.lineCount}</AppTableCell>
                    <AppTableCell align="right">{order.quantityOrdered}</AppTableCell>
                    <AppTableCell align="right">{order.quantityReserved}</AppTableCell>
                    <AppTableCell align="right">{order.quantityRemaining}</AppTableCell>
                    <AppTableCell align="right" className="font-semibold">
                      {formatMoney(order.totalAmount)}
                    </AppTableCell>
                  </AppTableRow>
                );
              })}
            </AppTableBody>
          </AppTable>
        </AppTableContainer>
      )}
    </div>
  </div>
);
