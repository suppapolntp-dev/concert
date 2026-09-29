namespace TicketWorld.Domain.Entities;

using System.Text.RegularExpressions;
using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing a concert or ticketed event lifecycle.
/// Mapped to the 'events' table in PostgreSQL.
/// </summary>
public class Event : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public Guid OrganizerId { get; private set; }
    public Guid VenueId { get; private set; }
    public Guid? SeatLayoutId { get; private set; }
    public string Title { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public EventCategory Category { get; private set; } = EventCategory.Concert;
    public string? PosterImageUrl { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public EventStatus Status { get; private set; } = EventStatus.Draft;
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public DateTime? DoorsOpenAt { get; private set; }
    public DateTime SalesStartAt { get; private set; }
    public DateTime SalesEndAt { get; private set; }
    public int MaxTicketsPerUser { get; private set; } = 4;
    public int TotalSeats { get; private set; }
    public int SoldSeats { get; private set; }
    public int HeldSeats { get; private set; }
    public bool HasWaitingRoom { get; private set; }
    public string? RejectionReason { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ISoftDeletable
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual OrganizerProfile Organizer { get; private set; } = null!;
    public virtual Venue Venue { get; private set; } = null!;
    public virtual SeatLayout? SeatLayout { get; private set; }
    public virtual ICollection<TicketTier> TicketTiers { get; private set; } = new List<TicketTier>();
    public virtual ICollection<Seat> Seats { get; private set; } = new List<Seat>();
    public virtual ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public virtual ICollection<Waitlist> Waitlists { get; private set; } = new List<Waitlist>();

    // Parameterless constructor for EF Core
    protected Event() { }

    public static Event Create(
        Guid organizerId,
        Guid venueId,
        string title,
        DateTime startAt,
        DateTime endAt,
        DateTime salesStartAt,
        DateTime salesEndAt,
        EventCategory category = EventCategory.Concert,
        Guid? seatLayoutId = null,
        string? description = null,
        string? slug = null,
        string? posterImageUrl = null,
        string? coverImageUrl = null,
        DateTime? doorsOpenAt = null,
        int maxTicketsPerUser = 4,
        bool hasWaitingRoom = false)
    {
        if (organizerId == Guid.Empty)
            throw new DomainException("OrganizerId cannot be empty.");

        if (venueId == Guid.Empty)
            throw new DomainException("VenueId cannot be empty.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Event title is required.");

        if (startAt >= endAt)
            throw new DomainException("Event start time must precede end time.");

        if (salesStartAt >= salesEndAt)
            throw new DomainException("Sales start time must precede sales end time.");

        if (maxTicketsPerUser <= 0)
            throw new DomainException("Max tickets per user must be greater than zero.");

        var generatedSlug = string.IsNullOrWhiteSpace(slug)
            ? GenerateSlug(title)
            : slug.Trim().ToLowerInvariant();

        return new Event
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            VenueId = venueId,
            SeatLayoutId = seatLayoutId,
            Title = title.Trim(),
            Slug = generatedSlug,
            Description = description?.Trim(),
            Category = category,
            PosterImageUrl = posterImageUrl?.Trim(),
            CoverImageUrl = coverImageUrl?.Trim(),
            Status = EventStatus.Draft,
            StartAt = startAt,
            EndAt = endAt,
            DoorsOpenAt = doorsOpenAt,
            SalesStartAt = salesStartAt,
            SalesEndAt = salesEndAt,
            MaxTicketsPerUser = maxTicketsPerUser,
            HasWaitingRoom = hasWaitingRoom,
            TotalSeats = 0,
            SoldSeats = 0,
            HeldSeats = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string title,
        string? description,
        EventCategory category,
        DateTime startAt,
        DateTime endAt,
        DateTime salesStartAt,
        DateTime salesEndAt,
        int maxTicketsPerUser,
        bool hasWaitingRoom,
        string? posterImageUrl = null,
        string? coverImageUrl = null,
        DateTime? doorsOpenAt = null)
    {
        if (Status != EventStatus.Draft && Status != EventStatus.PendingApproval)
            throw new DomainException("Cannot edit core details of an active or published event.");

        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Event title cannot be empty.");

        if (startAt >= endAt)
            throw new DomainException("Event start time must precede end time.");

        if (salesStartAt >= salesEndAt)
            throw new DomainException("Sales start time must precede sales end time.");

        Title = title.Trim();
        Description = description?.Trim();
        Category = category;
        StartAt = startAt;
        EndAt = endAt;
        SalesStartAt = salesStartAt;
        SalesEndAt = salesEndAt;
        MaxTicketsPerUser = maxTicketsPerUser;
        HasWaitingRoom = hasWaitingRoom;
        PosterImageUrl = posterImageUrl?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        DoorsOpenAt = doorsOpenAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSeatCapacity(int totalSeats)
    {
        if (totalSeats < 0)
            throw new DomainException("Total seats cannot be negative.");

        TotalSeats = totalSeats;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SubmitForApproval()
    {
        if (Status != EventStatus.Draft)
            throw new DomainException("Only draft events can be submitted for approval.");

        Status = EventStatus.PendingApproval;
        RejectionReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != EventStatus.PendingApproval)
            throw new DomainException("Only pending events can be approved.");

        Status = EventStatus.Published;
        RejectionReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != EventStatus.PendingApproval)
            throw new DomainException("Only pending events can be rejected.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Rejection reason must be specified.");

        Status = EventStatus.Draft;
        RejectionReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status != EventStatus.Draft && Status != EventStatus.PendingApproval)
            throw new DomainException("Cannot publish event from current state.");

        Status = EventStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartSales()
    {
        if (Status != EventStatus.Published && Status != EventStatus.SoldOut)
            throw new DomainException("Event cannot be put on sale from current state.");

        Status = EventStatus.OnSale;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSoldOut()
    {
        if (Status != EventStatus.OnSale)
            throw new DomainException("Only on-sale events can be marked as sold out.");

        Status = EventStatus.SoldOut;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == EventStatus.Completed)
            throw new DomainException("Cannot cancel an already completed event.");

        Status = EventStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status == EventStatus.Cancelled)
            throw new DomainException("Cannot complete a cancelled event.");

        Status = EventStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordSeatHold(int count = 1)
    {
        HeldSeats += count;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseSeatHold(int count = 1)
    {
        HeldSeats = Math.Max(0, HeldSeats - count);
        if (Status == EventStatus.SoldOut && (SoldSeats + HeldSeats) < TotalSeats)
        {
            Status = EventStatus.OnSale;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordBooking(int count = 1)
    {
        HeldSeats = Math.Max(0, HeldSeats - count);
        SoldSeats += count;
        if (TotalSeats > 0 && SoldSeats >= TotalSeats)
        {
            Status = EventStatus.SoldOut;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordRefund(int count = 1)
    {
        SoldSeats = Math.Max(0, SoldSeats - count);
        if (Status == EventStatus.SoldOut)
        {
            Status = EventStatus.OnSale;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateSlug(string text)
    {
        var str = text.Trim().ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(str) ? Guid.NewGuid().ToString("n") : str;
    }
}
