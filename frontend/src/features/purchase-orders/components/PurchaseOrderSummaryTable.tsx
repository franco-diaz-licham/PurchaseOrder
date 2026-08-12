import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { StatusBadge } from '@/components/common/StatusBadge';
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
        <div className="overflow-x-auto">
          <table className="w-full min-w-225 text-left text-sm">
            <thead className="bg-muted text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-4 py-3">PO number</th>
                <th className="px-4 py-3">Warehouse</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Lines</th>
                <th className="px-4 py-3">Ordered</th>
                <th className="px-4 py-3">Reserved</th>
                <th className="px-4 py-3">Remaining</th>
                <th className="px-4 py-3">Total</th>
              </tr>
            </thead>
            <tbody>
              {purchaseOrders.map((order) => {
                const warehouse = findWarehouse(warehouses, order.warehouseId);
                return (
                  <tr
                    className="cursor-pointer border-t hover:bg-muted/60"
                    key={order.id}
                    onClick={() => onOpenPurchaseOrder(order.id)}
                    onKeyDown={(event) => {
                      if (event.key === 'Enter') onOpenPurchaseOrder(order.id);
                    }}
                    tabIndex={0}
                  >
                    <td className="px-4 py-3 font-semibold">{order.number}</td>
                    <td className="px-4 py-3">{warehouse?.displayName ?? order.warehouseId}</td>
                    <td className="px-4 py-3">
                      <StatusBadge status={order.status} />
                    </td>
                    <td className="px-4 py-3">{order.lineCount}</td>
                    <td className="px-4 py-3">{order.quantityOrdered}</td>
                    <td className="px-4 py-3">{order.quantityReserved}</td>
                    <td className="px-4 py-3">{order.quantityRemaining}</td>
                    <td className="px-4 py-3 font-semibold">{formatMoney(order.totalAmount)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  </div>
);
