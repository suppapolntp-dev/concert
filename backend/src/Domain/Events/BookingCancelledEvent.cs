namespace TicketWorld.Domain.Events;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;

/// <summary>
/// Domain event published when an order is cancelled or times out.
/// Triggers seat release, automated refund (if paid), and waitlist notifications.
/// </summary>
public sealed record BookingCancelledEvent(
    Guid BookingId,
    Guid EventId,
    List<Guid> SeatIds,
    RefundReason Reason
) : DomainEventBase;
