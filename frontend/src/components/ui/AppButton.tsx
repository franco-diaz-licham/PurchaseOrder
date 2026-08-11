import { cn } from '@/lib/cn';
import { cva, type VariantProps } from 'class-variance-authority';
import type { ButtonHTMLAttributes } from 'react';

const buttonVariants = cva(
  'inline-flex min-h-10 items-center justify-center gap-2 rounded-md px-4 text-sm font-semibold transition focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring disabled:cursor-not-allowed disabled:opacity-60',
  {
    variants: {
      appearance: {
        primary: 'bg-primary text-primary-foreground hover:bg-teal-800',
        secondary: 'border bg-card text-foreground hover:bg-muted',
        danger: 'bg-destructive text-white hover:bg-red-800',
        ghost: 'text-secondary hover:bg-muted'
      },
      size: {
        sm: 'min-h-8 px-3 text-xs',
        md: 'min-h-10 px-4 text-sm'
      }
    },
    defaultVariants: {
      appearance: 'primary',
      size: 'md'
    }
  }
);

type AppButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & VariantProps<typeof buttonVariants>;

export const AppButton = ({ className, appearance, size, type = 'button', ...props }: AppButtonProps) => <button className={cn(buttonVariants({ appearance, size }), className)} type={type} {...props} />;
