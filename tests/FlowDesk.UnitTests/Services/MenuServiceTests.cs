using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.UnitTests.Services;

public class MenuServiceTests
{
    private readonly MenuService _service = new();

    [Fact]
    public void The_admin_only_sees_home_because_they_do_not_work_on_customers()
    {
        var menu = _service.GetMenu(RoleEnum.ADMIN);

        Assert.Equal(["/home"], menu.Select(m => m.Path));
    }

    [Theory]
    [InlineData(RoleEnum.MANAGER)]
    [InlineData(RoleEnum.OPERATOR)]
    public void Managers_and_operators_see_home_and_customers(RoleEnum role)
    {
        var menu = _service.GetMenu(role);

        Assert.Equal(["/home", "/customers"], menu.Select(m => m.Path));
    }

    [Fact]
    public void Menu_entries_use_the_translation_keys_of_the_frontend()
    {
        var menu = _service.GetMenu(RoleEnum.MANAGER);

        Assert.Equal(
            ["menusidebar.label.home", "menusidebar.label.customers"],
            menu.Select(m => m.Name));
    }
}
