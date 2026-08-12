import type { KeyboardEventHandler, MouseEventHandler, ReactNode } from 'react';
import { cn } from '@/lib/cn';

type CellAlign = 'left' | 'right';

type AppTableContainerProps = {
  bordered?: boolean;
  children: ReactNode;
  className?: string;
};

type AppTableProps = {
  children: ReactNode;
  className?: string;
  minWidth?: string;
};

type AppTableSectionProps = {
  children: ReactNode;
  className?: string;
};

type AppTableRowProps = {
  children: ReactNode;
  className?: string;
  interactive?: boolean;
  onClick?: MouseEventHandler<HTMLTableRowElement>;
  onKeyDown?: KeyboardEventHandler<HTMLTableRowElement>;
  tabIndex?: number;
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
};

export const AppTableContainer = ({ bordered = false, className, ...props }: AppTableContainerProps) => <div className={cn('overflow-x-auto', bordered && 'rounded-md border', className)} {...props} />;

export const AppTable = ({ className, minWidth, ...props }: AppTableProps) => <table className={cn('w-full text-left text-sm', className)} style={{ minWidth }} {...props} />;

export const AppTableHead = ({ className, ...props }: AppTableSectionProps) => <thead className={cn('bg-muted text-xs uppercase text-muted-foreground', className)} {...props} />;

export const AppTableBody = ({ className, ...props }: AppTableSectionProps) => <tbody className={className} {...props} />;

export const AppTableRow = ({ className, interactive = false, ...props }: AppTableRowProps) => <tr className={cn('border-t', interactive && 'cursor-pointer hover:bg-muted/60', className)} {...props} />;

export const AppTableHeaderCell = ({ align = 'left', ariaLabel, className, ...props }: AppTableHeaderCellProps) => <th aria-label={ariaLabel} className={cn('px-4 py-3', align === 'right' && 'text-right', className)} {...props} />;

export const AppTableCell = ({ align = 'left', className, ...props }: AppTableCellProps) => <td className={cn('px-4 py-3', align === 'right' && 'text-right', className)} {...props} />;
