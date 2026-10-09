import {describe, expect, test} from 'vitest';
import {CustomerMapper} from '@app/mappers/CustomerMapper';

describe('CustomerMapper', () => {
  test('maps the API record and parses the modification date', () => {
    const customer = CustomerMapper({
      idCustomer: 7,
      legalName: 'Panificio Delle Valli S.n.c.',
      vatNumber: '01234567890',
      contactName: 'Irene Valli',
      email: 'amministrazione@panificiodellevalli.example',
      phone: '035 555 0142',
      archived: true,
      userModification: {
        idUser: 2,
        email: 'marta.belloni@meridiana.example',
        name: 'Marta',
        surname: 'Belloni'
      },
      dateModification: '2026-10-08T14:30:00'
    });

    expect(customer.idCustomer).toBe(7);
    expect(customer.legalName).toBe('Panificio Delle Valli S.n.c.');
    expect(customer.archived).toBe(true);
    expect(customer.userModification?.email).toBe(
      'marta.belloni@meridiana.example'
    );
    expect(customer.dateModification).toEqual(new Date(2026, 9, 8, 14, 30, 0));
  });

  test('replaces missing optional values so the UI never handles null', () => {
    const customer = CustomerMapper({
      idCustomer: 3,
      legalName: 'Ottica Vescovi',
      vatNumber: '09876543210',
      contactName: null,
      email: null,
      phone: null,
      dateModification: null
    });

    expect(customer.contactName).toBe('');
    expect(customer.email).toBe('');
    expect(customer.phone).toBe('');
    expect(customer.archived).toBe(false);
    expect(customer.dateModification).toBeNull();
  });
});
