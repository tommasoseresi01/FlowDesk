using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Application.Models.Common;

// Ciò che il token di Entra ID dice dell'utente che si sta autenticando.
public record SignedInUser(
    Guid EntraObjectId,
    string Email,
    string Name,
    string Surname,
    RoleEnum Role);
