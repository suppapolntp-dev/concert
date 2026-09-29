namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents an organizer's staff member with scoped event access and granular operational permissions.
/// Mapped to the 'organizer_staff' table in PostgreSQL.
/// </summary>
public class OrganizerStaff : BaseEntity, IAuditableEntity
{
    private static readonly HashSet<string> ForbiddenStaffPermissions = new(StringComparer.OrdinalIgnoreCase)
    {
        "finance.all",
        "finance.payout",
        "finance.bank",
        "staff.manage",
        "staff.delete"
    };

    public Guid OrganizerId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? EventId { get; private set; }
    public List<string> Permissions { get; private set; } = new();
    public bool IsActive { get; private set; }
    public Guid InvitedBy { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual OrganizerProfile Organizer { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;
    public virtual Event? Event { get; private set; }
    public virtual User InvitedByUser { get; private set; } = null!;

    // Parameterless constructor for EF Core
    protected OrganizerStaff() { }

    public static OrganizerStaff Create(
        Guid organizerId,
        Guid userId,
        Guid invitedBy,
        Guid? eventId = null,
        IEnumerable<string>? permissions = null)
    {
        if (organizerId == Guid.Empty)
            throw new DomainException("OrganizerId cannot be empty.");

        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        if (invitedBy == Guid.Empty)
            throw new DomainException("InvitedBy user ID cannot be empty.");

        var staff = new OrganizerStaff
        {
            Id = Guid.NewGuid(),
            OrganizerId = organizerId,
            UserId = userId,
            EventId = eventId,
            InvitedBy = invitedBy,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (permissions != null)
        {
            foreach (var perm in permissions)
            {
                staff.GrantPermission(perm);
            }
        }

        return staff;
    }

    public void GrantPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new DomainException("Permission name cannot be empty.");

        var normalized = permission.Trim().ToLowerInvariant();

        if (ForbiddenStaffPermissions.Contains(normalized) || normalized.StartsWith("finance.", StringComparison.OrdinalIgnoreCase))
            throw new DomainException($"Permission '{permission}' is restricted and cannot be granted to staff.");

        if (!Permissions.Contains(normalized))
        {
            Permissions.Add(normalized);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RevokePermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return;

        var normalized = permission.Trim().ToLowerInvariant();
        if (Permissions.Remove(normalized))
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void SetPermissions(IEnumerable<string> permissions)
    {
        var validatedList = new List<string>();
        foreach (var perm in permissions)
        {
            if (string.IsNullOrWhiteSpace(perm))
                continue;

            var normalized = perm.Trim().ToLowerInvariant();
            if (ForbiddenStaffPermissions.Contains(normalized) || normalized.StartsWith("finance.", StringComparison.OrdinalIgnoreCase))
                throw new DomainException($"Permission '{perm}' is restricted and cannot be granted to staff.");

            if (!validatedList.Contains(normalized))
            {
                validatedList.Add(normalized);
            }
        }

        Permissions = validatedList;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasPermission(string permission)
    {
        if (!IsActive)
            return false;

        return Permissions.Contains(permission.Trim().ToLowerInvariant());
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
