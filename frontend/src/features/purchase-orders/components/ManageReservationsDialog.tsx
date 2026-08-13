import { UilTimes } from '@iconscout/react-unicons';
import { useState } from 'react';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
import type { ReservationModel } from '@/features/reservations/types/reservation.types';
import { formatMoney } from '@/lib/formatMoney';
import type { InventoryTrackingMode } from '@/features/catalog/types/catalog.types';
import type { PurchaseOrderLineModel } from '../types/purchaseOrder.types';

const dateTime = new Intl.DateTimeFormat('en-AU', {
  dateStyle: 'short',
  timeStyle: 'short'
});

type ManageReservationsDialogProps = {
  availableQuantity: number | null;
  isReleasing: boolean;
  isReserving: boolean;
  itemName: string;
  trackingMode: InventoryTrackingMode | undefined;
  line: PurchaseOrderLineModel;
  maxReserveQuantity: number;
  reservations: ReservationModel[];
  user: string;
  onCancel: () => void;
  onRelease: (reservation: ReservationModel, quantity: number) => void;
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

  const release = (reservation: ReservationModel) => {
    const quantityToRelease = Number(releaseQuantities[reservation.id] || 0);
    onRelease(reservation, quantityToRelease);
  };

  const canRelease = (reservation: ReservationModel) => {
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
            <AppButton disabled={!canReserve} isLoading={isReserving} onClick={() => void reserve()} type="button">
              Reserve
            </AppButton>
          </div>

          <div className="overflow-hidden rounded-md border">
            <div className="border-b bg-card px-4 py-3 text-sm font-semibold">Active reservation records</div>
            <AppTableContainer maxHeight="18rem">
              <AppTable>
                <AppTableHead sticky>
                  <AppTableHeaderRow>
                    <AppTableHeaderCell align="right">Active reservation</AppTableHeaderCell>
                    <AppTableHeaderCell align="right">Unit cost</AppTableHeaderCell>
                    <AppTableHeaderCell>Reserved by</AppTableHeaderCell>
                    <AppTableHeaderCell>Reserved at</AppTableHeaderCell>
                    <AppTableHeaderCell align="right">{releaseLabel}</AppTableHeaderCell>
                    <AppTableHeaderCell>Action</AppTableHeaderCell>
                  </AppTableHeaderRow>
                </AppTableHead>
                <AppTableBody>
                  {reservations.length === 0 && (
                    <AppTableRow>
                      <AppTableCell className="text-muted-foreground" colSpan={6}>
                        No active reservations.
                      </AppTableCell>
                    </AppTableRow>
                  )}
                  {reservations.map((reservation) => (
                    <AppTableRow key={reservation.id}>
                      <AppTableCell align="right">
                        {reservation.quantityReserved} {quantityUnit}
                      </AppTableCell>
                      <AppTableCell align="right">{formatMoney(reservation.unitCostSnapshot)}</AppTableCell>
                      <AppTableCell>{reservation.reservedBy}</AppTableCell>
                      <AppTableCell>{dateTime.format(reservation.reservedAt)}</AppTableCell>
                      <AppTableCell>
                        <AppInput
                          aria-label={releaseLabel}
                          className="w-32 text-right"
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
                      </AppTableCell>
                      <AppTableCell>
                        <AppButton appearance="secondary" disabled={!canRelease(reservation)} isLoading={isReleasing} onClick={() => release(reservation)} type="button">
                          Release
                        </AppButton>
                      </AppTableCell>
                    </AppTableRow>
                  ))}
                </AppTableBody>
              </AppTable>
            </AppTableContainer>
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
