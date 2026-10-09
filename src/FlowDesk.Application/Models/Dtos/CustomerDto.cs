namespace FlowDesk.Application.Models.Dtos;

public class CustomerDto : AuditableDto
{
    public int IdCustomer { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Il frontend lo chiama "archived"; nel database è la cancellazione logica.
    public bool Archived { get; set; }
}
