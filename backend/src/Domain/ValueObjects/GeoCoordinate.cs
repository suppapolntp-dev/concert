namespace TicketWorld.Domain.ValueObjects;

using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a geographic latitude and longitude position.
/// </summary>
public readonly record struct GeoCoordinate
{
    public double Latitude { get; }
    public double Longitude { get; }

    public GeoCoordinate(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90)
            throw new DomainException("Latitude must be between -90 and 90 degrees.");

        if (longitude is < -180 or > 180)
            throw new DomainException("Longitude must be between -180 and 180 degrees.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString() => $"({Latitude}, {Longitude})";
}
