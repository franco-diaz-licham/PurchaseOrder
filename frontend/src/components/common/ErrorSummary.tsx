type ErrorSummaryProps = {
  messages: string[];
};

export const ErrorSummary = ({ messages }: ErrorSummaryProps) => {
  const visibleMessages = [...new Set(messages.flatMap((message) => message.split('\n')).filter((message) => message.trim().length > 0))];

  if (visibleMessages.length === 0) return null;

  return (
    <div className="rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
      {visibleMessages.length === 1 ? (
        <span>{visibleMessages[0]}</span>
      ) : (
        <ul className="list-disc space-y-1 pl-5">
          {visibleMessages.map((message) => (
            <li key={message}>{message}</li>
          ))}
        </ul>
      )}
    </div>
  );
};
