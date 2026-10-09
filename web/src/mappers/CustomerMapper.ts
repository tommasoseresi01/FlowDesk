import Customer from '@app/models/dtos/Customer';
import {parseDate} from '@app/utils/dateUtils';

export function CustomerMapper(item: any): Customer {
  return {
    idCustomer: item.idCustomer,
    legalName: item.legalName ?? '',
    vatNumber: item.vatNumber ?? '',
    contactName: item.contactName ?? '',
    email: item.email ?? '',
    phone: item.phone ?? '',
    archived: item.archived ?? false,
    userModification: item.userModification,
    dateModification: parseDate(item.dateModification)
  };
}
