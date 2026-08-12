import { formatMoney } from '@/lib/formatMoney';

type FinanceStatsHeaderProps = {
  reportLoadedAt: string;
  totalCommittedValue: number;
  totalReservationCount: number;
  totalReservedQuantity: number;
};

export const FinanceStatsHeader = ({ reportLoadedAt, totalCommittedValue, totalReservationCount, totalReservedQuantity }: FinanceStatsHeaderProps) => (
  <div className="grid gap-4 md:grid-cols-3">
    <div className="rounded-md border bg-card p-4">
      <div className="text-sm text-muted-foreground">Total committed value</div>
      <div className="mt-1 text-2xl font-semibold">{formatMoney(totalCommittedValue)}</div>
    </div>
    <div className="rounded-md border bg-card p-4">
      <div className="text-sm text-muted-foreground">Reserved quantity</div>
      <div className="mt-1 text-2xl font-semibold">{totalReservedQuantity}</div>
    </div>
    <div className="rounded-md border bg-card p-4">
      <div className="text-sm text-muted-foreground">Active reservations</div>
      <div className="mt-1 text-2xl font-semibold">{totalReservationCount}</div>
      <div className="mt-1 text-xs text-muted-foreground">Loaded {reportLoadedAt}</div>
    </div>
  </div>
);
