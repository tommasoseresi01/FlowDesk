using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Exceptions;
using FlowDesk.Application.Models.Common;
using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Services;

public class UserService(IUnitOfWork unitOfWork, IDateTimeService dateTimeService) : IUserService
{
    // L'ultimo accesso si riscrive al massimo ogni quarto d'ora: ogni richiesta passa di qui.
    private static readonly TimeSpan LastLoginRefreshInterval = TimeSpan.FromMinutes(15);

    public async Task<ApplicationUser> SignInAsync(SignedInUser signedInUser)
    {
        var now = dateTimeService.Now();
        var repository = unitOfWork.UserRepository;

        var user = await repository.GetByEntraObjectIdAsync(signedInUser.EntraObjectId);
        if (user is null)
        {
            return await repository.CreateAsync(new ApplicationUser
            {
                EntraObjectId = signedInUser.EntraObjectId,
                Email = signedInUser.Email,
                Name = signedInUser.Name,
                Surname = signedInUser.Surname,
                IdRole = signedInUser.Role,
                Enabled = true,
                DateCreation = now,
                DateLastLogin = now
            });
        }

        var isChanged = user.Email != signedInUser.Email
            || user.Name != signedInUser.Name
            || user.Surname != signedInUser.Surname
            || user.IdRole != signedInUser.Role;
        var isStale = now - user.DateLastLogin > LastLoginRefreshInterval;

        if (!isChanged && !isStale)
        {
            return user;
        }

        return await repository.UpdateSignInAsync(
            user.IdUser,
            signedInUser.Email,
            signedInUser.Name,
            signedInUser.Surname,
            signedInUser.Role,
            now);
    }

    public async Task<ApplicationUser> GetUserByIdAsync(int idUser)
    {
        var user = await unitOfWork.UserRepository.GetByIdAsync(idUser);
        return user ?? throw new NotFoundException($"User {idUser} not found");
    }
}
