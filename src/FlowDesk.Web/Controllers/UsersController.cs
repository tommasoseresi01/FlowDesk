using FlowDesk.Application.Abstractions.Services;
using FlowDesk.Application.Mappers;
using FlowDesk.Application.Models.Responses.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlowDesk.Web.Controllers;

// Il frontend chiama /api/users/...; /api/v1.0/users/... è l'indirizzo versionato equivalente.
[Route("api/[controller]")]
[Route("api/v1.0/[controller]")]
[ApiController]
[Authorize]
public class UsersController(
    IUserService userService,
    IMenuService menuService,
    ICurrentUserService currentUserService) : Controller
{
    // Se il token non è valido o l'utente non è abilitato, qui non si arriva: risponde 401
    // e il frontend mostra "utente non abilitato".
    [HttpGet("current")]
    public async Task<IActionResult> Current()
    {
        var user = await userService.GetUserByIdAsync(currentUserService.GetCurrentUserId());

        return Ok(new CurrentUserResponse()
            .SetResult(UserMapper.ToDto(user))
            .WithSuccess());
    }

    [HttpGet("menu")]
    public IActionResult Menu() =>
        Ok(new UserMenuResponse()
            .SetResult(menuService.GetMenu(currentUserService.UserRole()))
            .WithSuccess());
}
