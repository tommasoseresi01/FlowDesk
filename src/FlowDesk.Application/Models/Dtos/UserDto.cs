namespace FlowDesk.Application.Models.Dtos;

public class UserDto
{
    public int IdUser { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public RoleDto Role { get; set; } = new();
}
