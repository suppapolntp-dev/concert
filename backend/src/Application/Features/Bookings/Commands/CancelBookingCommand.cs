namespace TicketWorld.Application.Features.Bookings.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record CancelBookingCommand(
    Guid BookingId,
    string Reason = "Customer cancellation") : IRequest;

public class CancelBookingCommandValidator : AbstractValidator<CancelBookingCommand>
{
    public CancelBookingCommandValidator()
    {
        RuleFor(v => v.BookingId).NotEmpty();
    }
}

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CancelBookingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var booking = await _context.Bookings
            .Include(b => b.Items)
                .ThenInclude(i => i.Seat)
                    .ThenInclude(s => s.Tier)
            .Include(b => b.Event)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
            throw new NotFoundException($"Booking with ID {request.BookingId} was not found.");

        if (booking.UserId != userId && _currentUserService.Role != UserRole.Admin)
            throw new ForbiddenException("You do not have permission to cancel this booking.");

        if (booking.Status != BookingStatus.Pending)
            throw new BusinessRuleException($"Cannot cancel booking with status '{booking.Status}'. Confirmed bookings require a refund request.");

        booking.Cancel(request.Reason);

        foreach (var item in booking.Items)
        {
            item.Seat.Release();
            item.Seat.Tier.ReleaseQuota(1);
        }

        booking.Event.ReleaseSeatHold(booking.Items.Count);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
