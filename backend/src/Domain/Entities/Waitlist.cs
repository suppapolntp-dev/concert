namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a customer position in a queue for sold-out event inventory.
/// Mapped to the 'waitlists' table in PostgreSQL.
/// </summary>
public class Waitlist : BaseEntity, IAuditableEntity
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? TierId { get; private set; }
    public int QueueNumber { get; private set; }
    public int RequestedQuantity { get; private set; } = 1;
    public WaitlistStatus Status { get; private set; } = WaitlistStatus.Queued;
    public DateTime? NotifiedAt { get; private set; }
    public DateTime? ClaimExpiresAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Event Event { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual TicketTier? Tier { get; private set; }

    // Parameterless constructor for EF Core
    protected Waitlist() { }

    public static Waitlist Create(
        Guid eventId,
        Guid userId,
        int queueNumber,
        int requestedQuantity = 1,
        Guid? tierId = null)
    {
        if (eventId == Guid.Empty)
            throw new DomainException("EventId cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (queueNumber <= 0)
            throw new DomainException("Queue number must be greater than zero.");

        if (requestedQuantity <= 0)
            throw new DomainException("Requested quantity must be greater than zero.");

        return new Waitlist
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            TierId = tierId,
            QueueNumber = queueNumber,
            RequestedQuantity = requestedQuantity,
            Status = WaitlistStatus.Queued,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Notify(TimeSpan claimDuration)
    {
        if (Status != WaitlistStatus.Queued)
            throw new DomainException($"Cannot notify waitlist entry in '{Status}' status.");

        Status = WaitlistStatus.Notified;
        NotifiedAt = DateTime.UtcNow;
        ClaimExpiresAt = DateTime.UtcNow.Add(claimDuration);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Convert()
    {
        if (Status != WaitlistStatus.Notified)
            throw new DomainException("Only notified waitlist entries can be converted.");

        if (ClaimExpiresAt.HasValue && DateTime.UtcNow > ClaimExpiresAt.Value)
        {
            Status = WaitlistStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
            throw new DomainException("Waitlist claim period has expired.");
        }

        Status = WaitlistStatus.Converted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        Status = WaitlistStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }
}
