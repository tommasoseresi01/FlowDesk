namespace FlowDesk.Application.Utils;

public static class ClaimsNames
{
    public const string USER_ID = "USER_ID";
    public const string USER_EMAIL = "USER_EMAIL";
    public const string ROLE_ID = "ROLE_ID";

    // È il tipo di claim configurato come RoleClaimType: fa funzionare [Authorize(Roles = ...)].
    public const string ROLE_NAME = "ROLE";
}
