import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import { EditPurchaseOrderLineDialog } from './EditPurchaseOrderLineDialog';

describe('EditPurchaseOrderLineDialog', () => {
  test('submits updated ordered quantity and user', async () => {
    // Arrange
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockResolvedValue(undefined);

    render(<EditPurchaseOrderLineDialog isSaving={false} itemName="BEAM-6M - 6m Spreader Beam [Unit]" quantityOrdered={10} quantityReserved={4} onCancel={vi.fn()} onSubmit={onSubmit} />);

    // Act
    await user.clear(screen.getByLabelText('Ordered quantity'));
    await user.type(screen.getByLabelText('Ordered quantity'), '15');
    await user.clear(screen.getByLabelText('User'));
    await user.type(screen.getByLabelText('User'), 'Tara Smith');
    await user.click(screen.getByRole('button', { name: 'Save' }));

    // Assert
    expect(onSubmit).toHaveBeenCalledTimes(1);
    expect(onSubmit).toHaveBeenCalledWith({
      quantityOrdered: 15,
      user: 'Tara Smith'
    });
  });

  test('shows the reserved quantity and uses it as the minimum ordered quantity', () => {
    // Arrange / Act
    render(<EditPurchaseOrderLineDialog isSaving={false} itemName="BEAM-6M - 6m Spreader Beam [Unit]" quantityOrdered={10} quantityReserved={4} onCancel={vi.fn()} onSubmit={vi.fn()} />);

    // Assert
    expect(screen.getByText('BEAM-6M - 6m Spreader Beam [Unit]')).toBeInTheDocument();
    expect(screen.getByText('Reserved quantity 4')).toBeInTheDocument();
    expect(screen.getByLabelText('Ordered quantity')).toHaveAttribute('min', '4');
  });

  test('shows loading state while saving', () => {
    // Arrange / Act
    render(<EditPurchaseOrderLineDialog isSaving itemName="BEAM-6M - 6m Spreader Beam [Unit]" quantityOrdered={10} quantityReserved={4} onCancel={vi.fn()} onSubmit={vi.fn()} />);

    // Assert
    expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Save' }).querySelector('.animate-spin')).toBeInTheDocument();
  });

  test('cancels the dialog', async () => {
    // Arrange
    const user = userEvent.setup();
    const onCancel = vi.fn();

    render(<EditPurchaseOrderLineDialog isSaving={false} itemName="BEAM-6M - 6m Spreader Beam [Unit]" quantityOrdered={10} quantityReserved={4} onCancel={onCancel} onSubmit={vi.fn()} />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Cancel' }));

    // Assert
    expect(onCancel).toHaveBeenCalledTimes(1);
  });
});
