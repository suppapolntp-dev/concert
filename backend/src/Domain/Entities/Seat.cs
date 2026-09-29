namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// High-contention entity representing an individual physical or virtual concert seat.
/// Uses PostgreSQL xmin as an optimistic concurrency token.
/// </summary>
public class Seat : BaseEntity
{
    public Guid EventId { get; private set; }
    public Guid TierId { get; private set; }
    public string Section { get; private set; } = null!;
    public string Row { get; private set; } = null!;
    public string SeatNumber { get; private set; } = null!;
    public SeatStatus Status { get; private set; } = SeatStatus.Available;

    public Guid? HeldByUserId { get; private set; }
    public DateTime? HeldUntil { get; private set; }
    public string? PaymentIntentId { get; private set; }

    // Concurrency token for PostgreSQL xmin
    public uint Version { get; private set; }

    // Navigation Properties
    public virtual Event Event { get; private set; } = null!;
    public virtual TicketTier Tier { get; private set; } = null!;
    public virtual User? HeldByUser { get; private set; }

    // Parameterless constructor for EF Core
    protected Seat() { }

    public static Seat Create(
        Guid eventId,
        Guid tierId,
        string section,
        string row,
        string seatNumber)
    {
        if (eventId == Guid.Empty)
            throw new DomainException("EventId cannot be empty.");

        if (tierId == Guid.Empty)
            throw new DomainException("TierId cannot be empty.");

        if (string.IsNullOrWhiteSpace(section))
            throw new DomainException("Section is required.");

        if (string.IsNullOrWhiteSpace(row))
            throw new DomainException("Row is required.");

        if (string.IsNullOrWhiteSpace(seatNumber))
            throw new DomainException("SeatNumber is required.");

        return new Seat
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            TierId = tierId,
            Section = section.Trim(),
            Row = row.Trim(),
            SeatNumber = seatNumber.Trim(),
            Status = SeatStatus.Available
        };
    }

    public void Hold(Guid userId, TimeSpan duration, string? paymentIntentId = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        var isExpiredHold = Status == SeatStatus.Held && HeldUntil.HasValue && HeldUntil.Value < DateTime.UtcNow;
        if (Status != SeatStatus.Available && !isExpiredHold)
            throw new DomainException($"Seat {Section}-{Row}-{SeatNumber} is not available for holding.");

        Status = SeatStatus.Held;
        HeldByUserId = userId;
        HeldUntil = DateTime.UtcNow.Add(duration);
        PaymentIntentId = paymentIntentId;
    }

    public void AssignPaymentIntent(string paymentIntentId)
    {
        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new DomainException("Payment intent ID cannot be empty.");

        PaymentIntentId = paymentIntentId.Trim();
    }

    public void ConfirmBooking()
    {
        if (Status != SeatStatus.Held)
            throw new DomainException($"Cannot book seat {Section}-{Row}-{SeatNumber} because it is not held.");

        Status = SeatStatus.Booked;
        HeldUntil = null;
    }

    /// <summary>
    /// Reclaims and confirms a seat during payment webhook grace window reconciliation.
    /// Safely transitions directly from Available or expired Held to Booked without throwing.
    /// </summary>
    public void ReclaimAndConfirmFromGracePeriod(Guid userId, string paymentIntentId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        var isExpiredHold = Status == SeatStatus.Held && HeldUntil.HasValue && HeldUntil.Value < DateTime.UtcNow;
        var isAvailable = Status == SeatStatus.Available;

        if (!isAvailable && !isExpiredHold && HeldByUserId != userId)
        {
            throw new DomainException($"Seat {Section}-{Row}-{SeatNumber} was claimed by another customer.");
        }

        Status = SeatStatus.Booked;
        HeldByUserId = userId;
        HeldUntil = null;
        PaymentIntentId = paymentIntentId;
    }

    public void Release()
    {
        if (Status == SeatStatus.Booked)
            throw new DomainException("Cannot release an already booked seat.");

        Status = SeatStatus.Available;
        HeldByUserId = null;
        HeldUntil = null;
        PaymentIntentId = null;
    }

    public void Disable()
    {
        if (Status == SeatStatus.Booked)
            throw new DomainException("Cannot disable an already booked seat.");

        Status = SeatStatus.Disabled;
        HeldByUserId = null;
        HeldUntil = null;
        PaymentIntentId = null;
    }

    public void Enable()
    {
        if (Status == SeatStatus.Disabled)
        {
            Status = SeatStatus.Available;
        }
    }
}
