namespace TicketWorld.Application.Common.Interfaces;

public interface ISeatMapNotifier
{
    Task NotifySeatsStatusChangedAsync(
        Guid eventId,
        IEnumerable<Guid> seatIds,
        string status,
        CancellationToken cancellationToken = default);
}
