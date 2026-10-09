namespace FlowDesk.Application.Models.Common;

public class PaginatedList<T> : List<T>
{
    // Numero totale di righe che soddisfano i filtri, indipendentemente dalla pagina richiesta.
    public int TotNum { get; set; }
}
