namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Cryptographic access token issued upon passing the virtual waiting room queue.
/// Mapped to the 'waiting_room_tokens' table in PostgreSQL.
/// </summary>
public class WaitingRoomToken : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public double PriorityScore { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTime? UsedAt { get; private set; }

    // Computed Properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired;

    // Navigation Properties
    public virtual Event Event { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    // Parameterless constructor for EF Core
    protected WaitingRoomToken() { }

    public static WaitingRoomToken Create(
        Guid eventId,
        Guid userId,
        string tokenHash,
        double priorityScore,
        TimeSpan validityDuration)
    {
        if (eventId == Guid.Empty)
            throw new DomainException("EventId cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        var now = DateTime.UtcNow;
        return new WaitingRoomToken
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            TokenHash = tokenHash.Trim(),
            PriorityScore = priorityScore,
            IssuedAt = now,
            ExpiresAt = now.Add(validityDuration),
            IsUsed = false
        };
    }

    public void Consume()
    {
        if (IsUsed)
            throw new DomainException("Waiting room token has already been consumed.");

        if (IsExpired)
            throw new DomainException("Waiting room token has expired.");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
