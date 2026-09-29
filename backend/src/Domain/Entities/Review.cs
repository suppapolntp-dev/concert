namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Verified customer review and star rating for completed events.
/// Mapped to the 'reviews' table in PostgreSQL.
/// </summary>
public class Review : BaseEntity, IAuditableEntity
{
    public Guid EventId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? BookingId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsApproved { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Event Event { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual Booking? Booking { get; private set; }

    // Parameterless constructor for EF Core
    protected Review() { }

    public static Review Create(
        Guid eventId,
        Guid userId,
        int rating,
        string? comment = null,
        Guid? bookingId = null,
        bool isApproved = true)
    {
        if (eventId == Guid.Empty)
            throw new DomainException("EventId cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5 stars.");

        return new Review
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            BookingId = bookingId,
            Rating = rating,
            Comment = comment?.Trim(),
            IsApproved = isApproved,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateComment(int rating, string? comment)
    {
        if (rating < 1 || rating > 5)
            throw new DomainException("Rating must be between 1 and 5 stars.");

        Rating = rating;
        Comment = comment?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Moderate(bool isApproved)
    {
        IsApproved = isApproved;
        UpdatedAt = DateTime.UtcNow;
    }
}
