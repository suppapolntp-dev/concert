namespace TicketWorld.Application.Common.Interfaces;

/// <summary>
/// Clock abstraction for testable time handling.
/// </summary>
public interface IDateTimeService
{
    DateTime UtcNow { get; }
}
