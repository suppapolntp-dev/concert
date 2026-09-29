namespace TicketWorld.Domain.ValueObjects;

using TicketWorld.Domain.Exceptions;

/// <summary>
/// Immutable value object representing a validated chronological time window.
/// </summary>
public readonly record struct DateRange
{
    public DateTime Start { get; }
    public DateTime End { get; }

    public DateRange(DateTime start, DateTime end)
    {
        if (end <= start)
            throw new DomainException("End date must be chronologically after start date.");

        Start = start;
        End = end;
    }

    public bool Contains(DateTime point) => point >= Start && point <= End;
    public bool Overlaps(DateRange other) => Start < other.End && End > other.Start;
    public TimeSpan Duration => End - Start;

    public override string ToString() => $"{Start:O} - {End:O}";
}
