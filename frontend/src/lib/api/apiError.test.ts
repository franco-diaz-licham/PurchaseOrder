import { describe, expect, test } from 'vitest';
import { getApiErrorMessage } from './apiError';

describe('api error', () => {
  test('uses backend message when one is returned', () => {
    // Arrange
    const error = {
      response: {
        data: {
          message: 'Purchase order line already exists.'
        }
      }
    };

    // Act
    const message = getApiErrorMessage(error, 'Fallback message.');

    // Assert
    expect(message).toBe('Purchase order line already exists.');
  });

  test('joins backend validation errors', () => {
    // Arrange
    const error = {
      response: {
        data: {
          validationErrors: ['Quantity is required.', 'User is required.']
        }
      }
    };

    // Act
    const message = getApiErrorMessage(error, 'Fallback message.');

    // Assert
    expect(message).toBe('Quantity is required.\nUser is required.');
  });
});
