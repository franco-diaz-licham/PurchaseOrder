import { getApiErrorMessage } from '@/lib/api/apiError';

type QueryErrorState = {
  isError: boolean;
  error: unknown;
};

type PurchaseOrderDetailErrorState = {
  purchaseOrderQuery: QueryErrorState;
  statusMutation: QueryErrorState;
  addLineMutation: QueryErrorState;
  removeLineMutation: QueryErrorState;
  updateLineMutation: QueryErrorState;
  createReservationMutation: QueryErrorState;
  releaseReservationMutation: QueryErrorState;
};

type ErrorDefinition = {
  state: QueryErrorState;
  fallbackMessage: string;
};

export const getPurchaseOrderDetailErrorMessages = (state: PurchaseOrderDetailErrorState) => {
  const errors: ErrorDefinition[] = [
    { state: state.purchaseOrderQuery, fallbackMessage: 'Purchase order could not be loaded.' },
    { state: state.statusMutation, fallbackMessage: 'Purchase order status could not be changed.' },
    { state: state.addLineMutation, fallbackMessage: 'Purchase order line could not be added.' },
    { state: state.removeLineMutation, fallbackMessage: 'Purchase order line could not be removed.' },
    { state: state.updateLineMutation, fallbackMessage: 'Purchase order line could not be updated.' },
    { state: state.createReservationMutation, fallbackMessage: 'Stock could not be reserved for this line.' },
    { state: state.releaseReservationMutation, fallbackMessage: 'Reservation could not be released.' }
  ];

  return errors.filter((error) => error.state.isError).map((error) => getApiErrorMessage(error.state.error, error.fallbackMessage));
};
