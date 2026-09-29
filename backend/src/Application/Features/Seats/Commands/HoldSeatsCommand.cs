namespace TicketWorld.Application.Features.Seats.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record HoldSeatsResponse(
    Guid EventId,
    List<Guid> SeatIds,
    DateTime HeldUntil);

public sealed record HoldSeatsCommand(
    Guid EventId,
    List<Guid> SeatIds) : IRequest<HoldSeatsResponse>;

public class HoldSeatsCommandValidator : AbstractValidator<HoldSeatsCommand>
{
    public HoldSeatsCommandValidator()
    {
        RuleFor(v => v.EventId).NotEmpty();
        RuleFor(v => v.SeatIds)
            .NotEmpty().WithMessage("At least one seat must be selected.")
            .Must(s => s.Count <= 6).WithMessage("Cannot hold more than 6 seats in a single request.");
    }
}

public class HoldSeatsCommandHandler : IRequestHandler<HoldSeatsCommand, HoldSeatsResponse>
{
    private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10);
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDistributedLockService _lockService;
    private readonly ISeatMapNotifier? _seatMapNotifier;

    public HoldSeatsCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService,
        IDistributedLockService lockService,
        ISeatMapNotifier? seatMapNotifier = null)
    {
        _context = context;
        _currentUserService = currentUserService;
        _lockService = lockService;
        _seatMapNotifier = seatMapNotifier;
    }

    public async Task<HoldSeatsResponse> Handle(HoldSeatsCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        // Acquire RedLock distributed lock per event to prevent concurrent double-holding
        var lockKey = $"lock:event:{request.EventId}:seats";
        using var redLock = await _lockService.AcquireLockAsync(
            lockKey,
            expiryTime: TimeSpan.FromSeconds(10),
            waitTime: TimeSpan.FromSeconds(5),
            retryTime: TimeSpan.FromMilliseconds(200),
            cancellationToken);

        if (redLock == null)
            throw new ConflictException("Seat reservation system is currently busy. Please try again.");

        var ev = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        if (ev == null)
            throw new NotFoundException($"Event with ID {request.EventId} was not found.");

        if (ev.Status != EventStatus.OnSale)
            throw new BusinessRuleException($"Event is not currently on sale (Status: {ev.Status}).");

        var now = DateTime.UtcNow;

        // Enforce maximum tickets per user: count active holds + confirmed bookings
        var userActiveHolds = await _context.Seats
            .CountAsync(s => s.EventId == request.EventId && s.HeldByUserId == userId && s.Status == SeatStatus.Held && s.HeldUntil > now, cancellationToken);

        var userConfirmedTickets = await _context.Bookings
            .Where(b => b.UserId == userId && b.EventId == request.EventId && b.Status == BookingStatus.Confirmed)
            .SelectMany(b => b.Items)
            .CountAsync(cancellationToken);

        var totalRequested = userActiveHolds + userConfirmedTickets + request.SeatIds.Count;
        if (totalRequested > ev.MaxTicketsPerUser)
        {
            var remaining = Math.Max(0, ev.MaxTicketsPerUser - (userActiveHolds + userConfirmedTickets));
            throw new BusinessRuleException(
                $"Holding {request.SeatIds.Count} seat(s) exceeds maximum limit of {ev.MaxTicketsPerUser} tickets per user. " +
                $"You currently have {userActiveHolds} held and {userConfirmedTickets} confirmed ticket(s). Remaining allowed: {remaining}.");
        }

        var seats = await _context.Seats
            .Include(s => s.Tier)
            .Where(s => s.EventId == request.EventId && request.SeatIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (seats.Count != request.SeatIds.Count)
            throw new NotFoundException("One or more selected seats do not exist.");

        var unavailableSeats = seats
            .Where(s => s.Status != SeatStatus.Available && !(s.Status == SeatStatus.Held && s.HeldUntil.HasValue && s.HeldUntil.Value < now))
            .ToList();

        if (unavailableSeats.Count != 0)
        {
            var seatNames = string.Join(", ", unavailableSeats.Select(s => $"{s.Section}-{s.Row}-{s.SeatNumber}"));
            throw new ConflictException($"The following seat(s) are already held or booked: {seatNames}");
        }

        // Apply hold to seats
        foreach (var seat in seats)
        {
            seat.Hold(userId, HoldDuration);
            seat.Tier.HoldQuota(1);
        }

        ev.RecordSeatHold(seats.Count);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("One or more selected seats were reserved by another customer. Please choose different seats.");
        }

        if (_seatMapNotifier != null)
        {
            await _seatMapNotifier.NotifySeatsStatusChangedAsync(request.EventId, request.SeatIds, "Held", cancellationToken);
        }

        return new HoldSeatsResponse(request.EventId, request.SeatIds, now.Add(HoldDuration));
    }
}
