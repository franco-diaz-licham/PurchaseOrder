import { UilPen, UilPlus, UilTrash } from '@iconscout/react-unicons';
import { AppButton } from '@/components/ui/AppButton';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
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
  removingLineId: string | null;
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
  removingLineId,
  purchaseOrder,
  reservationUser,
  stockByItemId,
  onAddLine,
  onEditLine,
  onManageReservations,
  onRemoveLine
}: PurchaseOrderLinesTableProps) => {
  const isReadOnly = purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled';
  const showReservationActions = !isReadOnly;
  const reservationMessage = purchaseOrder.status === 'Pending' ? 'Reservations are available after the purchase order is approved.' : 'This purchase order is read-only. Lines and reservations can no longer be changed.';

  return (
    <article className="rounded-md border bg-card">
      <div className="grid gap-3 p-4">
        {!canReserveStock && <div className="rounded-md border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">{reservationMessage}</div>}
        <AppTableContainer bordered maxHeight="calc(100vh - 21rem)">
          <AppTable minWidth="73.75rem">
            <AppTableHead sticky>
              <AppTableHeaderRow className="border-b bg-card text-sm">
                <AppTableHeaderCell className="font-semibold" colSpan={canChangeLines ? 7 : 6}>
                  <div className="flex items-center justify-between gap-3">
                    <span>Purchase order lines</span>
                    {canChangeLines && (
                      <AppButton disabled={isAddingLine || availableItemCount === 0} onClick={onAddLine} type="button">
                        <UilPlus className="h-4 w-4" />
                        Add item
                      </AppButton>
                    )}
                  </div>
                </AppTableHeaderCell>
                <AppTableHeaderCell className="border-l font-semibold" colSpan={showReservationActions ? 3 : 2}>
                  Reservations
                </AppTableHeaderCell>
              </AppTableHeaderRow>
              <AppTableHeaderRow className="bg-muted text-xs uppercase text-muted-foreground">
                <AppTableHeaderCell>Item</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Ordered</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Reserved</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Remaining</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Unit cost</AppTableHeaderCell>
                <AppTableHeaderCell align="right">Total Amount</AppTableHeaderCell>
                {canChangeLines && <AppTableHeaderCell align="right" ariaLabel="Line actions" />}
                <AppTableHeaderCell align="right" className="border-l">
                  Available
                </AppTableHeaderCell>
                <AppTableHeaderCell align="right">Active</AppTableHeaderCell>
                {showReservationActions && <AppTableHeaderCell align="right" ariaLabel="Reservation actions" />}
              </AppTableHeaderRow>
            </AppTableHead>
            <AppTableBody>
              {purchaseOrder.lines.map((line) => {
                const item = findInventoryItem(inventoryItems, line.inventoryItemId);
                const stock = stockByItemId.get(line.inventoryItemId);
                const lineReservations = activeReservations.filter((reservation) => reservation.purchaseOrderLineId === line.id);
                const isLineRemoving = isRemovingLine && removingLineId === line.id;
                return (
                  <AppTableRow key={line.id}>
                    <AppTableCell className="font-medium">{item?.displayName ?? line.inventoryItemId}</AppTableCell>
                    <AppTableCell align="right">{line.quantityOrdered}</AppTableCell>
                    <AppTableCell align="right">{line.quantityReserved}</AppTableCell>
                    <AppTableCell align="right">{line.quantityRemaining}</AppTableCell>
                    <AppTableCell align="right">{formatMoney(line.unitCost)}</AppTableCell>
                    <AppTableCell align="right">{formatMoney(line.lineAmount)}</AppTableCell>
                    {canChangeLines && (
                      <AppTableCell align="right">
                        <div className="flex items-center justify-end gap-2">
                          <AppButton aria-label="Edit line" appearance="secondary" className="h-8 w-8 px-0" onClick={() => onEditLine(line.id)} title="Edit line">
                            <UilPen className="h-4 w-4 text-blue-700" />
                          </AppButton>
                          <AppButton aria-label="Remove line" appearance="secondary" className="h-8 w-8 px-0" disabled={isRemovingLine} isLoading={isLineRemoving} onClick={() => onRemoveLine(line.id, reservationUser)} title="Remove line">
                            <UilTrash className="h-4 w-4 text-red-700" />
                          </AppButton>
                        </div>
                      </AppTableCell>
                    )}
                    <AppTableCell align="right" className="border-l">
                      {stock ? stock.availableQuantity : 'Not stocked'}
                    </AppTableCell>
                    <AppTableCell align="right">
                      {lineReservations.length === 0 && <span className="text-muted-foreground">None</span>}
                      {lineReservations.length > 0 && <span>{lineReservations.length}</span>}
                    </AppTableCell>
                    {showReservationActions && (
                      <AppTableCell align="right">
                        <AppButton aria-label="Manage reservations" appearance="secondary" className="h-8 w-8 px-0" disabled={!canReserveStock} onClick={() => onManageReservations(line.id)} title="Manage reservations">
                          <UilPen className="h-4 w-4 text-blue-700" />
                        </AppButton>
                      </AppTableCell>
                    )}
                  </AppTableRow>
                );
              })}
            </AppTableBody>
          </AppTable>
        </AppTableContainer>
      </div>
    </article>
  );
};
