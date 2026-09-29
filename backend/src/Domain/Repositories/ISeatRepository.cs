namespace TicketWorld.Domain.Repositories;

using TicketWorld.Domain.Entities;

/// <summary>
/// Repository interface for Seat entity queries and high-concurrency reservation operations.
/// </summary>
public interface ISeatRepository : IRepository<Seat, Guid>
{
    Task<IReadOnlyList<Seat>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Seat>> GetAvailableByIdsForUpdateAsync(IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Seat>> GetHeldExpiredSeatsAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default);
}
