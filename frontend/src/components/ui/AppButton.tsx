import { cn } from '@/lib/cn';
import { cva, type VariantProps } from 'class-variance-authority';
import type { ButtonHTMLAttributes } from 'react';

const buttonVariants = cva(
  'inline-flex min-h-10 cursor-pointer items-center justify-center gap-2 rounded-md px-4 text-sm font-semibold transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring disabled:cursor-not-allowed disabled:opacity-60',
  {
    variants: {
      appearance: {
        primary: 'bg-primary text-primary-foreground hover:bg-teal-800',
        secondary: 'border bg-card text-foreground hover:bg-muted',
        danger: 'bg-destructive text-white hover:bg-red-800',
        ghost: 'text-secondary hover:bg-muted',
        link: 'min-h-0 px-0 text-primary underline-offset-4 hover:underline'
      },
      size: {
        sm: 'min-h-8 px-3 text-xs',
        md: 'min-h-10 px-4 text-sm'
      }
    },
    defaultVariants: {
      appearance: 'primary',
      size: 'sm'
    }
  }
);

type AppButtonProps = ButtonHTMLAttributes<HTMLButtonElement> &
  VariantProps<typeof buttonVariants> & {
    isLoading?: boolean;
  };

export const AppButton = ({ children, className, appearance, disabled, isLoading = false, size, type = 'button', ...props }: AppButtonProps) => (
  <button className={cn(buttonVariants({ appearance, size }), className)} disabled={disabled || isLoading} type={type} {...props}>
    {isLoading && <span className="h-4 w-4 animate-spin rounded-full border-2 border-current/30 border-t-current" />}
    {children}
  </button>
);
