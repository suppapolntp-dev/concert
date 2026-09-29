namespace TicketWorld.Application.Features.Seats.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;

public sealed record SeatStatusDto(
    Guid Id,
    Guid TierId,
    string Section,
    string Row,
    string SeatNumber,
    SeatStatus Status,
    decimal Price,
    string TierName,
    string ColorHex);

public sealed record GetEventSeatsQuery(Guid EventId) : IRequest<List<SeatStatusDto>>;

public class GetEventSeatsQueryHandler : IRequestHandler<GetEventSeatsQuery, List<SeatStatusDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventSeatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SeatStatusDto>> Handle(GetEventSeatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var seats = await _context.Seats
            .AsNoTracking()
            .Include(s => s.Tier)
            .Where(s => s.EventId == request.EventId)
            .OrderBy(s => s.Section)
            .ThenBy(s => s.Row)
            .ThenBy(s => s.SeatNumber)
            .ToListAsync(cancellationToken);

        return seats.Select(s =>
        {
            var effectiveStatus = s.Status;
            // Treat expired holds as Available in real-time view
            if (s.Status == SeatStatus.Held && s.HeldUntil.HasValue && s.HeldUntil.Value < now)
            {
                effectiveStatus = SeatStatus.Available;
            }

            return new SeatStatusDto(
                s.Id,
                s.TierId,
                s.Section,
                s.Row,
                s.SeatNumber,
                effectiveStatus,
                s.Tier.Price,
                s.Tier.Name,
                s.Tier.ColorHex);
        }).ToList();
    }
}
