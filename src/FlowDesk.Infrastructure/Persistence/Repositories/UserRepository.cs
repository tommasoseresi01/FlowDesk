using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Entities.Enums;
using FlowDesk.Infrastructure.Extensions;
using FlowDesk.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace FlowDesk.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<ApplicationUser?> GetByIdAsync(int idUser) =>
        await context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.IdUser == idUser);

    public async Task<ApplicationUser?> GetByEntraObjectIdAsync(Guid entraObjectId) =>
        await context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.EntraObjectId == entraObjectId);

    public async Task<ApplicationUser> CreateAsync(ApplicationUser toCreate)
    {
        await context.Users.AddAsync(toCreate);
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Un'altra richiesta ha creato lo stesso utente un istante prima: si usa quello.
            context.Entry(toCreate).State = EntityState.Detached;
            return await GetByEntraObjectIdAsync(toCreate.EntraObjectId)
                ?? throw new InvalidOperationException("User creation conflict could not be resolved", ex);
        }

        return (await GetByIdAsync(toCreate.IdUser))!;
    }

    public async Task<ApplicationUser> UpdateSignInAsync(
        int idUser,
        string email,
        string name,
        string surname,
        RoleEnum role,
        DateTime lastLogin)
    {
        var user = await context.Users.FirstAsync(u => u.IdUser == idUser);
        user.Email = email;
        user.Name = name;
        user.Surname = surname;
        user.IdRole = role;
        user.DateLastLogin = lastLogin;

        await context.SaveChangesAsync();
        return (await GetByIdAsync(idUser))!;
    }
}
