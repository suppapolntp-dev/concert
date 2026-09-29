namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Exceptions;

/// <summary>
/// Junction entity representing a user following an artist.
/// Mapped to the 'artist_followers' table in PostgreSQL with a composite primary key (UserId, ArtistId).
/// </summary>
public class ArtistFollower
{
    public Guid UserId { get; private set; }
    public Guid ArtistId { get; private set; }
    public DateTime FollowedAt { get; private set; }

    // Navigation Properties
    public virtual User User { get; private set; } = null!;
    public virtual Artist Artist { get; private set; } = null!;

    // Parameterless constructor for EF Core
    protected ArtistFollower() { }

    public ArtistFollower(Guid userId, Guid artistId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (artistId == Guid.Empty)
            throw new DomainException("ArtistId cannot be empty.");

        UserId = userId;
        ArtistId = artistId;
        FollowedAt = DateTime.UtcNow;
    }
}
