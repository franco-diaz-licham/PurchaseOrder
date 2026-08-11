import { cn } from '@/lib/cn';
import type { InputHTMLAttributes } from 'react';

export const AppInput = ({ className, ...props }: InputHTMLAttributes<HTMLInputElement>) => (
  <input className={cn('min-h-10 rounded-md border bg-white px-3 text-sm outline-none focus:border-primary focus:ring-2 focus:ring-primary/20', className)} {...props} />
);
