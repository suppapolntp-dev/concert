namespace TicketWorld.Infrastructure.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Repositories;

public class SeatRepository : Repository<Seat>, ISeatRepository
{
    public SeatRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Seat>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.Section)
            .ThenBy(s => s.Row)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Seat>> GetByIdsAsync(IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        var idList = seatIds.ToList();
        return await DbSet
            .Where(s => idList.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Seat>> GetAvailableByIdsForUpdateAsync(IEnumerable<Guid> seatIds, CancellationToken cancellationToken = default)
    {
        var idList = seatIds.ToList();
        return await DbSet
            .Where(s => idList.Contains(s.Id) && (s.Status == SeatStatus.Available || (s.Status == SeatStatus.Held && s.HeldUntil < DateTime.UtcNow)))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Seat>> GetHeldExpiredSeatsAsync(DateTime cutoffUtc, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.Status == SeatStatus.Held && s.HeldUntil.HasValue && s.HeldUntil.Value < cutoffUtc)
            .ToListAsync(cancellationToken);
    }
}
