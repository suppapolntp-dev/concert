namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a peer-to-peer ticket transfer transaction.
/// Mapped to the 'ticket_transfers' table in PostgreSQL.
/// </summary>
public class TicketTransfer : BaseEntity, IAuditableEntity
{
    public Guid BookingItemId { get; private set; }
    public Guid FromUserId { get; private set; }
    public Guid? ToUserId { get; private set; }
    public string ToEmail { get; private set; } = null!;
    public string TransferToken { get; private set; } = null!;
    public TransferStatus Status { get; private set; } = TransferStatus.Pending;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual BookingItem BookingItem { get; private set; } = null!;
    public virtual User FromUser { get; private set; } = null!;
    public virtual User? ToUser { get; private set; }

    // Parameterless constructor for EF Core
    protected TicketTransfer() { }

    public static TicketTransfer Create(
        Guid bookingItemId,
        Guid fromUserId,
        string toEmail,
        TimeSpan validityDuration,
        Guid? toUserId = null)
    {
        if (bookingItemId == Guid.Empty)
            throw new DomainException("BookingItemId cannot be empty.");

        if (fromUserId == Guid.Empty)
            throw new DomainException("FromUserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new DomainException("Recipient email is required.");

        return new TicketTransfer
        {
            Id = Guid.NewGuid(),
            BookingItemId = bookingItemId,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            ToEmail = toEmail.Trim().ToLowerInvariant(),
            TransferToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
            Status = TransferStatus.Pending,
            ExpiresAt = DateTime.UtcNow.Add(validityDuration),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Accept(Guid recipientUserId)
    {
        if (recipientUserId == Guid.Empty)
            throw new DomainException("Recipient user ID cannot be empty.");

        if (Status != TransferStatus.Pending)
            throw new DomainException($"Cannot accept transfer in '{Status}' status.");

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = TransferStatus.Expired;
            UpdatedAt = DateTime.UtcNow;
            throw new DomainException("Transfer request has expired.");
        }

        ToUserId = recipientUserId;
        Status = TransferStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        if (Status != TransferStatus.Pending)
            throw new DomainException($"Cannot reject transfer in '{Status}' status.");

        Status = TransferStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel(Guid userId)
    {
        if (userId != FromUserId)
            throw new DomainException("Only the sender can cancel the ticket transfer.");

        if (Status != TransferStatus.Pending)
            throw new DomainException($"Cannot cancel transfer in '{Status}' status.");

        Status = TransferStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }
}
