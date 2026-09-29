namespace TicketWorld.Infrastructure.Services;

using TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Provides UTC system clock time.
/// </summary>
public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
