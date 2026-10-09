namespace FlowDesk.Application.Models.Common;

public enum SortDirectionEnum
{
    ASCENDING = 1,
    DESCENDING = 2
}

public class SortingInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public SortDirectionEnum SortDirection { get; set; }
}
