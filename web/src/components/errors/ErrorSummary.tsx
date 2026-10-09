import {FieldError, FieldErrors, FieldValues} from 'react-hook-form';
import {useTranslation} from 'react-i18next';

type ErrorSummaryProps<T extends FieldValues> = {
  errors: FieldErrors<T>;
};

// Riepilogo in cima al form: elenca i messaggi di validazione client (message)
// e quelli restituiti dal backend (types, impostati da useApiError).
function ErrorSummary<T extends FieldValues>({errors}: ErrorSummaryProps<T>) {
  const [t] = useTranslation();

  const messages: string[] = [];
  Object.values(errors).forEach((value) => {
    const error = value as FieldError | undefined;
    if (!error) return;
    if (error.types) {
      Object.values(error.types).forEach((message) => {
        if (typeof message === 'string') messages.push(message);
      });
    } else if (typeof error.message === 'string') {
      messages.push(error.message);
    }
  });

  if (messages.length === 0) {
    return null;
  }

  return (
    <div className="alert alert-danger" role="alert">
      <h5>
        <i className="icon fas fa-ban" />
        {t('shared.generic.errorSummaryTitle')}
      </h5>
      <ul>
        {messages.map((message, index) => (
          <li key={index}>{message}</li>
        ))}
      </ul>
    </div>
  );
}

export default ErrorSummary;
