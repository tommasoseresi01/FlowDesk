namespace FlowDesk.Application.Models.Requests.Customers;

// I campi che creazione e modifica di un cliente hanno in comune (e le stesse regole di validazione).
public interface ICustomerData
{
    string LegalName { get; }
    string VatNumber { get; }
    string ContactName { get; }
    string Email { get; }
    string Phone { get; }
}
