using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Entities.Enums;

namespace FlowDesk.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(int idUser);
    Task<ApplicationUser?> GetByEntraObjectIdAsync(Guid entraObjectId);

    // Se due richieste creano lo stesso utente nello stesso istante, la seconda restituisce quello già creato.
    Task<ApplicationUser> CreateAsync(ApplicationUser toCreate);

    Task<ApplicationUser> UpdateSignInAsync(
        int idUser,
        string email,
        string name,
        string surname,
        RoleEnum role,
        DateTime lastLogin);
}
