import ApplicationErrorDto from '@app/models/dtos/ApplicationErrorDto';

type ApiResponse<T> = {
  success: boolean;
  result: T;
  errors?: ApplicationErrorDto[];
  totResultNumber?: number;
};

export default ApiResponse;
