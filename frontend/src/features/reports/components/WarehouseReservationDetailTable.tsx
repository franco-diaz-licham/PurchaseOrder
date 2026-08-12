import { AppButton } from '@/components/ui/AppButton';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
import { formatMoney } from '@/lib/formatMoney';
import type { WarehouseCommittedValueModel } from '../types/finance.types';

type WarehouseReservationDetailTableProps = {
  warehouse: WarehouseCommittedValueModel;
  onClose: () => void;
  onOpenPurchaseOrder: (purchaseOrderId: string, warehouseId: string) => void;
};

export const WarehouseReservationDetailTable = ({ warehouse, onClose, onOpenPurchaseOrder }: WarehouseReservationDetailTableProps) => (
  <div className="rounded-md border border-primary/30 bg-card shadow-sm">
    <div className="flex flex-col gap-2 border-b bg-muted/30 p-4 md:flex-row md:items-center md:justify-between">
      <div>
        <h2 className="text-base font-semibold">{warehouse.warehouseDisplayName}</h2>
        <p className="mt-1 text-sm text-muted-foreground">Reservation detail uses the unit cost captured when each reservation was created.</p>
      </div>
      <AppButton appearance="secondary" onClick={onClose}>
        Close
      </AppButton>
    </div>
    <AppTableContainer maxHeight="18rem">
      <AppTable minWidth="51.25rem">
        <AppTableHead sticky>
          <AppTableHeaderRow>
            <AppTableHeaderCell>PO number</AppTableHeaderCell>
            <AppTableHeaderCell>Item</AppTableHeaderCell>
            <AppTableHeaderCell align="right">Qty reserved</AppTableHeaderCell>
            <AppTableHeaderCell align="right">Unit cost at reservation</AppTableHeaderCell>
            <AppTableHeaderCell align="right">Committed value</AppTableHeaderCell>
          </AppTableHeaderRow>
        </AppTableHead>
        <AppTableBody>
          {warehouse.reservations.map((reservation) => (
            <AppTableRow key={reservation.stockReservationId}>
              <AppTableCell>
                <AppButton appearance="link" className="font-semibold" type="button" onClick={() => onOpenPurchaseOrder(reservation.purchaseOrderId, warehouse.warehouseId)}>
                  {reservation.purchaseOrderNumber}
                </AppButton>
              </AppTableCell>
              <AppTableCell className="font-medium">{reservation.itemDisplayName}</AppTableCell>
              <AppTableCell align="right">{reservation.quantityReserved}</AppTableCell>
              <AppTableCell align="right">{formatMoney(reservation.unitCostSnapshot)}</AppTableCell>
              <AppTableCell align="right" className="font-semibold">
                {formatMoney(reservation.committedValue)}
              </AppTableCell>
            </AppTableRow>
          ))}
        </AppTableBody>
      </AppTable>
    </AppTableContainer>
  </div>
);
