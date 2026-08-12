import { AppButton } from '@/components/ui/AppButton';
import { formatMoney } from '@/lib/formatMoney';
import type { WarehouseCommittedValue } from '../types/finance.types';

type WarehouseReservationDetailTableProps = {
  warehouse: WarehouseCommittedValue;
  onClose: () => void;
  onOpenPurchaseOrder: (purchaseOrderId: string, warehouseId: string) => void;
};

export const WarehouseReservationDetailTable = ({ warehouse, onClose, onOpenPurchaseOrder }: WarehouseReservationDetailTableProps) => (
  <div className="rounded-md border bg-card">
    <div className="flex flex-col gap-2 border-b p-4 md:flex-row md:items-center md:justify-between">
      <div>
        <h2 className="text-base font-semibold">{warehouse.warehouseDisplayName}</h2>
        <p className="mt-1 text-sm text-muted-foreground">Reservation detail uses the unit cost captured when each reservation was created.</p>
      </div>
      <AppButton appearance="secondary" onClick={onClose}>
        Close
      </AppButton>
    </div>
    <div className="overflow-x-auto">
      <table className="w-full min-w-205 text-left text-sm">
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
          {warehouse.reservations.map((reservation) => (
            <tr className="border-t" key={reservation.stockReservationId}>
              <td className="px-4 py-3">
                <button className="font-medium text-primary underline-offset-4 hover:underline" type="button" onClick={() => onOpenPurchaseOrder(reservation.purchaseOrderId, warehouse.warehouseId)}>
                  {reservation.purchaseOrderNumber}
                </button>
              </td>
              <td className="px-4 py-3">{reservation.itemDisplayName}</td>
              <td className="px-4 py-3">{reservation.quantityReserved}</td>
              <td className="px-4 py-3">{formatMoney(reservation.unitCostSnapshot)}</td>
              <td className="px-4 py-3 font-semibold">{formatMoney(reservation.committedValue)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  </div>
);
