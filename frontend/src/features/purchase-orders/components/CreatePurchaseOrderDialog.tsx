import { UilTimes } from '@iconscout/react-unicons';
import { useForm } from 'react-hook-form';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppSelect } from '@/components/ui/AppSelect';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';

export type CreatePurchaseOrderFormValues = {
  warehouseId: string;
  user: string;
};

type CreatePurchaseOrderDialogProps = {
  isError: boolean;
  isSaving: boolean;
  warehouses: WarehouseModel[];
  onCancel: () => void;
  onSubmit: (values: CreatePurchaseOrderFormValues) => Promise<void>;
};

export const CreatePurchaseOrderDialog = ({ isError, isSaving, warehouses, onCancel, onSubmit }: CreatePurchaseOrderDialogProps) => {
  const warehouseOptions = warehouses.map((warehouse) => ({
    label: warehouse.displayName,
    value: warehouse.id
  }));

  const form = useForm<CreatePurchaseOrderFormValues>({
    defaultValues: {
      warehouseId: '',
      user: 'Franco Diaz'
    }
  });

  const submit = form.handleSubmit(async (values) => {
    await onSubmit(values);
    form.reset({ warehouseId: '', user: 'Franco Diaz' });
  });

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/40 p-4" role="dialog" aria-modal="true" aria-labelledby="create-po-title">
      <form className="w-full max-w-lg rounded-md border bg-card shadow-lg" onSubmit={submit}>
        <div className="flex items-center justify-between border-b p-4">
          <h2 className="text-base font-semibold" id="create-po-title">
            New purchase order
          </h2>
          <AppButton appearance="ghost" onClick={onCancel} type="button">
            <UilTimes className="h-4 w-4" />
          </AppButton>
        </div>

        <div className="grid gap-3 p-4">
          {isError && <ErrorMessage message="Purchase order could not be created." />}

          <AppField label="Warehouse">
            <AppSelect autoFocus options={warehouseOptions} placeholder="Select warehouse" required {...form.register('warehouseId')} />
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
            Create
          </AppButton>
        </div>
      </form>
    </div>
  );
};
