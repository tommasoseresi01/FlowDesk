namespace FlowDesk.Application.Models.Requests.Customers;

// Valori del filtro "statusFilter" inviato dalla pagina Clienti.
public static class CustomerStatusFilter
{
    public const string ACTIVE = "active";
    public const string ARCHIVED = "archived";
}

public class CustomerSearchRequest : MaterialReactTableRequest
{
    public string LegalNameFilter => FilterValue("legalNameFilter");
    public string VatNumberFilter => FilterValue("vatNumberFilter");

    // Vuoto significa "tutti".
    public string StatusFilter => FilterValue("statusFilter");
}
