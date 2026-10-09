namespace FlowDesk.Domain.Entities;

public abstract class AuditableEntity
{
    public int IdUserCreation { get; set; }
    public DateTime DateCreation { get; set; }
    public int IdUserModification { get; set; }
    public DateTime DateModification { get; set; }
    public int? IdUserDeleted { get; set; }
    public DateTime? DateDeleted { get; set; }

    public virtual ApplicationUser UserCreation { get; set; } = null!;
    public virtual ApplicationUser UserModification { get; set; } = null!;
    public virtual ApplicationUser? UserDeleted { get; set; }
}
