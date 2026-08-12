import { UilTimes } from '@iconscout/react-unicons';
import { useForm } from 'react-hook-form';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';

export type EditPurchaseOrderLineFormValues = {
  quantityOrdered: number;
  user: string;
};

type EditPurchaseOrderLineDialogProps = {
  itemName: string;
  quantityOrdered: number;
  quantityReserved: number;
  isSaving: boolean;
  onCancel: () => void;
  onSubmit: (values: EditPurchaseOrderLineFormValues) => Promise<void>;
};

export const EditPurchaseOrderLineDialog = ({ itemName, quantityOrdered, quantityReserved, isSaving, onCancel, onSubmit }: EditPurchaseOrderLineDialogProps) => {
  const form = useForm<EditPurchaseOrderLineFormValues>({
    defaultValues: {
      quantityOrdered,
      user: 'Franco Diaz'
    }
  });

  const submit = form.handleSubmit(onSubmit);

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="edit-line-title">
      <form className="w-full max-w-lg rounded-md border bg-card shadow-lg" onSubmit={submit}>
        <div className="flex items-center justify-between border-b p-4">
          <h2 className="text-base font-semibold" id="edit-line-title">
            Edit purchase order line
          </h2>
          <AppButton appearance="ghost" onClick={onCancel} type="button">
            <UilTimes className="h-4 w-4" />
          </AppButton>
        </div>

        <div className="grid gap-3 p-4">
          <div>
            <div className="font-medium">{itemName}</div>
            <div className="mt-1 text-sm text-muted-foreground">Reserved quantity {quantityReserved}</div>
          </div>

          <AppField label="Ordered quantity">
            <AppInput autoFocus min={quantityReserved} required step="0.001" type="number" {...form.register('quantityOrdered', { valueAsNumber: true })} />
          </AppField>

          <AppField label="User">
            <AppInput required {...form.register('user')} />
          </AppField>
        </div>

        <div className="flex justify-end gap-2 border-t p-4">
          <AppButton appearance="secondary" onClick={onCancel} type="button">
            Cancel
          </AppButton>
          <AppButton disabled={isSaving} type="submit">
            Save
          </AppButton>
        </div>
      </form>
    </div>
  );
};
