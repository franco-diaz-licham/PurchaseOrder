type EmptyStateProps = {
  title: string;
};

export const EmptyState = ({ title }: EmptyStateProps) => <div className="rounded-md border border-dashed bg-card p-8 text-center text-sm text-muted-foreground">{title}</div>;
