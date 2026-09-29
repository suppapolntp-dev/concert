namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents an individual ticket line-item bound to a seat within a booking.
/// Mapped to the 'booking_items' table in PostgreSQL.
/// </summary>
public class BookingItem : BaseEntity
{
    public Guid BookingId { get; private set; }
    public Guid SeatId { get; private set; }
    public Guid TierId { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string QrCodePayload { get; private set; } = null!;
    public bool IsCheckedIn { get; private set; }
    public DateTime? CheckedInAt { get; private set; }
    public Guid? CheckedInBy { get; private set; }

    // Navigation Properties
    public virtual Booking Booking { get; private set; } = null!;
    public virtual Seat Seat { get; private set; } = null!;
    public virtual TicketTier Tier { get; private set; } = null!;
    public virtual User? CheckedInByUser { get; private set; }
    public virtual ICollection<TicketTransfer> Transfers { get; private set; } = new List<TicketTransfer>();

    // Parameterless constructor for EF Core
    protected BookingItem() { }

    public static BookingItem Create(
        Guid bookingId,
        Guid seatId,
        Guid tierId,
        decimal unitPrice,
        string qrCodePayload)
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId cannot be empty.");

        if (seatId == Guid.Empty)
            throw new DomainException("SeatId cannot be empty.");

        if (tierId == Guid.Empty)
            throw new DomainException("TierId cannot be empty.");

        if (unitPrice < 0)
            throw new DomainException("Unit price cannot be negative.");

        if (string.IsNullOrWhiteSpace(qrCodePayload))
            throw new DomainException("QR code payload is required.");

        return new BookingItem
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            SeatId = seatId,
            TierId = tierId,
            UnitPrice = unitPrice,
            QrCodePayload = qrCodePayload.Trim(),
            IsCheckedIn = false
        };
    }

    public void UpdateQrCodePayload(string newPayload)
    {
        if (string.IsNullOrWhiteSpace(newPayload))
            throw new DomainException("New QR code payload cannot be empty.");

        QrCodePayload = newPayload.Trim();
    }

    public void CheckIn(Guid staffUserId)
    {
        if (staffUserId == Guid.Empty)
            throw new DomainException("Staff user ID is required for check-in.");

        if (IsCheckedIn)
            throw new DomainException($"Ticket has already been checked in at {CheckedInAt}.");

        IsCheckedIn = true;
        CheckedInAt = DateTime.UtcNow;
        CheckedInBy = staffUserId;
    }
}
