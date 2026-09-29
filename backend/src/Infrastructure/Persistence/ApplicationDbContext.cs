namespace TicketWorld.Infrastructure.Persistence;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Common;
using TicketWorld.Domain.Entities;
using TicketWorld.Infrastructure.Persistence.Interceptors;

/// <summary>
/// PostgreSQL database context implementing Clean Architecture persistence contracts,
/// global soft-delete query filters, and audit interceptors.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly AuditableEntityInterceptor? _auditableEntityInterceptor;
    private readonly SoftDeleteInterceptor? _softDeleteInterceptor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AuditableEntityInterceptor? auditableEntityInterceptor = null,
        SoftDeleteInterceptor? softDeleteInterceptor = null)
        : base(options)
    {
        _auditableEntityInterceptor = auditableEntityInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<OrganizerProfile> OrganizerProfiles => Set<OrganizerProfile>();
    public DbSet<OrganizerStaff> OrganizerStaff => Set<OrganizerStaff>();
    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<ArtistFollower> ArtistFollowers => Set<ArtistFollower>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<SeatLayout> SeatLayouts => Set<SeatLayout>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketTier> TicketTiers => Set<TicketTier>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<PromoUsage> PromoUsages => Set<PromoUsage>();
    public DbSet<Payout> Payouts => Set<Payout>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<TicketTransfer> TicketTransfers => Set<TicketTransfer>();
    public DbSet<Waitlist> Waitlists => Set<Waitlist>();
    public DbSet<WaitingRoomToken> WaitingRoomTokens => Set<WaitingRoomToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemConfig> SystemConfigs => Set<SystemConfig>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (_auditableEntityInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_auditableEntityInterceptor);
        }

        if (_softDeleteInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_softDeleteInterceptor);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply all IEntityTypeConfiguration classes in Infrastructure
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filter for entities implementing ISoftDeletable
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "p");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.DeletedAt));
                var nullConstant = Expression.Constant(null, typeof(DateTime?));
                var comparison = Expression.Equal(property, nullConstant);
                var lambda = Expression.Lambda(comparison, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
