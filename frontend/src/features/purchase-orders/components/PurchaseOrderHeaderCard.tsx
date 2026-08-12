import { UilCheck, UilPlus, UilSync, UilTimes } from '@iconscout/react-unicons';
import { StatusBadge } from '@/components/common/StatusBadge';
import { AppButton } from '@/components/ui/AppButton';
import { formatMoney } from '@/lib/formatMoney';
import type { PurchaseOrderModel } from '../types/purchaseOrder.types';

type PurchaseOrderHeaderCardProps = {
  availableItemCount: number;
  canChangeLines: boolean;
  isAddingLine: boolean;
  isChangingStatus: boolean;
  purchaseOrder: PurchaseOrderModel;
  warehouseDisplayName: string;
  onAddLine: () => void;
  onApprove: () => void;
  onCancel: () => void;
  onClose: () => void;
};

export const PurchaseOrderHeaderCard = ({ availableItemCount, canChangeLines, isAddingLine, isChangingStatus, purchaseOrder, warehouseDisplayName, onAddLine, onApprove, onCancel, onClose }: PurchaseOrderHeaderCardProps) => (
  <article className="rounded-md border bg-card">
    <div className="flex flex-col gap-3 border-b p-4 md:flex-row md:items-center md:justify-between">
      <div>
        <div className="flex items-center gap-3">
          <h2 className="text-base font-semibold">{purchaseOrder.number}</h2>
          <StatusBadge status={purchaseOrder.status} />
        </div>
        <p className="mt-1 text-sm text-muted-foreground">{warehouseDisplayName}</p>
      </div>
      <div className="flex flex-wrap gap-2">
        {canChangeLines && (
          <AppButton disabled={isAddingLine || availableItemCount === 0} onClick={onAddLine} type="button">
            <UilPlus className="h-4 w-4" />
            Add line
          </AppButton>
        )}
        <AppButton appearance="secondary" disabled={purchaseOrder.status !== 'Pending' || isChangingStatus} onClick={onApprove}>
          <UilCheck className="h-4 w-4" />
          Approve
        </AppButton>
        <AppButton appearance="secondary" disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || isChangingStatus} onClick={onClose}>
          <UilSync className="h-4 w-4" />
          Close
        </AppButton>
        <AppButton appearance="danger" disabled={purchaseOrder.status === 'Closed' || purchaseOrder.status === 'Cancelled' || isChangingStatus} onClick={onCancel}>
          <UilTimes className="h-4 w-4" />
          Cancel
        </AppButton>
      </div>
    </div>

    <div className="grid gap-3 border-b p-4 text-sm md:grid-cols-3">
      <div>
        <p className="text-xs uppercase text-muted-foreground">Subtotal</p>
        <p className="mt-1 font-semibold">{formatMoney(purchaseOrder.subtotalAmount)}</p>
      </div>
      <div>
        <p className="text-xs uppercase text-muted-foreground">GST</p>
        <p className="mt-1 font-semibold">{formatMoney(purchaseOrder.gstAmount)}</p>
      </div>
      <div>
        <p className="text-xs uppercase text-muted-foreground">Total</p>
        <p className="mt-1 font-semibold">{formatMoney(purchaseOrder.totalAmount)}</p>
      </div>
    </div>
  </article>
);
