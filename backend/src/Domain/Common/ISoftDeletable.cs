namespace TicketWorld.Domain.Common;

public interface ISoftDeletable
{
    DateTime? DeletedAt { get => null; set { } }
    bool IsDeleted => DeletedAt.HasValue;
}
