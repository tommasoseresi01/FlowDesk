using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Models.Dtos;
using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Application.Services;

public class MenuService : IMenuService
{
    private sealed record MenuEntry(MenuItemDto Item, RoleEnum[] Roles);

    // Le voci di menu usano le chiavi di traduzione e i percorsi delle rotte del frontend.
    // L'amministratore configura il sistema ma non lavora su clienti e pratiche.
    private static readonly MenuEntry[] Entries =
    [
        new(
            new MenuItemDto { Name = "menusidebar.label.home", Icon = "nav-icon fas fa-home", Path = "/home" },
            [RoleEnum.ADMIN, RoleEnum.MANAGER, RoleEnum.OPERATOR]),
        new(
            new MenuItemDto { Name = "menusidebar.label.customers", Icon = "nav-icon fas fa-building", Path = "/customers" },
            [RoleEnum.MANAGER, RoleEnum.OPERATOR])
    ];

    public IReadOnlyList<MenuItemDto> GetMenu(RoleEnum role) =>
        Entries
            .Where(entry => entry.Roles.Contains(role))
            .Select(entry => entry.Item)
            .ToList();
}
