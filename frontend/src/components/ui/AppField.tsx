import type { ReactNode } from 'react';

type AppFieldProps = {
  label: string;
  children: ReactNode;
};

export const AppField = ({ label, children }: AppFieldProps) => (
  <label className="grid gap-1.5 text-sm font-medium text-secondary">
    <span>{label}</span>
    {children}
  </label>
);
