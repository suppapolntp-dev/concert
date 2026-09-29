namespace TicketWorld.Domain.Events;

using TicketWorld.Domain.Common;

/// <summary>
/// Domain event published when payment is verified and booking is finalized.
/// Triggers QR ticket issuance, email confirmation, and sold counter updates.
/// </summary>
public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid UserId,
    Guid EventId,
    List<Guid> SeatIds,
    decimal TotalAmount,
    string Currency
) : DomainEventBase;
