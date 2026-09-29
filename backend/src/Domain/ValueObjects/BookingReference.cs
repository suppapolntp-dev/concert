namespace TicketWorld.Domain.ValueObjects;

using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a human-readable unique booking identifier.
/// Format: BK-YYYYMMDD-XXXXXX or TW-YYYY-XXXXXX
/// </summary>
public readonly record struct BookingReference
{
    private static readonly Regex RefRegex = new(
        @"^(BK-\d{8}-[A-Z0-9]{6,8}|TW-\d{4}-[A-Z0-9]{6})$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public BookingReference(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Booking reference cannot be empty.");

        var normalized = value.Trim().ToUpperInvariant();

        if (!RefRegex.IsMatch(normalized))
            throw new DomainException($"Invalid booking reference format: {value}");

        Value = normalized;
    }

    public static BookingReference Generate()
    {
        var year = DateTime.UtcNow.Year;
        var random = RandomNumberGenerator.GetBytes(4);
        var code = Convert.ToHexString(random)[..6].ToUpperInvariant();
        return new BookingReference($"TW-{year}-{code}");
    }

    public static implicit operator string(BookingReference reference) => reference.Value;
    public static explicit operator BookingReference(string value) => new(value);
    public override string ToString() => Value;
}
