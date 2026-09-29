namespace TicketWorld.Domain.ValueObjects;

using System.Text.RegularExpressions;
using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a validated E.164 international phone number.
/// </summary>
public readonly record struct PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^\+[1-9]\d{6,14}$",
        RegexOptions.Compiled);

    public string Value { get; }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Phone number cannot be empty.");

        var trimmed = value.Trim();

        if (!PhoneRegex.IsMatch(trimmed))
            throw new DomainException($"Invalid E.164 phone number format: {value}");

        Value = trimmed;
    }

    public static implicit operator string(PhoneNumber phone) => phone.Value;
    public static explicit operator PhoneNumber(string value) => new(value);
    public override string ToString() => Value;
}
