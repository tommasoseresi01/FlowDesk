import ApplicationErrorDto from '@app/models/dtos/ApplicationErrorDto';

// Errore applicativo restituito dal backend (success=false), con il dettaglio per campo.
class ApiError extends Error {
  errors: ApplicationErrorDto[];

  constructor(message: string, errors: ApplicationErrorDto[]) {
    super(message);
    this.name = 'ApiError';
    this.errors = errors;
  }
}

export default ApiError;
