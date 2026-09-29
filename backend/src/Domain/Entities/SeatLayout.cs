namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Represents a seating configuration or SVG blueprint associated with a venue.
/// Mapped to the 'seat_layouts' table in PostgreSQL.
/// </summary>
public class SeatLayout : BaseEntity, IAuditableEntity
{
    public Guid VenueId { get; private set; }
    public string Name { get; private set; } = null!;
    public string LayoutJson { get; private set; } = "{}";
    public int TotalSeats { get; private set; }
    public bool IsDefault { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation Properties
    public virtual Venue Venue { get; private set; } = null!;
    public virtual ICollection<Event> Events { get; private set; } = new List<Event>();

    // Parameterless constructor for EF Core
    protected SeatLayout() { }

    public static SeatLayout Create(
        Guid venueId,
        string name,
        string layoutJson,
        int totalSeats,
        bool isDefault = false)
    {
        if (venueId == Guid.Empty)
            throw new DomainException("VenueId cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Layout name is required.");

        if (totalSeats < 0)
            throw new DomainException("Total seats cannot be negative.");

        return new SeatLayout
        {
            Id = Guid.NewGuid(),
            VenueId = venueId,
            Name = name.Trim(),
            LayoutJson = string.IsNullOrWhiteSpace(layoutJson) ? "{}" : layoutJson.Trim(),
            TotalSeats = totalSeats,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateLayout(string name, string layoutJson, int totalSeats)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Layout name cannot be empty.");

        if (totalSeats < 0)
            throw new DomainException("Total seats cannot be negative.");

        Name = name.Trim();
        LayoutJson = string.IsNullOrWhiteSpace(layoutJson) ? "{}" : layoutJson.Trim();
        TotalSeats = totalSeats;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
        UpdatedAt = DateTime.UtcNow;
    }
}
