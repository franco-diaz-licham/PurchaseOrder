import { UilPen, UilPlus, UilTrash } from '@iconscout/react-unicons';
import { AppButton } from '@/components/ui/AppButton';
import type { InventoryItemModel, WarehouseStockModel } from '@/features/catalog/types/catalog.types';
import { findInventoryItem } from '@/features/catalog/utils/catalogLookup';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import { formatMoney } from '@/lib/formatMoney';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';

type PurchaseOrderLinesTableProps = {
  activeReservations: ReservationModel[];
  availableItemCount: number;
  canChangeLines: boolean;
  canReserveStock: boolean;
  inventoryItems?: InventoryItemModel[];
  isAddingLine: boolean;
  isRemovingLine: boolean;
  purchaseOrder: PurchaseOrderModel;
  reservationUser: string;
  stockByItemId: Map<string, WarehouseStockModel>;
  onAddLine: () => void;
  onEditLine: (purchaseOrderLineId: string) => void;
  onManageReservations: (purchaseOrderLineId: string) => void;
  onRemoveLine: (purchaseOrderLineId: string, user: string) => void;
};

export const PurchaseOrderLinesTable = ({
  activeReservations,
  availableItemCount,
  canChangeLines,
  canReserveStock,
  inventoryItems,
  isAddingLine,
  isRemovingLine,
  purchaseOrder,
  reservationUser,
  stockByItemId,
  onAddLine,
  onEditLine,
  onManageReservations,
  onRemoveLine
}: PurchaseOrderLinesTableProps) => (
  <article className="rounded-md border bg-card">
    <div className="grid gap-3 p-4">
      {!canReserveStock && <div className="rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">Reservations are available after the purchase order is approved.</div>}
      <div className="overflow-x-auto rounded-md border">
        <table className="w-full min-w-295 text-left text-sm">
          <thead>
            <tr className="border-b bg-card text-sm">
              <th className="px-4 py-3 font-semibold" colSpan={canChangeLines ? 7 : 6}>
                <div className="flex items-center justify-between gap-3">
                  <span>Purchase order lines</span>
                  {canChangeLines && (
                    <AppButton disabled={isAddingLine || availableItemCount === 0} onClick={onAddLine} type="button">
                      <UilPlus className="h-4 w-4" />
                      Add line
                    </AppButton>
                  )}
                </div>
              </th>
              <th className="border-l px-4 py-3 font-semibold" colSpan={3}>
                Reservations
              </th>
            </tr>
            <tr className="bg-muted text-xs uppercase text-muted-foreground">
              <th className="px-4 py-3">Item</th>
              <th className="px-4 py-3 text-right">Ordered</th>
              <th className="px-4 py-3 text-right">Reserved</th>
              <th className="px-4 py-3 text-right">Remaining</th>
              <th className="px-4 py-3 text-right">Unit cost</th>
              <th className="px-4 py-3 text-right">Total Amount</th>
              {canChangeLines && <th className="px-4 py-3 text-right" aria-label="Line actions" />}
              <th className="border-l px-4 py-3 text-right">Available</th>
              <th className="px-4 py-3 text-right">Active</th>
              <th className="px-4 py-3 text-right" aria-label="Reservation actions" />
            </tr>
          </thead>
          <tbody>
            {purchaseOrder.lines.map((line) => {
              const item = findInventoryItem(inventoryItems, line.inventoryItemId);
              const stock = stockByItemId.get(line.inventoryItemId);
              const lineReservations = activeReservations.filter((reservation) => reservation.purchaseOrderLineId === line.id);
              return (
                <tr className="border-t" key={line.id}>
                  <td className="px-4 py-3 font-medium">{item?.displayName ?? line.inventoryItemId}</td>
                  <td className="px-4 py-3 text-right">{line.quantityOrdered}</td>
                  <td className="px-4 py-3 text-right">{line.quantityReserved}</td>
                  <td className="px-4 py-3 text-right">{line.quantityRemaining}</td>
                  <td className="px-4 py-3 text-right">{formatMoney(line.unitCost)}</td>
                  <td className="px-4 py-3 text-right">{formatMoney(line.lineAmount)}</td>
                  {canChangeLines && (
                    <td className="px-4 py-3 text-right">
                      <div className="flex items-center justify-end gap-2">
                        <AppButton aria-label="Edit line" appearance="secondary" className="h-8 w-8 px-0" onClick={() => onEditLine(line.id)} title="Edit line">
                          <UilPen className="h-4 w-4 text-blue-700" />
                        </AppButton>
                        <AppButton aria-label="Remove line" appearance="secondary" className="h-8 w-8 px-0" disabled={isRemovingLine} onClick={() => onRemoveLine(line.id, reservationUser)} title="Remove line">
                          <UilTrash className="h-4 w-4 text-red-700" />
                        </AppButton>
                      </div>
                    </td>
                  )}
                  <td className="border-l px-4 py-3 text-right">{stock ? stock.availableQuantity : 'Not stocked'}</td>
                  <td className="px-4 py-3 text-right">
                    {lineReservations.length === 0 && <span className="text-muted-foreground">None</span>}
                    {lineReservations.length > 0 && <span>{lineReservations.length}</span>}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <AppButton aria-label="Manage reservations" appearance="secondary" className="h-8 w-8 px-0" disabled={!canReserveStock} onClick={() => onManageReservations(line.id)} title="Manage reservations">
                      <UilPen className="h-4 w-4 text-blue-700" />
                    </AppButton>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </div>
  </article>
);
