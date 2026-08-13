import { render, screen } from '@testing-library/react';
import { describe, expect, test } from 'vitest';
import { AppButton } from './AppButton';

describe('AppButton', () => {
  test('shows a loading spinner and disables the button while loading', () => {
    // Act
    render(<AppButton isLoading>Save</AppButton>);

    // Assert
    expect(screen.getByRole('button', { name: 'Save' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Save' }).querySelector('.animate-spin')).toBeInTheDocument();
  });

  test('shows only the spinner content while an icon-only button is loading', () => {
    // Act
    render(
      <AppButton aria-label="Remove line" isLoading>
        <span data-testid="remove-icon" />
      </AppButton>
    );

    // Assert
    expect(screen.getByRole('button', { name: 'Remove line' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Remove line' }).querySelector('.animate-spin')).toBeInTheDocument();
    expect(screen.queryByTestId('remove-icon')).not.toBeInTheDocument();
  });
});
