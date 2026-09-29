namespace TicketWorld.Application.Features.Bookings.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record BookingTicketDto(
    Guid ItemId,
    Guid SeatId,
    string Section,
    string Row,
    string SeatNumber,
    string TierName,
    decimal UnitPrice,
    string QrCodePayload,
    bool IsCheckedIn,
    DateTime? CheckedInAt);

public sealed record BookingDetailDto(
    Guid Id,
    string BookingReference,
    Guid EventId,
    string EventTitle,
    DateTime EventStartAt,
    string VenueName,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    BookingStatus Status,
    DateTime ExpiresAt,
    DateTime? ConfirmedAt,
    List<BookingTicketDto> Tickets);

public sealed record GetBookingByIdQuery(Guid BookingId) : IRequest<BookingDetailDto>;

public class GetBookingByIdQueryHandler : IRequestHandler<GetBookingByIdQuery, BookingDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetBookingByIdQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<BookingDetailDto> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
            .Include(b => b.Items)
                .ThenInclude(i => i.Seat)
            .Include(b => b.Items)
                .ThenInclude(i => i.Tier)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
            throw new NotFoundException($"Booking with ID {request.BookingId} was not found.");

        if (booking.UserId != userId && _currentUserService.Role != UserRole.Admin)
            throw new ForbiddenException("You do not have permission to view this booking.");

        var tickets = booking.Items.Select(i => new BookingTicketDto(
            i.Id,
            i.SeatId,
            i.Seat.Section,
            i.Seat.Row,
            i.Seat.SeatNumber,
            i.Tier.Name,
            i.UnitPrice,
            i.QrCodePayload,
            i.IsCheckedIn,
            i.CheckedInAt)).ToList();

        return new BookingDetailDto(
            booking.Id,
            booking.BookingReference,
            booking.EventId,
            booking.Event.Title,
            booking.Event.StartAt,
            booking.Event.Venue.Name,
            booking.TotalAmount,
            booking.DiscountAmount,
            booking.FinalAmount,
            booking.Status,
            booking.ExpiresAt,
            booking.ConfirmedAt,
            tickets);
    }
}
