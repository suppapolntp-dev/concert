namespace TicketWorld.Domain.Repositories;

/// <summary>
/// Unit of Work interface defining the transactional consistency boundary.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IEventRepository Events { get; }
    ISeatRepository Seats { get; }
    IBookingRepository Bookings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
