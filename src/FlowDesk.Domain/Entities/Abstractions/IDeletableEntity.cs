namespace FlowDesk.Domain.Entities.Abstractions;

public interface IDeletableEntity
{
    bool Deleted { get; set; }
}
