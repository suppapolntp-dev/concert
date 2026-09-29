namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a financial transaction processed through a third-party gateway (Stripe, Omise, etc.).
/// Mapped to the 'payments' table in PostgreSQL.
/// </summary>
public class Payment : BaseEntity, IAuditableEntity
{
    public Guid BookingId { get; private set; }
    public string PaymentIntentId { get; private set; } = null!;
    public PaymentGateway Gateway { get; private set; } = PaymentGateway.Stripe;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "THB";
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public string IdempotencyKey { get; private set; } = null!;
    public string? PaymentMethodType { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime? PaidAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Booking Booking { get; private set; } = null!;
    public virtual ICollection<Refund> Refunds { get; private set; } = new List<Refund>();

    // Parameterless constructor for EF Core
    protected Payment() { }

    public static Payment Create(
        Guid bookingId,
        string paymentIntentId,
        PaymentGateway gateway,
        decimal amount,
        string idempotencyKey,
        string currency = "THB",
        string? paymentMethodType = null)
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId cannot be empty.");

        if (string.IsNullOrWhiteSpace(paymentIntentId))
            throw new DomainException("PaymentIntentId is required.");

        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new DomainException("IdempotencyKey is required.");

        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");

        return new Payment
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PaymentIntentId = paymentIntentId.Trim(),
            Gateway = gateway,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(currency) ? "THB" : currency.Trim().ToUpperInvariant(),
            IdempotencyKey = idempotencyKey.Trim(),
            PaymentMethodType = paymentMethodType?.Trim(),
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void MarkSucceeded(DateTime? paidAt = null)
    {
        Status = PaymentStatus.Succeeded;
        PaidAt = paidAt ?? DateTime.UtcNow;
        ErrorMessage = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkProcessing()
    {
        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string errorMessage)
    {
        Status = PaymentStatus.Failed;
        ErrorMessage = errorMessage.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCancelled()
    {
        Status = PaymentStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkRefunded()
    {
        Status = PaymentStatus.Refunded;
        UpdatedAt = DateTime.UtcNow;
    }
}
