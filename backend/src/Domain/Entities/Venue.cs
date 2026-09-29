namespace TicketWorld.Domain.Entities;

using TicketWorld.Domain.Common;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Aggregate root representing an event venue or concert hall.
/// Mapped to the 'venues' table in PostgreSQL.
/// </summary>
public class Venue : AggregateRoot, IAuditableEntity, ISoftDeletable
{
    public string Name { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Country { get; private set; } = "Thailand";
    public string? PostalCode { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public int TotalCapacity { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // ISoftDeletable
    public DateTime? DeletedAt { get; set; }

    // Navigation Properties
    public virtual ICollection<SeatLayout> SeatLayouts { get; private set; } = new List<SeatLayout>();
    public virtual ICollection<Event> Events { get; private set; } = new List<Event>();

    // Parameterless constructor for EF Core
    protected Venue() { }

    public static Venue Create(
        string name,
        string address,
        string city,
        int totalCapacity,
        string country = "Thailand",
        string? postalCode = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Venue name is required.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Address is required.");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.");

        if (totalCapacity <= 0)
            throw new DomainException("Total capacity must be greater than zero.");

        return new Venue
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Address = address.Trim(),
            City = city.Trim(),
            Country = string.IsNullOrWhiteSpace(country) ? "Thailand" : country.Trim(),
            PostalCode = postalCode?.Trim(),
            Latitude = latitude,
            Longitude = longitude,
            TotalCapacity = totalCapacity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        string address,
        string city,
        int totalCapacity,
        string country,
        string? postalCode,
        decimal? latitude,
        decimal? longitude)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Venue name cannot be empty.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("Address cannot be empty.");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City cannot be empty.");

        if (totalCapacity <= 0)
            throw new DomainException("Total capacity must be greater than zero.");

        Name = name.Trim();
        Address = address.Trim();
        City = city.Trim();
        Country = string.IsNullOrWhiteSpace(country) ? "Thailand" : country.Trim();
        PostalCode = postalCode?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        TotalCapacity = totalCapacity;
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
