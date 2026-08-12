import { formatMoney } from '@/lib/formatMoney';

type PurchaseOrderTotalsBarProps = {
  gstAmount: number;
  subtotalAmount: number;
  totalAmount: number;
};

export const PurchaseOrderTotalsBar = ({ gstAmount, subtotalAmount, totalAmount }: PurchaseOrderTotalsBarProps) => (
  <div className="grid gap-4 md:grid-cols-3">
    <div className="rounded-md border bg-card p-4">
      <div className="text-sm text-muted-foreground">Subtotal</div>
      <div className="mt-2 text-2xl font-semibold">{formatMoney(subtotalAmount)}</div>
    </div>
    <div className="rounded-md border bg-card p-4">
      <div className="text-sm text-muted-foreground">GST</div>
      <div className="mt-2 text-2xl font-semibold">{formatMoney(gstAmount)}</div>
    </div>
    <div className="rounded-md border bg-card p-4">
      <div className="text-sm text-muted-foreground">Total</div>
      <div className="mt-2 text-2xl font-semibold">{formatMoney(totalAmount)}</div>
    </div>
  </div>
);
