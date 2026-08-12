import type { KeyboardEventHandler, MouseEventHandler, ReactNode } from 'react';
import { cn } from '@/lib/cn';

type CellAlign = 'left' | 'right';

type AppTableContainerProps = {
  bordered?: boolean;
  children: ReactNode;
  className?: string;
  maxHeight?: string;
};

type AppTableProps = {
  children: ReactNode;
  className?: string;
  minWidth?: string;
};

type AppTableSectionProps = {
  children: ReactNode;
  className?: string;
  sticky?: boolean;
};

type AppTableRowProps = {
  children: ReactNode;
  className?: string;
  interactive?: boolean;
  onClick?: MouseEventHandler<HTMLTableRowElement>;
  onKeyDown?: KeyboardEventHandler<HTMLTableRowElement>;
  tabIndex?: number;
};

type AppTableHeaderRowProps = {
  children: ReactNode;
  className?: string;
};

type AppTableHeaderCellProps = {
  align?: CellAlign;
  ariaLabel?: string;
  children?: ReactNode;
  className?: string;
  colSpan?: number;
};

type AppTableCellProps = {
  align?: CellAlign;
  children: ReactNode;
  className?: string;
  colSpan?: number;
};

export const AppTableContainer = ({ bordered = false, className, maxHeight, ...props }: AppTableContainerProps) => <div className={cn('overflow-auto rounded-md', bordered && 'border bg-card', className)} style={{ maxHeight }} {...props} />;

export const AppTable = ({ className, minWidth, ...props }: AppTableProps) => <table className={cn('w-full border-separate border-spacing-0 text-left text-sm', className)} style={{ minWidth }} {...props} />;

export const AppTableHead = ({ className, sticky = false, ...props }: AppTableSectionProps) => <thead className={cn('bg-muted text-xs uppercase text-muted-foreground', sticky && 'sticky top-0 z-10 shadow-[0_1px_0_hsl(var(--border))]', className)} {...props} />;

export const AppTableBody = ({ className, ...props }: AppTableSectionProps) => <tbody className={className} {...props} />;

export const AppTableHeaderRow = ({ className, ...props }: AppTableHeaderRowProps) => <tr className={className} {...props} />;

export const AppTableRow = ({ className, interactive = false, ...props }: AppTableRowProps) => <tr className={cn('border-t', interactive && 'cursor-pointer hover:bg-muted/60', className)} {...props} />;

export const AppTableHeaderCell = ({ align = 'left', ariaLabel, className, ...props }: AppTableHeaderCellProps) => <th aria-label={ariaLabel} className={cn('border-b px-4 py-3', align === 'right' && 'text-right', className)} {...props} />;

export const AppTableCell = ({ align = 'left', className, ...props }: AppTableCellProps) => <td className={cn('border-t px-4 py-3', align === 'right' && 'text-right', className)} {...props} />;
