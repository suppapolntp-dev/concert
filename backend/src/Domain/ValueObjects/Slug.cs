namespace TicketWorld.Domain.ValueObjects;

using System.Text.RegularExpressions;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a URL-safe lowercase slug identifier.
/// </summary>
public readonly record struct Slug
{
    public string Value { get; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Slug cannot be empty.");

        var normalized = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9-]", "-");
        normalized = Regex.Replace(normalized, @"-{2,}", "-").Trim('-');

        if (string.IsNullOrWhiteSpace(normalized))
            throw new DomainException("Slug cannot result in an empty string.");

        if (normalized.Length > 300)
            throw new DomainException("Slug exceeds maximum length of 300 characters.");

        Value = normalized;
    }

    public static implicit operator string(Slug slug) => slug.Value;
    public static explicit operator Slug(string value) => new(value);
    public override string ToString() => Value;
}
