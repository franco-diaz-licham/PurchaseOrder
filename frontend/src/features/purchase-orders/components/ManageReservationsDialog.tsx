import { UilTimes } from '@iconscout/react-unicons';
import { useState } from 'react';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import type { Reservation } from '@/features/reservations/types/reservation.types';
import { formatMoney } from '@/lib/formatMoney';
import type { PurchaseOrderLine } from '../types/purchaseOrder.types';

const dateTime = new Intl.DateTimeFormat('en-AU', {
  dateStyle: 'short',
  timeStyle: 'short'
});

type ManageReservationsDialogProps = {
  availableQuantity: number | null;
  isReleasing: boolean;
  isReserving: boolean;
  itemName: string;
  trackingMode: string | undefined;
  line: PurchaseOrderLine;
  maxReserveQuantity: number;
  reservations: Reservation[];
  user: string;
  onCancel: () => void;
  onRelease: (reservation: Reservation, quantity: number) => void;
  onReserve: (quantity: number, user: string) => Promise<void>;
  onUserChange: (user: string) => void;
};

export const ManageReservationsDialog = ({ availableQuantity, isReleasing, isReserving, itemName, trackingMode, line, maxReserveQuantity, reservations, user, onCancel, onRelease, onReserve, onUserChange }: ManageReservationsDialogProps) => {
  const isWeightTracked = trackingMode === 'Weight';
  const quantityLabel = isWeightTracked ? 'Weight to reserve (kg)' : 'Quantity to reserve';
  const releaseLabel = isWeightTracked ? 'Weight to release (kg)' : 'Quantity to release';
  const quantityUnit = isWeightTracked ? 'kg' : 'units';
  const quantityStep = isWeightTracked ? '0.001' : '1';
  const minimumQuantity = isWeightTracked ? '0.001' : '1';
  const [quantity, setQuantity] = useState('');
  const [releaseQuantities, setReleaseQuantities] = useState<Record<string, string>>({});
  const parsedQuantity = Number(quantity || 0);
  const canReserve = !isReserving && parsedQuantity > 0 && parsedQuantity <= maxReserveQuantity && user.trim().length > 0;

  const reserve = async () => {
    await onReserve(parsedQuantity, user);
    setQuantity('');
  };

  const release = (reservation: Reservation) => {
    const quantityToRelease = Number(releaseQuantities[reservation.id] || 0);
    onRelease(reservation, quantityToRelease);
  };

  const canRelease = (reservation: Reservation) => {
    const quantityToRelease = Number(releaseQuantities[reservation.id] || 0);
    return !isReleasing && user.trim().length > 0 && quantityToRelease > 0 && quantityToRelease <= reservation.quantityReserved;
  };

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="manage-reservations-title">
      <div className="w-full max-w-4xl rounded-md border bg-card shadow-lg">
        <div className="flex items-center justify-between border-b p-4">
          <div>
            <h2 className="text-base font-semibold" id="manage-reservations-title">
              Manage reservations
            </h2>
            <p className="mt-1 text-sm text-muted-foreground">
              {itemName} - Available {availableQuantity ?? 'Not stocked'} {quantityUnit} - Remaining {line.quantityRemaining} {quantityUnit}
            </p>
          </div>
          <AppButton appearance="ghost" onClick={onCancel} type="button">
            <UilTimes className="h-4 w-4" />
          </AppButton>
        </div>

        <div className="grid gap-4 p-4">
          <div className="grid gap-3 rounded-md border p-4 md:grid-cols-[1fr_1fr_auto] md:items-end">
            <AppField label={quantityLabel}>
              <AppInput autoFocus max={maxReserveQuantity} min={minimumQuantity} onChange={(event) => setQuantity(event.target.value)} required step={quantityStep} type="number" value={quantity} />
            </AppField>

            <AppField label="User">
              <AppInput onChange={(event) => onUserChange(event.target.value)} required value={user} />
            </AppField>

            <AppButton disabled={!canReserve} onClick={() => void reserve()} type="button">
              Reserve
            </AppButton>
          </div>

          <div className="overflow-hidden rounded-md border">
            <div className="border-b bg-card px-4 py-3 text-sm font-semibold">Active reservation records</div>
            <table className="w-full text-left text-sm">
              <thead className="bg-muted text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-4 py-3">Active reservation</th>
                  <th className="px-4 py-3">Unit cost</th>
                  <th className="px-4 py-3">Reserved by</th>
                  <th className="px-4 py-3">Reserved at</th>
                  <th className="px-4 py-3">{releaseLabel}</th>
                  <th className="px-4 py-3">Action</th>
                </tr>
              </thead>
              <tbody>
                {reservations.length === 0 && (
                  <tr className="border-t">
                    <td className="px-4 py-3 text-muted-foreground" colSpan={6}>
                      No active reservations.
                    </td>
                  </tr>
                )}
                {reservations.map((reservation) => (
                  <tr className="border-t" key={reservation.id}>
                    <td className="px-4 py-3">
                      {reservation.quantityReserved} {quantityUnit}
                    </td>
                    <td className="px-4 py-3">{formatMoney(reservation.unitCostSnapshot)}</td>
                    <td className="px-4 py-3">{reservation.reservedBy}</td>
                    <td className="px-4 py-3">{dateTime.format(reservation.reservedAt)}</td>
                    <td className="px-4 py-3">
                      <AppInput
                        className="w-32"
                        max={reservation.quantityReserved}
                        min={minimumQuantity}
                        onChange={(event) =>
                          setReleaseQuantities((current) => ({
                            ...current,
                            [reservation.id]: event.target.value
                          }))
                        }
                        step={quantityStep}
                        type="number"
                        value={releaseQuantities[reservation.id] ?? ''}
                      />
                    </td>
                    <td className="px-4 py-3">
                      <AppButton appearance="secondary" disabled={!canRelease(reservation)} onClick={() => release(reservation)} type="button">
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
