import { UilTimes } from '@iconscout/react-unicons';
import { AppButton } from '@/components/ui/AppButton';
import type { InventoryItemModel, WarehouseStockModel } from '@/features/catalog/types/catalog.types';
import { findInventoryItem } from '@/features/catalog/utils/catalogLookup';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import { formatMoney } from '@/lib/formatMoney';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';

type PurchaseOrderLinesTableProps = {
  activeReservations: ReservationModel[];
  canChangeLines: boolean;
  canReserveStock: boolean;
  inventoryItems?: InventoryItemModel[];
  isRemovingLine: boolean;
  purchaseOrder: PurchaseOrderModel;
  reservationUser: string;
  stockByItemId: Map<string, WarehouseStockModel>;
  onManageReservations: (purchaseOrderLineId: string) => void;
  onRemoveLine: (purchaseOrderLineId: string, user: string) => void;
};

export const PurchaseOrderLinesTable = ({ activeReservations, canChangeLines, canReserveStock, inventoryItems, isRemovingLine, purchaseOrder, reservationUser, stockByItemId, onManageReservations, onRemoveLine }: PurchaseOrderLinesTableProps) => (
  <article className="rounded-md border bg-card">
    <div className="p-4">
      <div className="overflow-x-auto rounded-md border">
        <table className={`w-full text-left text-sm ${canReserveStock ? 'min-w-295' : 'min-w-180'}`}>
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
              const item = findInventoryItem(inventoryItems, line.inventoryItemId);
              const stock = stockByItemId.get(line.inventoryItemId);
              const lineReservations = activeReservations.filter((reservation) => reservation.purchaseOrderLineId === line.id);
              return (
                <tr className="border-t" key={line.id}>
                  <td className="px-4 py-3">{item?.displayName ?? line.inventoryItemId}</td>
                  <td className="px-4 py-3">{line.quantityOrdered}</td>
                  <td className="px-4 py-3">{line.quantityReserved}</td>
                  <td className="px-4 py-3">{line.quantityRemaining}</td>
                  <td className="px-4 py-3">{formatMoney(line.unitCost)}</td>
                  <td className="px-4 py-3">{formatMoney(line.lineAmount)}</td>
                  {canChangeLines && (
                    <td className="px-4 py-3">
                      <AppButton appearance="secondary" disabled={isRemovingLine} onClick={() => onRemoveLine(line.id, reservationUser)}>
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
                        <AppButton appearance="secondary" onClick={() => onManageReservations(line.id)}>
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
);
