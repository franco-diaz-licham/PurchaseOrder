import { cn } from '@/lib/cn';
import type { SelectHTMLAttributes } from 'react';

export type AppSelectOption = {
  label: string;
  value: string;
};

type AppSelectProps = SelectHTMLAttributes<HTMLSelectElement> & {
  options?: AppSelectOption[];
  placeholder?: string;
};

export const AppSelect = ({ children, className, options, placeholder, ...props }: AppSelectProps) => (
  <select className={cn('min-h-10 rounded-md border bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20', className)} {...props}>
    {placeholder && <option value="">{placeholder}</option>}
    {options?.map((option) => (
      <option key={option.value} value={option.value}>
        {option.label}
      </option>
    ))}
    {children}
  </select>
);
