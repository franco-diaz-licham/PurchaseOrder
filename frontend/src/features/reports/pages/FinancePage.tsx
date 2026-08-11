import { useForm } from 'react-hook-form';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { AppButton } from '@/components/ui/AppButton';
import { AppField } from '@/components/ui/AppField';
import { AppInput } from '@/components/ui/AppInput';
import { AppSelect } from '@/components/ui/AppSelect';
import { useInventoryItemsQuery } from '@/features/catalog/queries/catalog.queries';
import { useChangeInventoryItemStandardCostMutation } from '@/features/catalog/queries/inventoryItem.mutations';
import { useWarehouseCommittedValuesQuery } from '../queries/finance.queries';

type CostFormValues = {
  inventoryItemId: string;
  standardCost: number;
  user: string;
};

export const FinancePage = () => {
  const financeQuery = useWarehouseCommittedValuesQuery();
  const itemsQuery = useInventoryItemsQuery();
  const costMutation = useChangeInventoryItemStandardCostMutation();
  const { register, handleSubmit, reset } = useForm<CostFormValues>({
    defaultValues: {
      inventoryItemId: '',
      standardCost: 1,
      user: 'finance-user'
    }
  });

  const totalCommittedValue = (financeQuery.data ?? []).reduce((total, row) => total + row.committedValue, 0);

  const submit = handleSubmit(async (values) => {
    await costMutation.mutateAsync({
      inventoryItemId: values.inventoryItemId,
      standardCost: Number(values.standardCost),
      user: values.user
    });
    reset({ inventoryItemId: '', standardCost: 1, user: values.user });
  });

  return (
    <section>
      <PageHeader description="Committed value is calculated from active reservations using the standard cost captured at reservation time." title="Finance" />

      <div className="grid gap-6 p-6 xl:grid-cols-[380px_1fr]">
        <form className="self-start rounded-md border bg-card p-4" onSubmit={submit}>
          <h2 className="text-base font-semibold">Change item standard cost</h2>
          <div className="mt-4 grid gap-3">
            <AppField label="Inventory item">
              <AppSelect required {...register('inventoryItemId')}>
                <option value="">Select item</option>
                {(itemsQuery.data ?? []).map((item) => (
                  <option key={item.id} value={item.id}>
                    {item.sku} - current ${item.standardCost.toFixed(2)}
                  </option>
                ))}
              </AppSelect>
            </AppField>
            <AppField label="New standard cost">
              <AppInput min="0" required step="0.0001" type="number" {...register('standardCost', { valueAsNumber: true })} />
            </AppField>
            <AppField label="User">
              <AppInput required {...register('user')} />
            </AppField>
            <AppButton disabled={costMutation.isPending} type="submit">Save cost</AppButton>
          </div>
        </form>

        <div className="grid gap-4">
          <div className="rounded-md border bg-card p-5">
            <div className="text-sm text-muted-foreground">Total committed value</div>
            <div className="mt-1 text-3xl font-semibold">${totalCommittedValue.toFixed(2)}</div>
          </div>
          <div className="rounded-md border bg-card">
            {financeQuery.isError && <ErrorMessage message="Finance values could not be loaded." />}
            {(financeQuery.data ?? []).length === 0 && !financeQuery.isLoading && <EmptyState title="No committed reservation value found." />}
            <table className="w-full text-left text-sm">
              <thead className="bg-muted text-xs uppercase text-muted-foreground">
                <tr>
                  <th className="px-4 py-3">Warehouse</th>
                  <th className="px-4 py-3">Committed value</th>
                </tr>
              </thead>
              <tbody>
                {(financeQuery.data ?? []).map((row) => (
                  <tr className="border-t" key={row.warehouseId}>
                    <td className="px-4 py-3">{row.warehouseDisplayName}</td>
                    <td className="px-4 py-3 font-semibold">${row.committedValue.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </section>
  );
};
