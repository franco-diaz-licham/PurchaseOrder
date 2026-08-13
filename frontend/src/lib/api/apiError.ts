/**
 * Error response shapes the API may return for domain, validation, or unexpected failures.
 */
type ApiErrorResponse = {
  statusCode?: number;
  message?: string;
  errors?: string[] | Record<string, string[]>;
  validationErrors?: string[];
  details?: string;
};

type ErrorWithResponse = {
  response?: {
    data?: unknown;
  };
};

const isObject = (value: unknown): value is Record<string, unknown> => typeof value === 'object' && value !== null;

const isApiErrorResponse = (value: unknown): value is ApiErrorResponse => {
  if (!isObject(value)) return false;
  return 'message' in value || 'errors' in value || 'validationErrors' in value || 'details' in value;
};

const flattenErrors = (errors: ApiErrorResponse['errors']) => {
  if (!errors) return [];
  if (Array.isArray(errors)) return errors;
  return Object.values(errors).flat();
};

const getAxiosResponseData = (error: unknown) => {
  if (!isObject(error)) return undefined;

  const errorWithResponse = error as ErrorWithResponse;
  return errorWithResponse.response?.data;
};

/**
 * Extracts the best user-facing error message from an API or JavaScript error.
 *
 * The backend normally returns a message in its API envelope, but validation and
 * framework errors can arrive as arrays or field dictionaries. This helper keeps
 * that normalization in one place so components do not need to understand every
 * response shape.
 */
export const getApiErrorMessage = (error: unknown, fallbackMessage: string): string => {
  const responseData = getAxiosResponseData(error);
  const apiError = isApiErrorResponse(responseData) ? responseData : isApiErrorResponse(error) ? error : null;

  if (apiError?.message) return apiError.message;
  if (apiError?.validationErrors?.length) return apiError.validationErrors.join('\n');

  const errors = flattenErrors(apiError?.errors);
  if (errors.length > 0) return errors.join('\n');

  if (apiError?.details) return apiError.details;
  if (error instanceof Error && error.message) return error.message;

  return fallbackMessage;
};
