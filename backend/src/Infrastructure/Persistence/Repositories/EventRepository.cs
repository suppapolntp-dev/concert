namespace TicketWorld.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Repositories;

public class EventRepository : Repository<Event>, IEventRepository
{
    public EventRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Event?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        return await DbSet
            .Include(e => e.Organizer)
            .Include(e => e.Venue)
            .Include(e => e.TicketTiers)
            .FirstOrDefaultAsync(e => e.Slug == normalizedSlug, cancellationToken);
    }

    public async Task<Event?> GetWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Organizer)
            .Include(e => e.Venue)
            .Include(e => e.TicketTiers)
            .Include(e => e.SeatLayout)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Event>> GetPublishedEventsAsync(int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Status == Domain.Enums.EventStatus.Published || e.Status == Domain.Enums.EventStatus.OnSale)
            .OrderBy(e => e.StartAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }
}
