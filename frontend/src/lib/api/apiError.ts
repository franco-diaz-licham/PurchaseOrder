import type { AxiosError } from 'axios';

type ApiErrorResponse = {
  statusCode?: number;
  message?: string;
  errors?: string[] | Record<string, string[]>;
  validationErrors?: string[];
  details?: string;
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
  const axiosError = error as AxiosError;
  return axiosError.response?.data;
};

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
