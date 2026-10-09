using FlowDesk.Application.Models.Dtos;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(ApplicationUser entity) => new()
    {
        IdUser = entity.IdUser,
        Name = entity.Name,
        Surname = entity.Surname,
        Email = entity.Email,
        Enabled = entity.Enabled,
        Role = new RoleDto
        {
            IdRole = (int)entity.IdRole,
            RoleName = entity.Role?.RoleName ?? entity.IdRole.ToString()
        }
    };
}
