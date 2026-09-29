namespace TicketWorld.Domain.Enums;

/// <summary>
/// Defines user access levels and authorization roles across the TicketWorld platform.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Level 1: Standard customer who can browse events, buy tickets, join waiting rooms, and follow artists.
    /// </summary>
    Customer,

    /// <summary>
    /// Level 2: Concert organizer who can create events, manage ticket tiers, view sales, and request payouts.
    /// </summary>
    Organizer,

    /// <summary>
    /// Level 2b: Organizer operational staff with scoped permissions (e.g., ticket check-in scanning).
    /// </summary>
    OrgStaff,

    /// <summary>
    /// Level 3: Platform administrator with full access to approvals, disputes, user bans, and system configs.
    /// </summary>
    Admin
}
