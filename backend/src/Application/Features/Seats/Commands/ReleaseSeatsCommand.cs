namespace TicketWorld.Application.Features.Seats.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record ReleaseSeatsCommand(
    Guid EventId,
    List<Guid> SeatIds) : IRequest;

public class ReleaseSeatsCommandValidator : AbstractValidator<ReleaseSeatsCommand>
{
    public ReleaseSeatsCommandValidator()
    {
        RuleFor(v => v.EventId).NotEmpty();
        RuleFor(v => v.SeatIds).NotEmpty();
    }
}

public class ReleaseSeatsCommandHandler : IRequestHandler<ReleaseSeatsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISeatMapNotifier? _seatMapNotifier;

    public ReleaseSeatsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        ISeatMapNotifier? seatMapNotifier = null)
    {
        _context = context;
        _currentUserService = currentUserService;
        _seatMapNotifier = seatMapNotifier;
    }

    public async Task Handle(ReleaseSeatsCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var seats = await _context.Seats
            .Include(s => s.Tier)
            .Where(s => s.EventId == request.EventId && request.SeatIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var ev = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        var releasedSeats = new List<Guid>();
        foreach (var seat in seats)
        {
            if (seat.Status == SeatStatus.Held && (seat.HeldByUserId == userId || _currentUserService.Role == UserRole.Admin))
            {
                seat.Release();
                seat.Tier.ReleaseQuota(1);
                releasedSeats.Add(seat.Id);
            }
        }

        if (releasedSeats.Count > 0 && ev != null)
        {
            ev.ReleaseSeatHold(releasedSeats.Count);
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (_seatMapNotifier != null && releasedSeats.Count > 0)
        {
            await _seatMapNotifier.NotifySeatsStatusChangedAsync(request.EventId, releasedSeats, "Available", cancellationToken);
        }
    }
}
