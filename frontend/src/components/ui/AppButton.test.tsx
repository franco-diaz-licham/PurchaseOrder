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
});
