using FlowDesk.Application.Models.Common;

namespace FlowDesk.Application.Models.Requests;

public class MaterialReactTableFilter
{
    public string Id { get; set; } = string.Empty;
    public string? Value { get; set; }
}

public class MaterialReactTableSorting
{
    public string Id { get; set; } = string.Empty;
    public bool Desc { get; set; }
}

// Ricalca lo stato della griglia del frontend (material-react-table).
public class MaterialReactTableRequest
{
    public int Start { get; set; }
    public int Size { get; set; }
    public List<MaterialReactTableFilter> Filters { get; set; } = [];
    public string GlobalFilter { get; set; } = string.Empty;
    public List<MaterialReactTableSorting> Sorting { get; set; } = [];

    // Il valore del filtro con quell'id, oppure stringa vuota se non è stato inviato.
    protected string FilterValue(string id) =>
        Filters.FirstOrDefault(f => f.Id == id)?.Value?.Trim() ?? string.Empty;

    // Si ordina per una sola colonna: la prima.
    public SortingInfo? ToSortingInfo()
    {
        var first = Sorting.FirstOrDefault();
        if (first is null || string.IsNullOrWhiteSpace(first.Id))
        {
            return null;
        }

        return new SortingInfo
        {
            ColumnName = first.Id,
            SortDirection = first.Desc
                ? SortDirectionEnum.DESCENDING
                : SortDirectionEnum.ASCENDING
        };
    }
}
