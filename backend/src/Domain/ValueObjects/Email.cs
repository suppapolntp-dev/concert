namespace TicketWorld.Domain.ValueObjects;

using System.Text.RegularExpressions;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a normalized RFC 5322 compliant email address.
/// </summary>
public readonly record struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalized))
            throw new DomainException($"Invalid email format: {value}");

        Value = normalized;
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value) => new(value);
    public override string ToString() => Value;
}
