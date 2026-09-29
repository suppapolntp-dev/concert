namespace TicketWorld.Domain.Events;

using TicketWorld.Domain.Common;

/// <summary>
/// Domain event published when a customer successfully acquires a temporary hold on a seat.
/// Triggers real-time SignalR broadcast and schedules background hold expiry job.
/// </summary>
public sealed record SeatHeldEvent(
    Guid SeatId,
    Guid EventId,
    Guid UserId,
    DateTime HeldUntil
) : DomainEventBase;
