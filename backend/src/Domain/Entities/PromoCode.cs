namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing a promotional discount code.
/// Mapped to the 'promo_codes' table in PostgreSQL.
/// </summary>
public class PromoCode : AggregateRoot, IAuditableEntity
{
    public Guid? OrganizerId { get; private set; }
    public Guid? EventId { get; private set; }
    public string Code { get; private set; } = null!;
    public DiscountType DiscountType { get; private set; } = DiscountType.Percentage;
    public decimal DiscountValue { get; private set; }
    public int MaxUses { get; private set; }
    public int UsedCount { get; private set; }
    public decimal? MaxDiscountAmount { get; private set; }
    public decimal MinOrderAmount { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime ValidUntil { get; private set; }
    public bool IsActive { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual OrganizerProfile? Organizer { get; private set; }
    public virtual Event? Event { get; private set; }
    public virtual ICollection<PromoUsage> Usages { get; private set; } = new List<PromoUsage>();

    // Parameterless constructor for EF Core
    protected PromoCode() { }

    public static PromoCode Create(
        string code,
        DiscountType discountType,
        decimal discountValue,
        int maxUses,
        DateTime validFrom,
        DateTime validUntil,
        decimal minOrderAmount = 0m,
        decimal? maxDiscountAmount = null,
        Guid? organizerId = null,
        Guid? eventId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Promo code cannot be empty.");

        if (discountValue <= 0)
            throw new DomainException("Discount value must be greater than zero.");

        if (discountType == DiscountType.Percentage && discountValue > 100)
            throw new DomainException("Percentage discount cannot exceed 100%.");

        if (maxUses <= 0)
            throw new DomainException("Max uses must be greater than zero.");

        if (validFrom >= validUntil)
            throw new DomainException("ValidFrom must precede ValidUntil.");

        return new PromoCode
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            DiscountType = discountType,
            DiscountValue = discountValue,
            MaxUses = maxUses,
            UsedCount = 0,
            MinOrderAmount = minOrderAmount,
            MaxDiscountAmount = maxDiscountAmount,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            IsActive = true,
            OrganizerId = organizerId,
            EventId = eventId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public decimal CalculateDiscount(decimal orderTotal)
    {
        if (!IsActive)
            throw new DomainException("Promo code is inactive.");

        var now = DateTime.UtcNow;
        if (now < ValidFrom || now > ValidUntil)
            throw new DomainException("Promo code has expired or is not yet active.");

        if (UsedCount >= MaxUses)
            throw new DomainException("Promo code usage limit has been reached.");

        if (orderTotal < MinOrderAmount)
            throw new DomainException($"Order total must be at least {MinOrderAmount:N2} to use this promo code.");

        decimal discount;
        if (DiscountType == DiscountType.Percentage)
        {
            discount = orderTotal * (DiscountValue / 100m);
            if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
            {
                discount = MaxDiscountAmount.Value;
            }
        }
        else
        {
            discount = Math.Min(orderTotal, DiscountValue);
        }

        return Math.Round(discount, 2);
    }

    public void RecordUsage()
    {
        if (UsedCount >= MaxUses)
            throw new DomainException("Promo code usage limit reached.");

        UsedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
