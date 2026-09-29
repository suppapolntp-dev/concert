namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing a system user across all roles (Customer, Organizer, OrgStaff, Admin).
/// Mapped to the 'users' table in PostgreSQL.
/// </summary>
public class User : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? Phone { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string PreferredLang { get; private set; } = "en";
    public string PreferredCurrency { get; private set; } = "THB";
    public string Timezone { get; private set; } = "Asia/Bangkok";
    public UserRole Role { get; private set; } = UserRole.Customer;
    public bool IsVerified { get; private set; }
    public bool IsBanned { get; private set; }
    public string? BannedReason { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTime? LockedUntil { get; private set; }
    public AuthProvider AuthProvider { get; private set; } = AuthProvider.Local;
    public string? GoogleSub { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ISoftDeletable
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public virtual OrganizerProfile? OrganizerProfile { get; private set; }
    public virtual ICollection<OrganizerStaff> OrganizerStaffAssignments { get; private set; } = new List<OrganizerStaff>();
    public virtual ICollection<Booking> Bookings { get; private set; } = new List<Booking>();
    public virtual ICollection<ArtistFollower> FollowedArtists { get; private set; } = new List<ArtistFollower>();
    public virtual ICollection<UserConsent> UserConsents { get; private set; } = new List<UserConsent>();
    public virtual ICollection<Waitlist> Waitlists { get; private set; } = new List<Waitlist>();

    // Parameterless constructor for EF Core
    protected User() { }

    public static User Create(
        string email,
        string passwordHash,
        string fullName,
        string? phone = null,
        UserRole role = UserRole.Customer,
        AuthProvider authProvider = AuthProvider.Local,
        string? googleSub = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name is required.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Phone = phone?.Trim(),
            Role = role,
            AuthProvider = authProvider,
            GoogleSub = googleSub,
            IsVerified = authProvider == AuthProvider.Google, // Google accounts pre-verified
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return user;
    }

    public void UpdateProfile(
        string fullName,
        string? phone,
        string? avatarUrl,
        string preferredLang,
        string preferredCurrency,
        string timezone)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("Full name cannot be empty.");

        FullName = fullName.Trim();
        Phone = phone?.Trim();
        AvatarUrl = avatarUrl?.Trim();
        PreferredLang = string.IsNullOrWhiteSpace(preferredLang) ? "en" : preferredLang.Trim().ToLowerInvariant();
        PreferredCurrency = string.IsNullOrWhiteSpace(preferredCurrency) ? "THB" : preferredCurrency.Trim().ToUpperInvariant();
        Timezone = string.IsNullOrWhiteSpace(timezone) ? "Asia/Bangkok" : timezone.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("Password hash cannot be empty.");

        PasswordHash = newPasswordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void VerifyEmail()
    {
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Ban(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Ban reason must be specified.");

        IsBanned = true;
        BannedReason = reason.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unban()
    {
        IsBanned = false;
        BannedReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordFailedLogin(int maxFailedAttempts = 5, TimeSpan? lockoutDuration = null)
    {
        FailedLoginCount++;
        if (FailedLoginCount >= maxFailedAttempts)
        {
            var duration = lockoutDuration ?? TimeSpan.FromMinutes(15);
            LockedUntil = DateTime.UtcNow.Add(duration);
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedLogin()
    {
        FailedLoginCount = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsLockedOut() => LockedUntil.HasValue && LockedUntil.Value > DateTime.UtcNow;

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
