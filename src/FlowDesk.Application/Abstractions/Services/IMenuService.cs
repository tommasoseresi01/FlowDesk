using FlowDesk.Application.Models.Dtos;
using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Application.Abstractions.Services;

public interface IMenuService
{
    // Il menu laterale del frontend, già filtrato in base al ruolo.
    IReadOnlyList<MenuItemDto> GetMenu(RoleEnum role);
}
