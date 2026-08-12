import { useInventoryItemsQuery } from '../queries/catalog.queries';
import { useChangeInventoryItemStandardCostMutation } from '../queries/inventoryItem.mutations';
import { EmptyState } from '@/components/common/EmptyState';
import { ErrorMessage } from '@/components/common/ErrorMessage';
import { PageHeader } from '@/components/common/PageHeader';
import { AppButton } from '@/components/ui/AppButton';
import { AppInput } from '@/components/ui/AppInput';
import { AppTable, AppTableBody, AppTableCell, AppTableContainer, AppTableHead, AppTableHeaderCell, AppTableHeaderRow, AppTableRow } from '@/components/ui/AppTable';
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
            <AppTableContainer maxHeight="calc(100vh - 10.5rem)">
              <AppTable minWidth="56.25rem">
                <AppTableHead sticky>
                  <AppTableHeaderRow>
                    <AppTableHeaderCell>SKU</AppTableHeaderCell>
                    <AppTableHeaderCell>Item</AppTableHeaderCell>
                    <AppTableHeaderCell>Category</AppTableHeaderCell>
                    <AppTableHeaderCell>Tracking</AppTableHeaderCell>
                    <AppTableHeaderCell align="right">Current cost</AppTableHeaderCell>
                    <AppTableHeaderCell align="right">New cost</AppTableHeaderCell>
                  </AppTableHeaderRow>
                </AppTableHead>
                <AppTableBody>
                  {(itemsQuery.data ?? []).map((item) => (
                    <AppTableRow key={item.id}>
                      <AppTableCell className="font-semibold">{item.sku}</AppTableCell>
                      <AppTableCell>{item.name}</AppTableCell>
                      <AppTableCell>{item.category}</AppTableCell>
                      <AppTableCell>{item.trackingMode}</AppTableCell>
                      <AppTableCell align="right">{formatMoney(item.standardCost)}</AppTableCell>
                      <AppTableCell>
                        <form className="flex items-center justify-end gap-2" onSubmit={(event) => changeStandardCost(event, item.id)}>
                          <AppInput className="w-28 text-right" defaultValue={item.standardCost} min="0" name="standardCost" step="0.01" type="number" />
                          <AppButton disabled={changeStandardCostMutation.isPending} type="submit">
                            Save
                          </AppButton>
                        </form>
                      </AppTableCell>
                    </AppTableRow>
                  ))}
                </AppTableBody>
              </AppTable>
            </AppTableContainer>
          )}
        </div>
      </div>
    </section>
  );
};
