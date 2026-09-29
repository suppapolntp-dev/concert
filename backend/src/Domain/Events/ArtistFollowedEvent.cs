namespace TicketWorld.Domain.Events;

using TicketWorld.Domain.Common;

/// <summary>
/// Domain event published when a user follows an artist.
/// Triggers atomic increment of the artist follower count.
/// </summary>
public sealed record ArtistFollowedEvent(
    Guid UserId,
    Guid ArtistId
) : DomainEventBase;
