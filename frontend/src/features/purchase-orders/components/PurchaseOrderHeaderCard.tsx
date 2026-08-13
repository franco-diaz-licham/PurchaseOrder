import { UilCheck, UilSync, UilTimes } from '@iconscout/react-unicons';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';

type PurchaseOrderHeaderCardProps = {
  changingStatusAction?: 'approve' | 'close' | 'cancel' | null;
  isChangingStatus: boolean;
  purchaseOrder: PurchaseOrderModel;
  warehouseDisplayName: string;
  onApprove: () => void;
  onCancel: () => void;
  onClose: () => void;
};

export const PurchaseOrderHeaderCard = ({ changingStatusAction = null, isChangingStatus, purchaseOrder, warehouseDisplayName, onApprove, onCancel, onClose }: PurchaseOrderHeaderCardProps) => {
  const isReadOnly = purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled';

  return (
    <article className="rounded-md border bg-card">
      <div className="flex flex-col gap-3 p-4 md:flex-row md:items-center md:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h2 className="text-base font-semibold">{purchaseOrder.number}</h2>
            <StatusBadge status={purchaseOrder.status} />
          </div>
          <p className="mt-1 text-sm text-muted-foreground">{warehouseDisplayName}</p>
        </div>
        {!isReadOnly && (
          <div className="flex flex-wrap gap-2">
            <AppButton appearance="secondary" disabled={purchaseOrder.status !== 'Pending' || isChangingStatus} isLoading={isChangingStatus && changingStatusAction === 'approve'} onClick={onApprove}>
              <UilCheck className="h-4 w-4" />
              Approve
            </AppButton>
            <AppButton appearance="secondary" disabled={isChangingStatus} isLoading={isChangingStatus && changingStatusAction === 'close'} onClick={onClose}>
              <UilSync className="h-4 w-4" />
              Close
            </AppButton>
            <AppButton appearance="danger" disabled={isChangingStatus} isLoading={isChangingStatus && changingStatusAction === 'cancel'} onClick={onCancel}>
              <UilTimes className="h-4 w-4" />
              Cancel
            </AppButton>
          </div>
        )}
      </div>
    </article>
  );
};
