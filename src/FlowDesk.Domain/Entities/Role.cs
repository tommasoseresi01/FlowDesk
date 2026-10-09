using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Domain.Entities;

// Tabella di lookup: le righe esistono nel database con gli stessi id di RoleEnum.
public class Role
{
    public RoleEnum IdRole { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
