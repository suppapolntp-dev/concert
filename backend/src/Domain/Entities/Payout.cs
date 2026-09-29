namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a platform disbursement to an event organizer.
/// Mapped to the 'payouts' table in PostgreSQL.
/// </summary>
public class Payout : BaseEntity, IAuditableEntity
{
    public Guid OrganizerId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "THB";
    public PayoutStatus Status { get; private set; } = PayoutStatus.Pending;
    public string BankAccountSnapshotEnc { get; private set; } = null!;
    public string? TransactionReference { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual OrganizerProfile Organizer { get; private set; } = null!;
    public virtual User? ApprovedByUser { get; private set; }

    // Parameterless constructor for EF Core
    protected Payout() { }

    public static Payout Create(
        Guid organizerId,
        decimal amount,
        string bankAccountSnapshotEnc,
        string currency = "THB")
    {
        if (organizerId == Guid.Empty)
            throw new DomainException("OrganizerId cannot be empty.");

        if (amount <= 0)
            throw new DomainException("Payout amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(bankAccountSnapshotEnc))
            throw new DomainException("Bank account snapshot is required.");

        return new Payout
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            Amount = amount,
            Currency = string.IsNullOrWhiteSpace(currency) ? "THB" : currency.Trim().ToUpperInvariant(),
            BankAccountSnapshotEnc = bankAccountSnapshotEnc,
            Status = PayoutStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Approve(Guid adminUserId)
    {
        if (adminUserId == Guid.Empty)
            throw new DomainException("Approving admin user ID cannot be empty.");

        if (Status != PayoutStatus.Pending)
            throw new DomainException($"Cannot approve payout in '{Status}' status.");

        Status = PayoutStatus.Approved;
        ApprovedBy = adminUserId;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartProcessing()
    {
        if (Status != PayoutStatus.Approved)
            throw new DomainException("Payout must be approved before processing.");

        Status = PayoutStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete(string transactionReference)
    {
        if (string.IsNullOrWhiteSpace(transactionReference))
            throw new DomainException("Transaction reference is required.");

        Status = PayoutStatus.Completed;
        TransactionReference = transactionReference.Trim();
        ProcessedAt = DateTime.UtcNow;
        FailureReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string failureReason)
    {
        Status = PayoutStatus.Failed;
        FailureReason = failureReason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
