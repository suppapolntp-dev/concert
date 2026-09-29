namespace TicketWorld.Domain.Events;

using TicketWorld.Domain.Common;

/// <summary>
/// Domain event published when an event status transitions to Published.
/// Triggers search indexing and notification of artist followers.
/// </summary>
public sealed record EventPublishedEvent(
    Guid EventId,
    string Title,
    List<Guid> ArtistIds
) : DomainEventBase;
