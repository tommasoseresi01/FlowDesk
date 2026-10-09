namespace FlowDesk.Domain.Entities.Enums;

// I valori devono coincidere con gli idRole che il frontend si aspetta (RoleEnum.ts)
// e con le righe della tabella Roles.
public enum RoleEnum
{
    ADMIN = 1,
    MANAGER = 2,
    OPERATOR = 3
}
