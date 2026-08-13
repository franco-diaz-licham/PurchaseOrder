import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, test, vi } from 'vitest';
import { ErrorSummary } from './ErrorSummary';

describe('ErrorSummary', () => {
  test('raises dismiss action when the close button is clicked', async () => {
    // Arrange
    const user = userEvent.setup();
    const onDismiss = vi.fn();
    render(<ErrorSummary messages={['Purchase order line already exists.']} onDismiss={onDismiss} />);

    // Act
    await user.click(screen.getByRole('button', { name: 'Dismiss errors' }));

    // Assert
    expect(onDismiss).toHaveBeenCalledTimes(1);
  });
});
