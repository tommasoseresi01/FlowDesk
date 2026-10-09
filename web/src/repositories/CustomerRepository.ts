import {MRT_SortingState} from 'material-react-table';
import CustomFilter from '@app/models/dtos/CustomFilter';
import Customer from '@app/models/dtos/Customer';
import {BaseRepository} from '@app/repositories/BaseRepository';
import {API_DATE_FORMAT, formatDate} from '@app/utils/dateUtils';

export class CustomerRepository extends BaseRepository {
  static async search(
    start: number,
    pageSize: number,
    filters: CustomFilter[],
    globalFilter: string,
    sorting: MRT_SortingState
  ): Promise<[unknown[], number]> {
    return this.execPaginationApi<unknown>(async (api) => {
      const {data} = await api<unknown[]>(`${this.baseUrl}/customers/search`, {
        method: 'POST',
        body: JSON.stringify({
          start,
          size: pageSize,
          filters,
          globalFilter,
          sorting: sorting ?? []
        })
      });
      return data;
    });
  }

  static async getById(id: number): Promise<unknown> {
    // id = 0 -> nuovo record: nessuna chiamata, si parte da un record vuoto
    if (id === 0) {
      return {
        idCustomer: 0,
        legalName: '',
        vatNumber: '',
        contactName: '',
        email: '',
        phone: '',
        archived: false,
        dateModification: formatDate(new Date(), API_DATE_FORMAT)
      };
    }

    return this.execApi<unknown>(async (api) => {
      const {data} = await api<unknown>(`${this.baseUrl}/customers/${id}`, {
        method: 'GET'
      });
      return data;
    });
  }

  static async create(customer: Customer): Promise<unknown> {
    return this.execApi<unknown>(async (api) => {
      const {data} = await api<unknown>(`${this.baseUrl}/customers`, {
        method: 'POST',
        body: JSON.stringify({
          legalName: customer.legalName,
          vatNumber: customer.vatNumber,
          contactName: customer.contactName,
          email: customer.email,
          phone: customer.phone
        })
      });
      return data;
    });
  }

  static async edit(customer: Customer): Promise<unknown> {
    return this.execApi<unknown>(async (api) => {
      const {data} = await api<unknown>(`${this.baseUrl}/customers`, {
        method: 'PUT',
        body: JSON.stringify({
          idCustomer: customer.idCustomer,
          legalName: customer.legalName,
          vatNumber: customer.vatNumber,
          contactName: customer.contactName,
          email: customer.email,
          phone: customer.phone
        })
      });
      return data;
    });
  }

  // Per i clienti DELETE significa archiviare: il backend non cancella mai il record.
  static async delete(id: number): Promise<unknown> {
    return this.execApi<unknown>(async (api) => {
      const {data} = await api<unknown>(`${this.baseUrl}/customers/${id}`, {
        method: 'DELETE'
      });
      return data;
    });
  }

  static async restore(id: number): Promise<unknown> {
    return this.execApi<unknown>(async (api) => {
      const {data} = await api<unknown>(
        `${this.baseUrl}/customers/${id}/restore`,
        {method: 'POST'}
      );
      return data;
    });
  }
}
