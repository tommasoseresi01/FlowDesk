namespace FlowDesk.Application.Models.Requests.Customers;

public class CreateCustomerRequest : ICustomerData
{
    public string LegalName { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
