using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Application.Abstractions.Services;

// L'utente che sta eseguendo la richiesta. L'interfaccia sta in Application così i servizi e
// l'audit del DbContext la usano senza conoscere HttpContext.
public interface ICurrentUserService
{
    // 0 se la richiesta non ha un utente applicativo.
    int GetCurrentUserId();

    RoleEnum UserRole();
}
