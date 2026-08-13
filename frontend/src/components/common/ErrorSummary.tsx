import { UilTimes } from '@iconscout/react-unicons';
import { AppButton } from '@/components/ui/AppButton';

type ErrorSummaryProps = {
  messages: string[];
  onDismiss?: () => void;
};

export const ErrorSummary = ({ messages, onDismiss }: ErrorSummaryProps) => {
  const visibleMessages = [...new Set(messages.flatMap((message) => message.split('\n')).filter((message) => message.trim().length > 0))];

  if (visibleMessages.length === 0) return null;

  return (
    <div className="flex items-start justify-between gap-3 rounded-md border border-red-200 bg-red-50 p-3 text-sm text-red-800">
      <div>
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
      {onDismiss && (
        <AppButton aria-label="Dismiss errors" appearance="ghost" className="text-red-800 hover:bg-red-100" onClick={onDismiss} type="button">
          <UilTimes className="h-4 w-4" />
        </AppButton>
      )}
    </div>
  );
};
