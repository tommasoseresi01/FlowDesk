using FlowDesk.Application.Models.Common;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Abstractions.Services;

public interface IUserService
{
    // Registra l'accesso: crea l'utente al primo login e allinea nome, email e ruolo a quelli del token.
    Task<ApplicationUser> SignInAsync(SignedInUser signedInUser);

    Task<ApplicationUser> GetUserByIdAsync(int idUser);
}
