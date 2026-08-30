interface ErrorMessageProps {
  message: string;
  isNotFound?: boolean;
}

export function ErrorMessage({ message, isNotFound = false }: ErrorMessageProps) {
  return (
    <div
      className={`error-message ${isNotFound ? 'error-message--not-found' : 'error-message--server'}`}
      role="alert"
      aria-live="assertive"
    >
      <div className="error-message__icon" aria-hidden="true">
        {isNotFound ? (
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="40"
            height="40"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <circle cx="11" cy="11" r="8" />
            <line x1="21" y1="21" x2="16.65" y2="16.65" />
            <line x1="8" y1="11" x2="14" y2="11" />
          </svg>
        ) : (
          <svg
            xmlns="http://www.w3.org/2000/svg"
            width="40"
            height="40"
            viewBox="0 0 24 24"
            fill="none"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
            strokeLinejoin="round"
          >
            <circle cx="12" cy="12" r="10" />
            <line x1="12" y1="8" x2="12" y2="12" />
            <line x1="12" y1="16" x2="12.01" y2="16" />
          </svg>
        )}
      </div>
      <p className="error-message__text">{message}</p>
    </div>
  );
}
