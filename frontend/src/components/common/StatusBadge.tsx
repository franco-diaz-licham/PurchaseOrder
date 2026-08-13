import type { PurchaseOrderStatus } from '@/features/purchase-orders/types/purchaseOrder.types';
import { cn } from '@/lib/cn';

type StatusBadgeProps = {
  status: PurchaseOrderStatus;
};

const statusClasses: Record<PurchaseOrderStatus, string> = {
  Pending: 'bg-slate-100 text-slate-700',
  Approved: 'bg-teal-100 text-teal-800',
  Closed: 'bg-blue-100 text-blue-800',
  Cancelled: 'bg-red-100 text-red-800'
};

export const StatusBadge = ({ status }: StatusBadgeProps) => <span className={cn('inline-flex h-7 items-center rounded-full px-2.5 text-xs font-semibold', statusClasses[status])}>{status}</span>;
