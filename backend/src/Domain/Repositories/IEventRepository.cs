namespace TicketWorld.Domain.Repositories;

using TicketWorld.Domain.Entities;

/// <summary>
/// Repository interface for Event aggregate operations.
/// </summary>
public interface IEventRepository : IRepository<Event, Guid>
{
    Task<Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Event?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Event>> GetPublishedEventsAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default);
}
