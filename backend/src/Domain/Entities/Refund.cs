namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a customer or automated refund lifecycle.
/// Mapped to the 'refunds' table in PostgreSQL.
/// </summary>
public class Refund : BaseEntity, IAuditableEntity
{
    public Guid BookingId { get; private set; }
    public Guid? PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "THB";
    public RefundReason Reason { get; private set; }
    public RefundStatus Status { get; private set; } = RefundStatus.Requested;
    public string? GatewayRefundId { get; private set; }
    public string? TransactionReference { get; private set; }
    public string? RejectionReason { get; private set; }
    public Guid RequestedBy { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Booking Booking { get; private set; } = null!;
    public virtual Payment? Payment { get; private set; }
    public virtual User RequestedByUser { get; private set; } = null!;
    public virtual User? ApprovedByUser { get; private set; }

    // Parameterless constructor for EF Core
    protected Refund() { }

    public static Refund Create(
        Guid bookingId,
        decimal amount,
        RefundReason reason,
        Guid requestedBy,
        Guid? paymentId = null,
        string currency = "THB")
    {
        if (bookingId == Guid.Empty)
            throw new DomainException("BookingId cannot be empty.");

        if (requestedBy == Guid.Empty)
            throw new DomainException("RequestedBy user ID cannot be empty.");

        if (amount <= 0)
            throw new DomainException("Refund amount must be greater than zero.");

        return new Refund
        {
            Id = Guid.NewGuid(),
            BookingId = bookingId,
            PaymentId = paymentId,
            Amount = amount,
            Reason = reason,
            RequestedBy = requestedBy,
            Currency = string.IsNullOrWhiteSpace(currency) ? "THB" : currency.Trim().ToUpperInvariant(),
            Status = RefundStatus.Requested,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid adminUserId)
    {
        if (adminUserId == Guid.Empty)
            throw new DomainException("Approving admin user ID cannot be empty.");

        if (Status != RefundStatus.Requested)
            throw new DomainException($"Cannot approve refund in '{Status}' status.");

        Status = RefundStatus.Approved;
        ApprovedBy = adminUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string rejectionReason)
    {
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new DomainException("Rejection reason must be specified.");

        Status = RefundStatus.Rejected;
        RejectionReason = rejectionReason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        Status = RefundStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string? gatewayRefundId, string? transactionReference = null)
    {
        Status = RefundStatus.Completed;
        GatewayRefundId = gatewayRefundId?.Trim();
        TransactionReference = transactionReference?.Trim();
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
