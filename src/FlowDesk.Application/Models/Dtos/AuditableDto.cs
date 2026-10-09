using FlowDesk.Domain.Entities;

namespace FlowDesk.Application.Models.Dtos;

public abstract class AuditableDto
{
    public DateTime DateCreation { get; set; }
    public DateTime DateModification { get; set; }
    public AuditUserDto? UserCreation { get; set; }
    public AuditUserDto? UserModification { get; set; }

    public void SetAuditableUsers(ApplicationUser? creation, ApplicationUser? modification)
    {
        UserCreation = ToAuditUser(creation);
        UserModification = ToAuditUser(modification);
    }

    private static AuditUserDto? ToAuditUser(ApplicationUser? user) =>
        user is null
            ? null
            : new AuditUserDto
            {
                IdUser = user.IdUser,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname
            };
}
