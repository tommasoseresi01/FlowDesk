import {getAccessToken} from '@app/utils/getAccessToken';
import ApiResponse from '@app/models/responses/ApiResponse';

export type ApiFetchResult<T> = {
  response: Response;
  data: ApiResponse<T>;
};

// Unico punto in cui l'applicazione chiama fetch: aggiunge il Bearer token e legge l'envelope JSON.
export async function apiFetch<T = unknown>(
  url: string,
  config: RequestInit = {}
): Promise<ApiFetchResult<T>> {
  const accessToken = await getAccessToken();

  const response = await fetch(url, {
    ...config,
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${accessToken}`
    }
  });
  const data = (await response.json()) as ApiResponse<T>;

  return {response, data};
}
