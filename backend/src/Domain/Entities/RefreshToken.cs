namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a hashed refresh token used for JWT token rotation and replay attack prevention.
/// Mapped to the 'refresh_tokens' table in PostgreSQL.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public string? DeviceInfo { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Computed properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation property
    public virtual User User { get; private set; } = null!;

    // Parameterless constructor for EF Core
    protected RefreshToken() { }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        DateTime expiresAt,
        string? deviceInfo = null,
        string? ipAddress = null)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            throw new DomainException("Token hash is required.");

        if (expiresAt <= DateTime.UtcNow)
            throw new DomainException("Expiration date must be in the future.");

        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash.Trim(),
            ExpiresAt = expiresAt,
            DeviceInfo = deviceInfo?.Trim(),
            IpAddress = ipAddress?.Trim(),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
            return;

        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = replacedByToken?.Trim();
    }
}
