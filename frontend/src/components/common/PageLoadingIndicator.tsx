type PageLoadingIndicatorProps = {
  message?: string;
};

export const PageLoadingIndicator = ({ message = 'Loading data...' }: PageLoadingIndicatorProps) => (
  <div className="flex min-h-screen items-center justify-center bg-background" role="status" aria-label={message}>
    <span className="h-16 w-16 animate-spin rounded-full border-4 border-primary/20 border-t-primary" />
  </div>
);
