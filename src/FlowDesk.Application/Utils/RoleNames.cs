using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Application.Utils;

// Costanti per gli attributi [Authorize(Roles = ...)]: i nomi coincidono con i membri di RoleEnum.
public static class RoleNames
{
    public const string ADMIN = nameof(RoleEnum.ADMIN);
    public const string MANAGER = nameof(RoleEnum.MANAGER);
    public const string OPERATOR = nameof(RoleEnum.OPERATOR);

    public const string MANAGER_OR_OPERATOR = MANAGER + "," + OPERATOR;
}
