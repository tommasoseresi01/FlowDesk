import {FieldValues, Path, UseFormSetError} from 'react-hook-form';
import ApiError from '@app/models/errors/ApiError';

const toFieldName = (field: string): string =>
  field ? field.charAt(0).toLowerCase() + field.slice(1) : 'general';

// Ritorna la funzione che smista un errore:
//  - ApiError con dettagli -> errori "server" sui campi del form (li mostra <ErrorSummary/>)
//  - qualsiasi altro errore -> error boundary della pagina
const useApiError = () => {
  function setErrorFromApiError<T extends FieldValues>(
    e: unknown,
    setError: UseFormSetError<T> | undefined,
    showBoundary: (error: unknown) => void
  ) {
    if (!(e instanceof ApiError) || !setError || e.errors.length === 0) {
      showBoundary(e);
      return;
    }

    const messagesByField: Record<string, string[]> = {};
    e.errors.forEach((err) => {
      const field = toFieldName(err.field);
      messagesByField[field] = [...(messagesByField[field] ?? []), err.message];
    });

    Object.entries(messagesByField).forEach(([field, messages]) => {
      const types: Record<string, string> = {};
      messages.forEach((message, index) => {
        types[`server${index}`] = message;
      });
      setError(field as Path<T>, {type: 'server', types});
    });
  }

  return setErrorFromApiError;
};

export default useApiError;
