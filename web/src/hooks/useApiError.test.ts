import {describe, expect, test, vi} from 'vitest';
import createApiErrorHandler from '@app/hooks/useApiError';
import ApiError from '@app/models/errors/ApiError';

// useApiError non usa stato React: restituisce una funzione pura, verificabile direttamente.
const setErrorFromApiError = createApiErrorHandler();

describe('useApiError', () => {
  test('binds backend errors to form fields, lower-casing the first letter', () => {
    const setError = vi.fn();
    const showBoundary = vi.fn();
    const error = new ApiError('Error executing api call', [
      {field: 'VatNumber', message: 'Partita IVA già presente'},
      {field: 'VatNumber', message: 'Partita IVA non valida'},
      {field: 'legalName', message: 'Ragione sociale troppo lunga'}
    ]);

    setErrorFromApiError(error, setError, showBoundary);

    expect(showBoundary).not.toHaveBeenCalled();
    expect(setError).toHaveBeenCalledWith('vatNumber', {
      type: 'server',
      types: {
        server0: 'Partita IVA già presente',
        server1: 'Partita IVA non valida'
      }
    });
    expect(setError).toHaveBeenCalledWith('legalName', {
      type: 'server',
      types: {server0: 'Ragione sociale troppo lunga'}
    });
  });

  test('an error without a field is reported under "general"', () => {
    const setError = vi.fn();

    setErrorFromApiError(
      new ApiError('Error executing api call', [
        {field: '', message: 'Il cliente ha pratiche aperte'}
      ]),
      setError,
      vi.fn()
    );

    expect(setError).toHaveBeenCalledWith('general', {
      type: 'server',
      types: {server0: 'Il cliente ha pratiche aperte'}
    });
  });

  test('anything that is not a detailed ApiError goes to the error boundary', () => {
    const setError = vi.fn();
    const showBoundary = vi.fn();
    const unexpected = new Error('Unexpected api call');
    const withoutDetails = new ApiError('Error executing api call', []);

    setErrorFromApiError(unexpected, setError, showBoundary);
    setErrorFromApiError(withoutDetails, setError, showBoundary);

    expect(setError).not.toHaveBeenCalled();
    expect(showBoundary).toHaveBeenCalledWith(unexpected);
    expect(showBoundary).toHaveBeenCalledWith(withoutDetails);
  });
});
