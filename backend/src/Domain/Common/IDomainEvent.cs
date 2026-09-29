namespace TicketWorld.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredAt => DateTime.UtcNow;
    DateTime OccurredOn => OccurredAt;
}

public abstract record DomainEventBase : IDomainEvent
{
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
