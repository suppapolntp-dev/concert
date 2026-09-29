namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing an event organizer profile (legal entity or verified promoter).
/// Mapped to the 'organizer_profiles' table in PostgreSQL.
/// </summary>
public class OrganizerProfile : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public Guid UserId { get; private set; }
    public string CompanyName { get; private set; } = null!;
    public string? CompanyLogoUrl { get; private set; }
    public string? Description { get; private set; }
    public string? Website { get; private set; }
    public string? TaxId { get; private set; }
    public string BankAccountNoEnc { get; private set; } = null!;
    public string BankName { get; private set; } = null!;
    public string BankAccountLast4 { get; private set; } = null!;
    public string PayoutSchedule { get; private set; } = "monthly";
    public decimal CommissionRate { get; private set; } = 0.0500m;
    public bool IsApproved { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ISoftDeletable
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; private set; } = null!;
    public virtual User? ApprovedByUser { get; private set; }
    public virtual ICollection<OrganizerStaff> StaffMembers { get; private set; } = new List<OrganizerStaff>();
    public virtual ICollection<Event> Events { get; private set; } = new List<Event>();
    public virtual ICollection<Payout> Payouts { get; private set; } = new List<Payout>();

    // Parameterless constructor for EF Core
    protected OrganizerProfile() { }

    public static OrganizerProfile Create(
        Guid userId,
        string companyName,
        string bankAccountNoEnc,
        string bankName,
        string bankAccountLast4,
        string? taxId = null,
        string? description = null,
        string? website = null,
        string? companyLogoUrl = null,
        decimal commissionRate = 0.0500m)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainException("Company name is required.");

        if (string.IsNullOrWhiteSpace(bankAccountNoEnc))
            throw new DomainException("Encrypted bank account number is required.");

        if (string.IsNullOrWhiteSpace(bankName))
            throw new DomainException("Bank name is required.");

        if (string.IsNullOrWhiteSpace(bankAccountLast4) || bankAccountLast4.Trim().Length > 4)
            throw new DomainException("Bank account last 4 digits are invalid.");

        if (commissionRate < 0m || commissionRate > 1m)
            throw new DomainException("Commission rate must be between 0.0 (0%) and 1.0 (100%).");

        return new OrganizerProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CompanyName = companyName.Trim(),
            BankAccountNoEnc = bankAccountNoEnc,
            BankName = bankName.Trim(),
            BankAccountLast4 = bankAccountLast4.Trim(),
            TaxId = taxId?.Trim(),
            Description = description?.Trim(),
            Website = website?.Trim(),
            CompanyLogoUrl = companyLogoUrl?.Trim(),
            CommissionRate = commissionRate,
            PayoutSchedule = "monthly",
            IsApproved = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateDetails(
        string companyName,
        string? companyLogoUrl,
        string? description,
        string? website,
        string? taxId)
    {
        if (string.IsNullOrWhiteSpace(companyName))
            throw new DomainException("Company name cannot be empty.");

        CompanyName = companyName.Trim();
        CompanyLogoUrl = companyLogoUrl?.Trim();
        Description = description?.Trim();
        Website = website?.Trim();
        TaxId = taxId?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBankInfo(
        string bankAccountNoEnc,
        string bankName,
        string bankAccountLast4)
    {
        if (string.IsNullOrWhiteSpace(bankAccountNoEnc))
            throw new DomainException("Encrypted bank account number is required.");

        if (string.IsNullOrWhiteSpace(bankName))
            throw new DomainException("Bank name is required.");

        if (string.IsNullOrWhiteSpace(bankAccountLast4) || bankAccountLast4.Trim().Length > 4)
            throw new DomainException("Bank account last 4 digits are invalid.");

        BankAccountNoEnc = bankAccountNoEnc;
        BankName = bankName.Trim();
        BankAccountLast4 = bankAccountLast4.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePayoutSchedule(string payoutSchedule)
    {
        var normalized = payoutSchedule.Trim().ToLowerInvariant();
        if (normalized != "weekly" && normalized != "biweekly" && normalized != "monthly")
            throw new DomainException("Payout schedule must be 'weekly', 'biweekly', or 'monthly'.");

        PayoutSchedule = normalized;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateCommissionRate(decimal newRate)
    {
        if (newRate < 0m || newRate > 1m)
            throw new DomainException("Commission rate must be between 0.0 (0%) and 1.0 (100%).");

        CommissionRate = newRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid adminUserId)
    {
        if (adminUserId == Guid.Empty)
            throw new DomainException("Approving admin user ID cannot be empty.");

        IsApproved = true;
        ApprovedBy = adminUserId;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevokeApproval()
    {
        IsApproved = false;
        ApprovedBy = null;
        ApprovedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
