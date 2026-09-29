namespace TicketWorld.Domain.Repositories;

using TicketWorld.Domain.Entities;

/// <summary>
/// Repository interface for Booking aggregate operations.
/// </summary>
public interface IBookingRepository : IRepository<Booking, Guid>
{
    Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default);
    Task<Booking?> GetWithItemsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountActiveByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    Task<Booking?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Booking>> GetUserBookingsAsync(Guid userId, CancellationToken cancellationToken = default);
}
