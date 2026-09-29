namespace TicketWorld.Domain.Common;

public abstract class AggregateRoot<TId> : BaseEntity<TId>
{
}

public abstract class AggregateRoot : AggregateRoot<Guid>
{
}
