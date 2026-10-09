using FlowDesk.Domain.Entities.Abstractions;

namespace FlowDesk.Domain.Entities;

// Un cliente non si cancella mai: "Deleted" significa archiviato (BR-06) e l'audit
// conserva chi l'ha archiviato e quando.
public class Customer : AuditableEntity, IDeletableEntity
{
    public int IdCustomer { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string VatNumber { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool Deleted { get; set; }
}
