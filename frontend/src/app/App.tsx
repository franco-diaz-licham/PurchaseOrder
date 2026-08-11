import { PrimeReactProvider } from '@primereact/core';
import { QueryClientProvider } from '@tanstack/react-query';
import { RouterProvider } from 'react-router-dom';
import { queryClient } from '@/lib/api/queryClient';
import { primeReactConfig } from './primeReactConfig';
import { router } from './router';

export const App = () => (
  <PrimeReactProvider {...primeReactConfig}>
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>
  </PrimeReactProvider>
);
