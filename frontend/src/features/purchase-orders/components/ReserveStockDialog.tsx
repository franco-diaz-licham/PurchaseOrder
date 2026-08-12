import { UilTimes } from '@iconscout/react-unicons';
import { useState } from 'react';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import type { PurchaseOrderLine } from '../types/purchaseOrder.types';

type ReserveStockDialogProps = {
  availableQuantity: number | null;
  itemName: string;
  isSaving: boolean;
  line: PurchaseOrderLine;
  maxQuantity: number;
  user: string;
  onCancel: () => void;
  onSubmit: (quantity: number, user: string) => Promise<void>;
  onUserChange: (user: string) => void;
};

export const ReserveStockDialog = ({ availableQuantity, itemName, isSaving, line, maxQuantity, user, onCancel, onSubmit, onUserChange }: ReserveStockDialogProps) => {
  const [quantity, setQuantity] = useState('');
  const parsedQuantity = Number(quantity || 0);
  const canSubmit = !isSaving && parsedQuantity > 0 && parsedQuantity <= maxQuantity && user.trim().length > 0;

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="reserve-line-title">
      <form
        className="w-full max-w-md rounded-md border bg-card shadow-lg"
        onSubmit={(event) => {
          event.preventDefault();
          void onSubmit(parsedQuantity, user);
        }}
      >
        <div className="flex items-center justify-between border-b p-4">
          <h2 className="text-base font-semibold" id="reserve-line-title">
            Reserve stock
          </h2>
          <AppButton appearance="ghost" onClick={onCancel} type="button">
            <UilTimes className="h-4 w-4" />
          </AppButton>
        </div>

        <div className="grid gap-3 p-4">
          <div className="text-sm">
            <p className="font-medium">{itemName}</p>
            <p className="mt-1 text-muted-foreground">
              Ordered {line.quantityOrdered} - Available {availableQuantity ?? 'Not stocked'} - Remaining {line.quantityRemaining}
            </p>
          </div>

          <AppField label="Quantity">
            <AppInput autoFocus max={maxQuantity} min="0.001" onChange={(event) => setQuantity(event.target.value)} required step="0.001" type="number" value={quantity} />
          </AppField>

          <AppField label="User">
            <AppInput onChange={(event) => onUserChange(event.target.value)} required value={user} />
          </AppField>
        </div>

        <div className="flex justify-end gap-2 border-t p-4">
          <AppButton appearance="secondary" onClick={onCancel} type="button">
            Cancel
          </AppButton>
          <AppButton disabled={!canSubmit} type="submit">
            Reserve
          </AppButton>
        </div>
      </form>
    </div>
  );
};
