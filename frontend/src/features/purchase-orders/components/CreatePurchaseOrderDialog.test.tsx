import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import type { WarehouseModel } from '@/features/catalog/types/catalog.types';
import { CreatePurchaseOrderDialog } from './CreatePurchaseOrderDialog';

const warehouses: WarehouseModel[] = [
  {
    id: 'warehouse-nsw',
    code: 'NSW',
    name: 'New South Wales',
    displayName: 'NSW - New South Wales'
  }
];

describe('CreatePurchaseOrderDialog', () => {
  test('submits selected warehouse and user', async () => {
    // Arrange
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(<CreatePurchaseOrderDialog isError={false} isSaving={false} warehouses={warehouses} onCancel={vi.fn()} onSubmit={onSubmit} />);

    // Act
    await user.selectOptions(screen.getByLabelText('Warehouse'), 'warehouse-nsw');
    await user.clear(screen.getByLabelText('User'));
    await user.type(screen.getByLabelText('User'), 'Tara Smith');
    await user.click(screen.getByRole('button', { name: 'Create' }));

    // Assert
    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(onSubmit).toHaveBeenCalledWith({
      warehouseId: 'warehouse-nsw',
      user: 'Tara Smith'
    });
  });

  test('shows the submit button loading state while saving', () => {
    // Act
    render(<CreatePurchaseOrderDialog isError={false} isSaving warehouses={warehouses} onCancel={vi.fn()} onSubmit={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Create' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Create' }).querySelector('.animate-spin')).toBeInTheDocument();
  });
});
