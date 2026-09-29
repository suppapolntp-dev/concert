namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Audit entity tracking redemptions of promotional codes.
/// Mapped to the 'promo_usages' table in PostgreSQL.
/// </summary>
public class PromoUsage : BaseEntity
{
    public Guid PromoCodeId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid BookingId { get; private set; }
    public decimal DiscountApplied { get; private set; }
    public DateTime UsedAt { get; private set; }

    // Navigation Properties
    public virtual PromoCode PromoCode { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual Booking Booking { get; private set; } = null!;

    // Parameterless constructor for EF Core
    protected PromoUsage() { }

    public static PromoUsage Create(
        Guid promoCodeId,
        Guid userId,
        Guid bookingId,
        decimal discountApplied)
    {
        if (promoCodeId == Guid.Empty)
            throw new DomainException("PromoCodeId cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId cannot be empty.");

        if (discountApplied < 0)
            throw new DomainException("Discount applied cannot be negative.");

        return new PromoUsage
        {
            Id = Guid.NewGuid(),
            PromoCodeId = promoCodeId,
            UserId = userId,
            BookingId = bookingId,
            DiscountApplied = discountApplied,
            UsedAt = DateTime.UtcNow
        };
    }
}
