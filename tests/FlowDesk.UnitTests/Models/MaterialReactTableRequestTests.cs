using FlowDesk.Application.Models.Common;
using FlowDesk.Application.Models.Requests;
using FlowDesk.Application.Models.Requests.Customers;

namespace FlowDesk.UnitTests.Models;

public class MaterialReactTableRequestTests
{
    [Fact]
    public void Filters_are_read_by_the_ids_the_frontend_sends()
    {
        var request = new CustomerSearchRequest
        {
            Filters =
            [
                new MaterialReactTableFilter { Id = "legalNameFilter", Value = "  Panificio " },
                new MaterialReactTableFilter { Id = "vatNumberFilter", Value = "0123" },
                new MaterialReactTableFilter { Id = "statusFilter", Value = "active" }
            ]
        };

        Assert.Equal("Panificio", request.LegalNameFilter);
        Assert.Equal("0123", request.VatNumberFilter);
        Assert.Equal(CustomerStatusFilter.ACTIVE, request.StatusFilter);
    }

    [Fact]
    public void A_filter_that_was_not_sent_is_empty()
    {
        var request = new CustomerSearchRequest();

        Assert.Equal(string.Empty, request.LegalNameFilter);
        Assert.Equal(string.Empty, request.StatusFilter);
    }

    [Fact]
    public void A_null_filter_value_is_treated_as_empty()
    {
        var request = new CustomerSearchRequest
        {
            Filters = [new MaterialReactTableFilter { Id = "legalNameFilter", Value = null }]
        };

        Assert.Equal(string.Empty, request.LegalNameFilter);
    }

    [Fact]
    public void Only_the_first_sorting_column_is_used()
    {
        var request = new MaterialReactTableRequest
        {
            Sorting =
            [
                new MaterialReactTableSorting { Id = "legalName", Desc = true },
                new MaterialReactTableSorting { Id = "vatNumber", Desc = false }
            ]
        };

        var sorting = request.ToSortingInfo();

        Assert.NotNull(sorting);
        Assert.Equal("legalName", sorting.ColumnName);
        Assert.Equal(SortDirectionEnum.DESCENDING, sorting.SortDirection);
    }

    [Fact]
    public void Without_sorting_there_is_no_sorting_info()
    {
        Assert.Null(new MaterialReactTableRequest().ToSortingInfo());
    }
}
