using FlowDesk.Application.Models.Common;
using FlowDesk.Infrastructure.Extensions;

namespace FlowDesk.UnitTests.Persistence;

public class OrderByColumnTests
{
    private record Author(string Email);

    private record Item(string Name, int Number, Author Author, List<string> Tags);

    private static readonly List<Item> Items =
    [
        new("beta", 2, new Author("z@example.com"), []),
        new("alpha", 3, new Author("m@example.com"), []),
        new("gamma", 1, new Author("a@example.com"), [])
    ];

    private static SortingInfo Sort(string column, SortDirectionEnum direction = SortDirectionEnum.ASCENDING) =>
        new() { ColumnName = column, SortDirection = direction };

    [Fact]
    public void Sorts_ascending_by_a_property_ignoring_the_case_of_its_name()
    {
        var names = Items.AsQueryable().OrderByColumn(Sort("name"))!.Select(i => i.Name).ToList();

        Assert.Equal(["alpha", "beta", "gamma"], names);
    }

    [Fact]
    public void Sorts_descending()
    {
        var numbers = Items.AsQueryable().OrderByColumn(Sort("Number", SortDirectionEnum.DESCENDING))!
            .Select(i => i.Number).ToList();

        Assert.Equal([3, 2, 1], numbers);
    }

    [Fact]
    public void Follows_a_dotted_path_through_a_navigation_property()
    {
        var emails = Items.AsQueryable().OrderByColumn(Sort("Author.Email"))!
            .Select(i => i.Author.Email).ToList();

        Assert.Equal(["a@example.com", "m@example.com", "z@example.com"], emails);
    }

    [Theory]
    [InlineData("doesNotExist")]
    [InlineData("Author.doesNotExist")]
    [InlineData("")]
    public void An_unknown_column_returns_null_so_the_caller_uses_its_default(string column)
    {
        Assert.Null(Items.AsQueryable().OrderByColumn(Sort(column)));
    }

    [Theory]
    [InlineData("Tags")]
    [InlineData("Author")]
    public void Collections_and_objects_are_not_sortable(string column)
    {
        Assert.Null(Items.AsQueryable().OrderByColumn(Sort(column)));
    }

    [Fact]
    public void No_sorting_info_returns_null()
    {
        Assert.Null(Items.AsQueryable().OrderByColumn(null));
    }
}
