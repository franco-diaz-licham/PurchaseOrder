import { describe, expect, test } from 'vitest';
import { getPurchaseOrderDetailErrorMessages } from './purchaseOrderDetailErrors';

const noError = {
  error: null,
  isError: false
};

const createState = (overrides = {}) => ({
  purchaseOrderQuery: noError,
  statusMutation: noError,
  addLineMutation: noError,
  removeLineMutation: noError,
  updateLineMutation: noError,
  createReservationMutation: noError,
  releaseReservationMutation: noError,
  ...overrides
});

describe('purchase order detail errors', () => {
  test('returns no messages when no query or mutation has failed', () => {
    // Arrange
    const state = createState();

    // Act
    const messages = getPurchaseOrderDetailErrorMessages(state);

    // Assert
    expect(messages).toEqual([]);
  });

  test('uses backend messages when query or mutation errors include one', () => {
    // Arrange
    const state = createState({
      addLineMutation: {
        isError: true,
        error: {
          response: {
            data: {
              message: 'Purchase order line already exists.'
            }
          }
        }
      }
    });

    // Act
    const messages = getPurchaseOrderDetailErrorMessages(state);

    // Assert
    expect(messages).toEqual(['Purchase order line already exists.']);
  });

  test('uses fallback messages when errors cannot be normalized', () => {
    // Arrange
    const state = createState({
      purchaseOrderQuery: {
        isError: true,
        error: null
      },
      releaseReservationMutation: {
        isError: true,
        error: null
      }
    });

    // Act
    const messages = getPurchaseOrderDetailErrorMessages(state);

    // Assert
    expect(messages).toEqual(['Purchase order could not be loaded.', 'Reservation could not be released.']);
  });

  test('returns messages in the same order as the purchase order detail workflow', () => {
    // Arrange
    const state = createState({
      statusMutation: {
        isError: true,
        error: new Error('Status failed.')
      },
      createReservationMutation: {
        isError: true,
        error: new Error('Reservation failed.')
      }
    });

    // Act
    const messages = getPurchaseOrderDetailErrorMessages(state);

    // Assert
    expect(messages).toEqual(['Status failed.', 'Reservation failed.']);
  });
});
