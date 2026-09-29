namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a pricing tier and quota allotment within an event.
/// Mapped to the 'ticket_tiers' table in PostgreSQL.
/// </summary>
public class TicketTier : BaseEntity, IAuditableEntity
{
    public Guid EventId { get; private set; }
    public string Name { get; private set; } = null!;
    public TicketTierType TierType { get; private set; } = TicketTierType.Standard;
    public decimal Price { get; private set; }
    public string ColorHex { get; private set; } = "#3B82F6";
    public string? Description { get; private set; }
    public int TotalQuota { get; private set; }
    public int AvailableQuota { get; private set; }
    public int HeldQuota { get; private set; }
    public int SoldQuota { get; private set; }
    public string? EntryGate { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Event Event { get; private set; } = null!;
    public virtual ICollection<Seat> Seats { get; private set; } = new List<Seat>();
    public virtual ICollection<BookingItem> BookingItems { get; private set; } = new List<BookingItem>();

    // Parameterless constructor for EF Core
    protected TicketTier() { }

    public static TicketTier Create(
        Guid eventId,
        string name,
        decimal price,
        int totalQuota,
        TicketTierType tierType = TicketTierType.Standard,
        string colorHex = "#3B82F6",
        string? description = null,
        string? entryGate = null)
    {
        if (eventId == Guid.Empty)
            throw new DomainException("EventId cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tier name is required.");

        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        if (totalQuota <= 0)
            throw new DomainException("Total quota must be greater than zero.");

        return new TicketTier
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Name = name.Trim(),
            Price = price,
            TierType = tierType,
            ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#3B82F6" : colorHex.Trim(),
            Description = description?.Trim(),
            EntryGate = entryGate?.Trim(),
            TotalQuota = totalQuota,
            AvailableQuota = totalQuota,
            HeldQuota = 0,
            SoldQuota = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(string name, decimal price, string colorHex, string? description, string? entryGate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tier name cannot be empty.");

        if (price < 0)
            throw new DomainException("Price cannot be negative.");

        Name = name.Trim();
        Price = price;
        ColorHex = string.IsNullOrWhiteSpace(colorHex) ? "#3B82F6" : colorHex.Trim();
        Description = description?.Trim();
        EntryGate = entryGate?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void HoldQuota(int quantity = 1)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (AvailableQuota < quantity)
            throw new DomainException($"Insufficient available quota for tier '{Name}'.");

        AvailableQuota -= quantity;
        HeldQuota += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseQuota(int quantity = 1)
    {
        if (quantity <= 0)
            return;

        var actualRelease = Math.Min(HeldQuota, quantity);
        HeldQuota -= actualRelease;
        AvailableQuota += actualRelease;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConfirmSoldQuota(int quantity = 1)
    {
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        var fromHeld = Math.Min(HeldQuota, quantity);
        HeldQuota -= fromHeld;

        var remaining = quantity - fromHeld;
        if (remaining > 0)
        {
            if (AvailableQuota < remaining)
                throw new DomainException("Insufficient quota to confirm sold tickets.");
            AvailableQuota -= remaining;
        }

        SoldQuota += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RefundQuota(int quantity = 1)
    {
        if (quantity <= 0)
            return;

        var actualRefund = Math.Min(SoldQuota, quantity);
        SoldQuota -= actualRefund;
        AvailableQuota += actualRefund;
        UpdatedAt = DateTime.UtcNow;
    }
}
