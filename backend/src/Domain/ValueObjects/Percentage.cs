namespace TicketWorld.Domain.ValueObjects;

using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a percentage between 0 and 100.
/// </summary>
public readonly record struct Percentage
{
    public decimal Value { get; }

    public Percentage(decimal value)
    {
        if (value is < 0 or > 100)
            throw new DomainException("Percentage must be between 0 and 100.");

        Value = value;
    }

    public static Percentage FromDecimalRate(decimal rate)
    {
        if (rate is < 0 or > 1)
            throw new DomainException("Decimal rate must be between 0 and 1.");

        return new Percentage(rate * 100m);
    }

    public decimal ApplyTo(decimal amount) => amount * (Value / 100m);
    public decimal AsDecimalRate => Value / 100m;

    public override string ToString() => $"{Value:N2}%";
}
