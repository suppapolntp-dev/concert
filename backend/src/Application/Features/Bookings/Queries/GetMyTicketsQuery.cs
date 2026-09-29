namespace TicketWorld.Application.Features.Bookings.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record UserTicketDto(
    Guid TicketId,
    Guid BookingId,
    string BookingReference,
    Guid EventId,
    string EventTitle,
    DateTime EventStartAt,
    string VenueName,
    string Section,
    string Row,
    string SeatNumber,
    string TierName,
    string QrCodePayload,
    bool IsCheckedIn,
    DateTime? CheckedInAt);

public sealed record GetMyTicketsQuery : IRequest<List<UserTicketDto>>;

public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, List<UserTicketDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyTicketsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<UserTicketDto>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var items = await _context.BookingItems
            .AsNoTracking()
            .Include(i => i.Booking)
                .ThenInclude(b => b.Event)
                    .ThenInclude(e => e.Venue)
            .Include(i => i.Seat)
            .Include(i => i.Tier)
            .Where(i => i.Booking.UserId == userId && i.Booking.Status == BookingStatus.Confirmed)
            .OrderBy(i => i.Booking.Event.StartAt)
            .ToListAsync(cancellationToken);

        return items.Select(i => new UserTicketDto(
            i.Id,
            i.BookingId,
            i.Booking.BookingReference,
            i.Booking.EventId,
            i.Booking.Event.Title,
            i.Booking.Event.StartAt,
            i.Booking.Event.Venue.Name,
            i.Seat.Section,
            i.Seat.Row,
            i.Seat.SeatNumber,
            i.Tier.Name,
            i.QrCodePayload,
            i.IsCheckedIn,
            i.CheckedInAt)).ToList();
    }
}
