import { UilTimes } from '@iconscout/react-unicons';
import { useState } from 'react';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import type { Reservation } from '@/features/reservations/types/reservation.types';
import type { PurchaseOrderLine } from '../types/purchaseOrder.types';

const money = new Intl.NumberFormat('en-AU', {
  style: 'currency',
  currency: 'AUD'
});

type ManageReservationsDialogProps = {
  availableQuantity: number | null;
  isReleasing: boolean;
  isReserving: boolean;
  itemName: string;
  line: PurchaseOrderLine;
  maxReserveQuantity: number;
  reservations: Reservation[];
  user: string;
  onCancel: () => void;
  onRelease: (reservation: Reservation) => void;
  onReserve: (quantity: number, user: string) => Promise<void>;
  onUserChange: (user: string) => void;
};

export const ManageReservationsDialog = ({ availableQuantity, isReleasing, isReserving, itemName, line, maxReserveQuantity, reservations, user, onCancel, onRelease, onReserve, onUserChange }: ManageReservationsDialogProps) => {
  const [quantity, setQuantity] = useState('');
  const parsedQuantity = Number(quantity || 0);
  const canReserve = !isReserving && parsedQuantity > 0 && parsedQuantity <= maxReserveQuantity && user.trim().length > 0;

  const reserve = async () => {
    await onReserve(parsedQuantity, user);
    setQuantity('');
  };

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="manage-reservations-title">
      <div className="w-full max-w-xl rounded-md border bg-card shadow-lg">
        <div className="flex items-center justify-between border-b p-4">
          <div>
            <h2 className="text-base font-semibold" id="manage-reservations-title">
              Manage reservations
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {itemName} - Available {availableQuantity ?? 'Not stocked'} - Remaining {line.quantityRemaining}
            </p>
          </div>
          <AppButton appearance="ghost" onClick={onCancel} type="button">
            <UilTimes className="h-4 w-4" />
          </AppButton>
        </div>

        <div className="grid gap-4 p-4">
          <div className="grid gap-3 rounded-md border p-4 md:grid-cols-[1fr_1fr_auto] md:items-end">
            <AppField label="Quantity">
              <AppInput autoFocus max={maxReserveQuantity} min="0.001" onChange={(event) => setQuantity(event.target.value)} required step="0.001" type="number" value={quantity} />
            </AppField>

            <AppField label="User">
              <AppInput onChange={(event) => onUserChange(event.target.value)} required value={user} />
            </AppField>

            <AppButton disabled={!canReserve} onClick={() => void reserve()} type="button">
              Reserve
            </AppButton>
          </div>

          <div className="overflow-hidden rounded-md border">
            <table className="w-full text-left text-sm">
              <thead className="bg-muted text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-4 py-3">Quantity</th>
                  <th className="px-4 py-3">Unit cost</th>
                  <th className="px-4 py-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {reservations.length === 0 && (
                  <tr className="border-t">
                    <td className="px-4 py-3 text-muted-foreground" colSpan={3}>
                      No active reservations.
                    </td>
                  </tr>
                )}
                {reservations.map((reservation) => (
                  <tr className="border-t" key={reservation.id}>
                    <td className="px-4 py-3">{reservation.quantityReserved}</td>
                    <td className="px-4 py-3">{money.format(reservation.unitCostSnapshot)}</td>
                    <td className="px-4 py-3">
                      <AppButton appearance="secondary" disabled={isReleasing || user.trim().length === 0} onClick={() => onRelease(reservation)} type="button">
                        Release
                      </AppButton>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        <div className="flex justify-end border-t p-4">
          <AppButton appearance="secondary" onClick={onCancel} type="button">
            Close
          </AppButton>
        </div>
      </div>
    </div>
  );
};
