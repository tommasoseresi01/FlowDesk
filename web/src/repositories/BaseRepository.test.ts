import {describe, expect, test, vi} from 'vitest';
import ApiError from '@app/models/errors/ApiError';
import {BaseRepository} from '@app/repositories/BaseRepository';

// Il vero apiFetch richiede MSAL e un browser: qui interessa solo la lettura dell'envelope.
vi.mock('@app/utils/apiFetch', () => ({apiFetch: vi.fn()}));

describe('BaseRepository', () => {
  test('execApi returns the result of a successful envelope', async () => {
    const result = await BaseRepository.execApi(async () => ({
      success: true,
      result: {idCustomer: 1}
    }));

    expect(result).toEqual({idCustomer: 1});
  });

  test('execPaginationApi returns the rows with the total count', async () => {
    const [rows, total] = await BaseRepository.execPaginationApi(async () => ({
      success: true,
      result: [{idCustomer: 1}, {idCustomer: 2}],
      totResultNumber: 137
    }));

    expect(rows).toHaveLength(2);
    expect(total).toBe(137);
  });

  test('an unsuccessful envelope becomes an ApiError with the field errors', async () => {
    const errors = [{field: 'VatNumber', message: 'Partita IVA già presente'}];

    const call = BaseRepository.execApi(async () => ({
      success: false,
      result: null,
      errors
    }));

    await expect(call).rejects.toBeInstanceOf(ApiError);
    await expect(call).rejects.toMatchObject({errors});
  });

  test('a failed call becomes a generic error that keeps the cause', async () => {
    const cause = new TypeError('Failed to fetch');

    const call = BaseRepository.execApi(async () => {
      throw cause;
    });

    await expect(call).rejects.not.toBeInstanceOf(ApiError);
    await expect(call).rejects.toMatchObject({cause});
  });
});
