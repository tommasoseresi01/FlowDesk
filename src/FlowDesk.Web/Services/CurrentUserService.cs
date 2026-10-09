using System.Security.Claims;
using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Utils;
using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Web.Services;

// Legge i claim applicativi aggiunti dopo la validazione del token (vedi JwtAuthenticationExtensions).
public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public int GetCurrentUserId() =>
        int.TryParse(User?.FindFirstValue(ClaimsNames.USER_ID), out var idUser) ? idUser : 0;

    public RoleEnum UserRole() =>
        int.TryParse(User?.FindFirstValue(ClaimsNames.ROLE_ID), out var idRole)
        && Enum.IsDefined(typeof(RoleEnum), idRole)
            ? (RoleEnum)idRole
            : throw new UnauthorizedAccessException("L'utente corrente non ha un ruolo applicativo.");
}
