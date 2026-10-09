import ApiError from '@app/models/errors/ApiError';
import ApiResponse from '@app/models/responses/ApiResponse';
import {apiFetch} from '@app/utils/apiFetch';

export type ApiFetch = typeof apiFetch;
type ExecFn<T> = (api: ApiFetch) => Promise<ApiResponse<T>>;

export class BaseRepository {
  protected static readonly baseUrl: string = import.meta.env.VITE_API_URL;

  // Esegue la chiamata e traduce l'envelope: success=false -> ApiError, tutto il resto -> Error.
  private static async run<T>(execFn: ExecFn<T>): Promise<ApiResponse<T>> {
    let data: ApiResponse<T>;
    try {
      data = await execFn(apiFetch);
    } catch (e) {
      throw new Error('Unexpected api call', {cause: e});
    }

    if (!data || data.success === false) {
      throw new ApiError('Error executing api call', data?.errors ?? []);
    }
    return data;
  }

  static async execApi<T>(execFn: ExecFn<T>): Promise<T> {
    const data = await this.run(execFn);
    return data.result;
  }

  static async execPaginationApi<T>(
    execFn: ExecFn<T[]>
  ): Promise<[T[], number]> {
    const data = await this.run(execFn);
    return [data.result, data.totResultNumber ?? 0];
  }
}
