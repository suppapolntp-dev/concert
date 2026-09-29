namespace TicketWorld.Domain.ValueObjects;

using TicketWorld.Domain.Exceptions;

public enum Currency
{
    THB,
    USD,
    JPY,
    KRW,
    EUR,
    GBP,
    SGD
}

/// <summary>
/// Immutable value object representing a monetary amount with an explicit ISO currency.
/// Prevents cross-currency arithmetic errors and negative balances.
/// </summary>
public readonly record struct Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency = Currency.THB)
    {
        if (amount < 0)
            throw new DomainException("Money amount cannot be negative.");

        Amount = Math.Round(amount, 2);
        Currency = currency;
    }

    public static Money Zero(Currency currency = Currency.THB) => new(0m, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot add {Currency} to {other.Currency}.");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"Cannot subtract different currencies: {Currency} and {other.Currency}.");

        if (Amount < other.Amount)
            throw new DomainException("Insufficient amount.");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money ApplyDiscount(decimal percentage)
    {
        if (percentage is < 0 or > 100)
            throw new DomainException("Discount percentage must be between 0 and 100.");

        return new Money(Amount * (1 - percentage / 100m), Currency);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);

    public override string ToString() => $"{Amount:N2} {Currency}";
}
