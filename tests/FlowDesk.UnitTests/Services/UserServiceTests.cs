using FlowDesk.Application.Abstractions.Persistence;
using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Exceptions;
using FlowDesk.Application.Models.Common;
using FlowDesk.Application.Services;
using FlowDesk.Domain.Entities;
using FlowDesk.Domain.Entities.Enums;
using NSubstitute;

namespace FlowDesk.UnitTests.Services;

public class UserServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 9, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid ObjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IUserRepository _repository = Substitute.For<IUserRepository>();
    private readonly UserService _service;

    public UserServiceTests()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.UserRepository.Returns(_repository);

        var dateTimeService = Substitute.For<IDateTimeService>();
        dateTimeService.Now().Returns(Now);

        _service = new UserService(unitOfWork, dateTimeService);
    }

    private static SignedInUser Marta(RoleEnum role = RoleEnum.MANAGER) =>
        new(ObjectId, "marta.belloni@example.com", "Marta", "Belloni", role);

    private static ApplicationUser StoredMarta(DateTime lastLogin, RoleEnum role = RoleEnum.MANAGER) => new()
    {
        IdUser = 7,
        EntraObjectId = ObjectId,
        Email = "marta.belloni@example.com",
        Name = "Marta",
        Surname = "Belloni",
        IdRole = role,
        Enabled = true,
        DateLastLogin = lastLogin
    };

    [Fact]
    public async Task First_sign_in_creates_the_user_with_the_role_from_the_token()
    {
        _repository.GetByEntraObjectIdAsync(ObjectId).Returns((ApplicationUser?)null);
        _repository.CreateAsync(Arg.Any<ApplicationUser>()).Returns(call => call.Arg<ApplicationUser>());

        await _service.SignInAsync(Marta());

        await _repository.Received(1).CreateAsync(Arg.Is<ApplicationUser>(u =>
            u.EntraObjectId == ObjectId
            && u.Email == "marta.belloni@example.com"
            && u.IdRole == RoleEnum.MANAGER
            && u.Enabled
            && u.DateCreation == Now
            && u.DateLastLogin == Now));
    }

    [Fact]
    public async Task A_recent_unchanged_user_is_not_written_again()
    {
        var stored = StoredMarta(lastLogin: Now.AddMinutes(-2));
        _repository.GetByEntraObjectIdAsync(ObjectId).Returns(stored);

        var user = await _service.SignInAsync(Marta());

        Assert.Same(stored, user);
        await _repository.DidNotReceiveWithAnyArgs().UpdateSignInAsync(default, default!, default!, default!, default, default);
    }

    [Fact]
    public async Task A_role_change_in_entra_id_is_written_immediately()
    {
        _repository.GetByEntraObjectIdAsync(ObjectId).Returns(StoredMarta(Now.AddMinutes(-1), RoleEnum.OPERATOR));

        await _service.SignInAsync(Marta(RoleEnum.MANAGER));

        await _repository.Received(1).UpdateSignInAsync(
            7, "marta.belloni@example.com", "Marta", "Belloni", RoleEnum.MANAGER, Now);
    }

    [Fact]
    public async Task A_stale_last_login_is_refreshed()
    {
        _repository.GetByEntraObjectIdAsync(ObjectId).Returns(StoredMarta(Now.AddMinutes(-30)));

        await _service.SignInAsync(Marta());

        await _repository.Received(1).UpdateSignInAsync(
            7, "marta.belloni@example.com", "Marta", "Belloni", RoleEnum.MANAGER, Now);
    }

    [Fact]
    public async Task Getting_an_unknown_user_fails_with_not_found()
    {
        _repository.GetByIdAsync(404).Returns((ApplicationUser?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetUserByIdAsync(404));
    }
}
