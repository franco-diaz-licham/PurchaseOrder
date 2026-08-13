/**
 * Standard successful API envelope returned by the backend.
 */
export type ApiResponse<T> = {
  /** HTTP status code represented by the backend response wrapper. */
  statusCode: number;

  /** Human-readable response message from the backend. */
  message: string;

  /** Typed response payload. */
  data: T;
};

/**
 * Standard API envelope for responses that do not return a payload.
 */
export type ApiMessage = {
  /** HTTP status code represented by the backend response wrapper. */
  statusCode: number;

  /** Human-readable response message from the backend. */
  message: string;
};
