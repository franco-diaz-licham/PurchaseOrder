import { UilTimes } from '@iconscout/react-unicons';
import { useForm } from 'react-hook-form';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppSelect } from '@/components/ui/AppSelect';
import type { InventoryItemModel } from '@/features/catalog/types/catalog.types';

export type AddPurchaseOrderLineFormValues = {
  inventoryItemId: string;
  quantityOrdered: number;
  user: string;
};

type AddPurchaseOrderLineDialogProps = {
  inventoryItems: InventoryItemModel[];
  isSaving: boolean;
  onCancel: () => void;
  onSubmit: (values: AddPurchaseOrderLineFormValues) => Promise<void>;
};

export const AddPurchaseOrderLineDialog = ({ inventoryItems, isSaving, onCancel, onSubmit }: AddPurchaseOrderLineDialogProps) => {
  const form = useForm<AddPurchaseOrderLineFormValues>({
    defaultValues: {
      inventoryItemId: '',
      quantityOrdered: 1,
      user: 'Franco Diaz'
    }
  });

  const submit = form.handleSubmit(async (values) => {
    await onSubmit(values);
    form.reset({ inventoryItemId: '', quantityOrdered: 1, user: values.user });
  });

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="add-line-title">
      <form className="w-full max-w-lg rounded-md border bg-card shadow-lg" onSubmit={submit}>
        <div className="flex items-center justify-between border-b p-4">
          <h2 className="text-base font-semibold" id="add-line-title">
            Add purchase order line
          </h2>
          <AppButton appearance="ghost" onClick={onCancel} type="button">
            <UilTimes className="h-4 w-4" />
          </AppButton>
        </div>

        <div className="grid gap-3 p-4">
          <AppField label="Inventory item">
            <AppSelect autoFocus required {...form.register('inventoryItemId')}>
              <option value="">Select item</option>
              {inventoryItems.map((item) => (
                <option key={item.id} value={item.id}>
                  {item.displayName}
                </option>
              ))}
            </AppSelect>
          </AppField>

          <AppField label="Quantity">
            <AppInput min="0.001" required step="0.001" type="number" {...form.register('quantityOrdered', { valueAsNumber: true })} />
          </AppField>

          <AppField label="User">
            <AppInput required {...form.register('user')} />
          </AppField>
        </div>

        <div className="flex justify-end gap-2 border-t p-4">
          <AppButton appearance="secondary" onClick={onCancel} type="button">
            Cancel
          </AppButton>
          <AppButton disabled={isSaving || inventoryItems.length === 0} type="submit">
            Add line
          </AppButton>
        </div>
      </form>
    </div>
  );
};
