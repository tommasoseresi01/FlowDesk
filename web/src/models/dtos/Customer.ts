import AuditUser from '@app/models/dtos/AuditUser';

type Customer = {
  idCustomer: number;
  legalName: string;
  vatNumber: string;
  contactName: string;
  email: string;
  phone: string;
  // Un cliente non si cancella: si archivia, e le sue pratiche restano consultabili.
  archived: boolean;
  userModification?: AuditUser;
  dateModification?: Date | null;
};

export default Customer;
