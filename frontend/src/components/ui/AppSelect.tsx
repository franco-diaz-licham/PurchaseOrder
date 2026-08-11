import { cn } from '@/lib/cn';
import type { SelectHTMLAttributes } from 'react';

export const AppSelect = ({ className, ...props }: SelectHTMLAttributes<HTMLSelectElement>) => (
  <select className={cn('min-h-10 rounded-md border bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20', className)} {...props} />
);
