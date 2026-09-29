namespace TicketWorld.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Repositories;

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Booking?> GetByReferenceAsync(string bookingReference, CancellationToken cancellationToken = default)
    {
        var normalized = bookingReference.Trim().ToUpperInvariant();
        return await DbSet
            .Include(b => b.Event)
            .Include(b => b.Items)
                .ThenInclude(i => i.Seat)
            .Include(b => b.Items)
                .ThenInclude(i => i.Tier)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.BookingReference == normalized, cancellationToken);
    }

    public async Task<Booking?> GetWithItemsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Event)
            .Include(b => b.Items)
                .ThenInclude(i => i.Seat)
            .Include(b => b.Items)
                .ThenInclude(i => i.Tier)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<int> CountActiveByUserAndEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.UserId == userId && b.EventId == eventId &&
                        (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
            .SelectMany(b => b.Items)
            .CountAsync(cancellationToken);
    }

    public async Task<Booking?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(b => b.Items)
                .ThenInclude(i => i.Seat)
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.Payments.Any(p => p.PaymentIntentId == paymentIntentId), cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetUserBookingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(b => b.UserId == userId)
            .Include(b => b.Event)
            .Include(b => b.Items)
                .ThenInclude(i => i.Tier)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
