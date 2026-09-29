namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Record of user statutory consent for Thailand PDPA compliance.
/// Mapped to the 'user_consents' table in PostgreSQL.
/// </summary>
public class UserConsent : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public string ConsentType { get; private set; } = null!;
    public bool IsGranted { get; private set; }
    public string PolicyVersion { get; private set; } = "v1.0";
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime GrantedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual User User { get; private set; } = null!;

    // Parameterless constructor for EF Core
    protected UserConsent() { }

    public static UserConsent Create(
        Guid userId,
        string consentType,
        bool isGranted,
        string policyVersion = "v1.0",
        string? ipAddress = null,
        string? userAgent = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(consentType))
            throw new DomainException("Consent type is required.");

        var now = DateTime.UtcNow;
        return new UserConsent
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConsentType = consentType.Trim().ToUpperInvariant(),
            IsGranted = isGranted,
            PolicyVersion = string.IsNullOrWhiteSpace(policyVersion) ? "v1.0" : policyVersion.Trim(),
            IpAddress = ipAddress?.Trim(),
            UserAgent = userAgent?.Trim(),
            GrantedAt = now,
            RevokedAt = isGranted ? null : now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void Revoke(string? ipAddress = null, string? userAgent = null)
    {
        IsGranted = false;
        RevokedAt = DateTime.UtcNow;
        IpAddress = ipAddress?.Trim() ?? IpAddress;
        UserAgent = userAgent?.Trim() ?? UserAgent;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReGrant(string policyVersion, string? ipAddress = null, string? userAgent = null)
    {
        IsGranted = true;
        GrantedAt = DateTime.UtcNow;
        RevokedAt = null;
        PolicyVersion = policyVersion.Trim();
        IpAddress = ipAddress?.Trim() ?? IpAddress;
        UserAgent = userAgent?.Trim() ?? UserAgent;
        UpdatedAt = DateTime.UtcNow;
    }
}
