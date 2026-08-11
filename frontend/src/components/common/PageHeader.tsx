import type { ReactNode } from 'react';

type PageHeaderProps = {
  title: string;
  description: string;
  children?: ReactNode;
};

export const PageHeader = ({ title, description, children }: PageHeaderProps) => (
  <div className="flex flex-col gap-4 border-b bg-card px-6 py-5 md:flex-row md:items-center md:justify-between">
    <div>
      <h1 className="text-2xl font-semibold text-foreground">{title}</h1>
      <p className="mt-1 max-w-3xl text-sm text-muted-foreground">{description}</p>
    </div>
    {children}
  </div>
);
