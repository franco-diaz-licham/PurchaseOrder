import { createBrowserRouter, Navigate } from 'react-router-dom';
import { AuditLogPage } from '@/features/audit/pages/AuditLogPage';
import { PurchaseOrdersPage } from '@/features/purchase-orders/pages/PurchaseOrdersPage';
import { FinancePage } from '@/features/reports/pages/FinancePage';
import { ReservationsPage } from '@/features/reservations/pages/ReservationsPage';
import { WorkspaceLayout } from '@/layouts/WorkspaceLayout';

export const router = createBrowserRouter([
  {
    element: <WorkspaceLayout />,
    children: [
      { index: true, element: <Navigate replace to="/purchase-orders" /> },
      { path: '/purchase-orders', element: <PurchaseOrdersPage /> },
      { path: '/reservations', element: <ReservationsPage /> },
      { path: '/finance', element: <FinancePage /> },
      { path: '/audit-log', element: <AuditLogPage /> }
    ]
  },
  { path: '*', element: <Navigate replace to="/purchase-orders" /> }
]);
