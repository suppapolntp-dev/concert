namespace TicketWorld.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedAt { get => DateTime.UtcNow; set { } }
    DateTime UpdatedAt { get => DateTime.UtcNow; set { } }
}
