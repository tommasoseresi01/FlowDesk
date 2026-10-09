using FlowDesk.Application.Models.Dtos;

namespace FlowDesk.Application.Models.Responses.Users;

public class CurrentUserResponse : BaseResponse<UserDto>
{
}

public class UserMenuResponse : BaseResponse<IEnumerable<MenuItemDto>>
{
}
