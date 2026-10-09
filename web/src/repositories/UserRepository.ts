import {BaseRepository} from '@app/repositories/BaseRepository';

export class UserRepository extends BaseRepository {
  static async getCurrentUser(): Promise<unknown> {
    return this.execApi<unknown>(async (api) => {
      const {data} = await api<unknown>(`${this.baseUrl}/users/current`, {
        method: 'GET'
      });
      return data;
    });
  }

  static async getMenu(): Promise<unknown[]> {
    return this.execApi<unknown[]>(async (api) => {
      const {data} = await api<unknown[]>(`${this.baseUrl}/users/menu`, {
        method: 'GET'
      });
      return data;
    });
  }
}
