namespace TicketWorld.Application.Features.Bookings.Commands;

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record ProcessStripeWebhookCommand(
    string EventId,
    string EventType,
    string RawPayload,
    string? PaymentIntentId,
    string? FailureReason = null) : IRequest;

public class ProcessStripeWebhookCommandHandler : IRequestHandler<ProcessStripeWebhookCommand>
{
    private readonly IApplicationDbContext _context;

    public ProcessStripeWebhookCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        // 1. Idempotency Check
        var existingWebhook = await _context.WebhookEvents
            .FirstOrDefaultAsync(w => w.Gateway == PaymentGateway.Stripe && w.EventId == request.EventId, cancellationToken);

        if (existingWebhook != null && existingWebhook.IsProcessed)
        {
            return; // Idempotently ignore already processed webhook
        }

        if (existingWebhook == null)
        {
            existingWebhook = WebhookEvent.Create(
                PaymentGateway.Stripe,
                request.EventId,
                request.EventType,
                request.RawPayload);
            _context.WebhookEvents.Add(existingWebhook);
        }

        try
        {
            if (string.IsNullOrWhiteSpace(request.PaymentIntentId))
            {
                existingWebhook.MarkProcessed();
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            var payment = await _context.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Items)
                        .ThenInclude(i => i.Seat)
                            .ThenInclude(s => s.Tier)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(p => p.PaymentIntentId == request.PaymentIntentId, cancellationToken);

            if (payment == null)
            {
                existingWebhook.RecordFailure($"Payment with Intent ID {request.PaymentIntentId} not found.");
                await _context.SaveChangesAsync(cancellationToken);
                return;
            }

            var booking = payment.Booking;

            switch (request.EventType)
            {
                case "payment_intent.succeeded":
                case "checkout.session.completed":
                    if (payment.Status != PaymentStatus.Succeeded)
                    {
                        payment.MarkSucceeded();

                        if (booking.Status == BookingStatus.Pending)
                        {
                            booking.Confirm();

                            foreach (var item in booking.Items)
                            {
                                // Grace period reconciliation: reclaim seat even if hold expired during checkout
                                item.Seat.ReclaimAndConfirmFromGracePeriod(booking.UserId, payment.PaymentIntentId);
                                item.Seat.Tier.ConfirmSoldQuota(1);
                            }

                            booking.Event.RecordBooking(booking.Items.Count);
                        }
                    }
                    break;

                case "payment_intent.payment_failed":
                    var reason = request.FailureReason ?? "Payment failed or was declined.";
                    payment.MarkFailed(reason);

                    if (booking.Status == BookingStatus.Pending)
                    {
                        booking.Cancel(reason);

                        foreach (var item in booking.Items)
                        {
                            if (item.Seat.Status == SeatStatus.Held)
                            {
                                item.Seat.Release();
                                item.Seat.Tier.ReleaseQuota(1);
                            }
                        }

                        booking.Event.ReleaseSeatHold(booking.Items.Count);
                    }
                    break;
            }

            existingWebhook.MarkProcessed();
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            existingWebhook.RecordFailure(ex.Message);
            await _context.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
