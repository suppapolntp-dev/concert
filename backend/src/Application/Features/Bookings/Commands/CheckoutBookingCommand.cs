namespace TicketWorld.Application.Features.Bookings.Commands;

using System.Security.Cryptography;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record CheckoutBookingResponse(
    Guid BookingId,
    string BookingReference,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal FinalAmount,
    string PaymentIntentId,
    DateTime ExpiresAt);

public sealed record CheckoutBookingCommand(
    Guid EventId,
    List<Guid> SeatIds,
    string? PromoCode = null) : IRequest<CheckoutBookingResponse>;

public class CheckoutBookingCommandValidator : AbstractValidator<CheckoutBookingCommand>
{
    public CheckoutBookingCommandValidator()
    {
        RuleFor(v => v.EventId).NotEmpty();
        RuleFor(v => v.SeatIds).NotEmpty();
    }
}

public class CheckoutBookingCommandHandler : IRequestHandler<CheckoutBookingCommand, CheckoutBookingResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CheckoutBookingCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<CheckoutBookingResponse> Handle(CheckoutBookingCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);
        if (ev == null)
            throw new NotFoundException($"Event with ID {request.EventId} was not found.");

        var seats = await _context.Seats
            .Include(s => s.Tier)
            .Where(s => s.EventId == request.EventId && request.SeatIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        if (seats.Count != request.SeatIds.Count)
            throw new NotFoundException("One or more selected seats were not found.");

        var now = DateTime.UtcNow;
        var invalidHold = seats.FirstOrDefault(s => s.Status != SeatStatus.Held || s.HeldByUserId != userId || s.HeldUntil <= now);
        if (invalidHold != null)
            throw new BusinessRuleException($"Seat {invalidHold.Section}-{invalidHold.Row}-{invalidHold.SeatNumber} is no longer held by you. Hold has expired or was released.");

        // Calculate hold expiry based on remaining hold window
        var holdExpiry = seats.Min(s => s.HeldUntil ?? now.AddMinutes(10));
        var remainingDuration = holdExpiry - now;
        if (remainingDuration <= TimeSpan.Zero)
            throw new BusinessRuleException("Your seat hold has expired.");

        var booking = Booking.Create(userId, request.EventId, remainingDuration);

        foreach (var seat in seats)
        {
            var qrPayload = $"TW-TICKET:{booking.BookingReference}:{seat.Id}:{Guid.NewGuid():N}";
            booking.AddItem(seat, seat.Tier, qrPayload);
        }

        // Apply promo code if present
        if (!string.IsNullOrWhiteSpace(request.PromoCode))
        {
            var code = request.PromoCode.Trim().ToUpperInvariant();
            var promo = await _context.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);

            if (promo == null || !promo.IsActive)
                throw new BusinessRuleException($"Promo code '{code}' is invalid or inactive.");

            if (promo.EventId.HasValue && promo.EventId.Value != request.EventId)
                throw new BusinessRuleException("This promo code cannot be applied to this event.");

            if (promo.OrganizerId.HasValue && promo.OrganizerId.Value != ev.OrganizerId)
                throw new BusinessRuleException("This promo code is not valid for this organizer's event.");

            var alreadyUsedByUser = await _context.PromoUsages
                .AnyAsync(u => u.PromoCodeId == promo.Id && u.UserId == userId, cancellationToken);
            if (alreadyUsedByUser)
                throw new BusinessRuleException("You have already used this promo code.");

            booking.ApplyPromoCode(promo);
            promo.RecordUsage();

            var usage = PromoUsage.Create(promo.Id, userId, booking.Id, booking.DiscountAmount);
            _context.PromoUsages.Add(usage);
        }

        // Generate Payment Intent & Record
        var paymentIntentId = $"pi_{Guid.NewGuid():N}";
        var idempotencyKey = $"idemp_{booking.Id:N}";

        var payment = Payment.Create(
            bookingId: booking.Id,
            paymentIntentId: paymentIntentId,
            gateway: PaymentGateway.Stripe,
            amount: booking.FinalAmount,
            idempotencyKey: idempotencyKey,
            currency: "THB");

        _context.Bookings.Add(booking);
        _context.Payments.Add(payment);

        foreach (var seat in seats)
        {
            seat.AssignPaymentIntent(paymentIntentId);
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("One or more seats or booking records were modified by another transaction. Please try again.");
        }

        return new CheckoutBookingResponse(
            booking.Id,
            booking.BookingReference,
            booking.TotalAmount,
            booking.DiscountAmount,
            booking.FinalAmount,
            paymentIntentId,
            booking.ExpiresAt);
    }
}
