import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';
import { PurchaseOrderTotalsBar } from './PurchaseOrderTotalsBar';

describe('PurchaseOrderTotalsBar', () => {
  test('renders subtotal, GST, and total amounts', () => {
    // Arrange / Act
    render(<PurchaseOrderTotalsBar gstAmount={79.5} subtotalAmount={795} totalAmount={874.5} />);

    // Assert
    expect(screen.getByText('Subtotal')).toBeInTheDocument();
    expect(screen.getByText('$795.00')).toBeInTheDocument();
    expect(screen.getByText('GST')).toBeInTheDocument();
    expect(screen.getByText('$79.50')).toBeInTheDocument();
    expect(screen.getByText('Total')).toBeInTheDocument();
    expect(screen.getByText('$874.50')).toBeInTheDocument();
  });
});
