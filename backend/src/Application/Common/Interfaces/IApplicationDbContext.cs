namespace TicketWorld.Application.Common.Interfaces;

using Microsoft.EntityFrameworkCore;
using TicketWorld.Domain.Entities;

/// <summary>
/// Abstraction for the persistence context used by application command and query handlers.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<OrganizerProfile> OrganizerProfiles { get; }
    DbSet<OrganizerStaff> OrganizerStaff { get; }
    DbSet<Artist> Artists { get; }
    DbSet<ArtistFollower> ArtistFollowers { get; }
    DbSet<Venue> Venues { get; }
    DbSet<SeatLayout> SeatLayouts { get; }
    DbSet<Event> Events { get; }
    DbSet<TicketTier> TicketTiers { get; }
    DbSet<Seat> Seats { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingItem> BookingItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PromoCode> PromoCodes { get; }
    DbSet<PromoUsage> PromoUsages { get; }
    DbSet<Payout> Payouts { get; }
    DbSet<Refund> Refunds { get; }
    DbSet<TicketTransfer> TicketTransfers { get; }
    DbSet<Waitlist> Waitlists { get; }
    DbSet<WaitingRoomToken> WaitingRoomTokens { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<SystemConfig> SystemConfigs { get; }
    DbSet<UserConsent> UserConsents { get; }
    DbSet<Review> Reviews { get; }
    DbSet<WebhookEvent> WebhookEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
