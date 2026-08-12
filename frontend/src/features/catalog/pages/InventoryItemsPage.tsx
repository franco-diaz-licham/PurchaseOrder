import { useInventoryItemsQuery } from '../queries/catalog.queries';
import { useChangeInventoryItemStandardCostMutation } from '../queries/inventoryItem.mutations';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { AppButton } from '@/components/ui/AppButton';
import { AppInput } from '@/components/ui/AppInput';
import { formatMoney } from '@/lib/formatMoney';

export const InventoryItemsPage = () => {
  const itemsQuery = useInventoryItemsQuery();
  const changeStandardCostMutation = useChangeInventoryItemStandardCostMutation();

  const changeStandardCost = async (event: React.FormEvent<HTMLFormElement>, inventoryItemId: string) => {
    event.preventDefault();

    const formData = new FormData(event.currentTarget);
    const standardCost = Number(formData.get('standardCost'));

    await changeStandardCostMutation.mutateAsync({
      inventoryItemId,
      standardCost,
      user: 'Franco Diaz'
    });
  };

  return (
    <section>
      <PageHeader description="Update item standard costs. Existing reservations keep the cost captured when they were created." title="Inventory Items" />

      <div className="p-6">
        <div className="rounded-md border bg-card">
          {itemsQuery.isError && <ErrorMessage message="Inventory items could not be loaded." />}
          {changeStandardCostMutation.isError && <ErrorMessage message="Standard cost could not be changed." />}
          {(itemsQuery.data ?? []).length === 0 && !itemsQuery.isLoading && <EmptyState title="No inventory items found." />}

          {(itemsQuery.data ?? []).length > 0 && (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[900px] text-left text-sm">
                <thead className="bg-muted text-xs uppercase text-muted-foreground">
                  <tr>
                    <th className="px-4 py-3">SKU</th>
                    <th className="px-4 py-3">Item</th>
                    <th className="px-4 py-3">Category</th>
                    <th className="px-4 py-3">Tracking</th>
                    <th className="px-4 py-3">Current cost</th>
                    <th className="px-4 py-3">New cost</th>
                  </tr>
                </thead>
                <tbody>
                  {(itemsQuery.data ?? []).map((item) => (
                    <tr className="border-t" key={item.id}>
                      <td className="px-4 py-3 font-semibold">{item.sku}</td>
                      <td className="px-4 py-3">{item.name}</td>
                      <td className="px-4 py-3">{item.category}</td>
                      <td className="px-4 py-3">{item.trackingMode}</td>
                      <td className="px-4 py-3">{formatMoney(item.standardCost)}</td>
                      <td className="px-4 py-3">
                        <form className="flex items-center gap-2" onSubmit={(event) => changeStandardCost(event, item.id)}>
                          <AppInput className="w-28" defaultValue={item.standardCost} min="0" name="standardCost" step="0.01" type="number" />
                          <AppButton disabled={changeStandardCostMutation.isPending} type="submit">
                            Save
                          </AppButton>
                        </form>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </section>
  );
};
