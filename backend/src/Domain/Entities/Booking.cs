namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing an order and ticket reservation.
/// Mapped to the 'bookings' table in PostgreSQL.
/// </summary>
public class Booking : AggregateRoot, IAuditableEntity
{
    public string BookingReference { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public Guid EventId { get; private set; }
    public Guid? PromoCodeId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal FinalAmount { get; private set; }
    public BookingStatus Status { get; private set; } = BookingStatus.Pending;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; private set; } = null!;
    public virtual Event Event { get; private set; } = null!;
    public virtual PromoCode? PromoCode { get; private set; }
    public virtual ICollection<BookingItem> Items { get; private set; } = new List<BookingItem>();
    public virtual ICollection<Payment> Payments { get; private set; } = new List<Payment>();
    public virtual ICollection<Refund> Refunds { get; private set; } = new List<Refund>();

    // Parameterless constructor for EF Core
    protected Booking() { }

    public static Booking Create(
        Guid userId,
        Guid eventId,
        TimeSpan holdDuration,
        string? bookingReference = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (eventId == Guid.Empty)
            throw new DomainException("EventId cannot be empty.");

        var reference = string.IsNullOrWhiteSpace(bookingReference)
            ? $"BK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}"
            : bookingReference.Trim().ToUpperInvariant();

        return new Booking
        {
            Id = Guid.NewGuid(),
            BookingReference = reference,
            UserId = userId,
            EventId = eventId,
            TotalAmount = 0m,
            DiscountAmount = 0m,
            FinalAmount = 0m,
            Status = BookingStatus.Pending,
            ExpiresAt = DateTime.UtcNow.Add(holdDuration),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void AddItem(Seat seat, TicketTier tier, string qrCodePayload)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Cannot add items to a non-pending booking.");

        var item = BookingItem.Create(Id, seat.Id, tier.Id, tier.Price, qrCodePayload);
        Items.Add(item);

        TotalAmount += tier.Price;
        RecalculateFinalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyPromoCode(PromoCode promoCode)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Cannot apply promo code to a non-pending booking.");

        PromoCodeId = promoCode.Id;
        DiscountAmount = promoCode.CalculateDiscount(TotalAmount);
        RecalculateFinalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePromoCode()
    {
        PromoCodeId = null;
        DiscountAmount = 0m;
        RecalculateFinalAmount();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException($"Cannot confirm booking in '{Status}' status.");

        Status = BookingStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == BookingStatus.Cancelled || Status == BookingStatus.Refunded)
            return;

        Status = BookingStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefunded()
    {
        Status = BookingStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkTransferred()
    {
        Status = BookingStatus.Transferred;
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateFinalAmount()
    {
        FinalAmount = Math.Max(0m, TotalAmount - DiscountAmount);
    }
}
