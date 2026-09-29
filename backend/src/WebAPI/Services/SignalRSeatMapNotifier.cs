namespace TicketWorld.WebAPI.Services;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.WebAPI.Hubs;

public class SignalRSeatMapNotifier : ISeatMapNotifier
{
    private readonly IHubContext<SeatMapHub> _hubContext;
    private readonly ILogger<SignalRSeatMapNotifier> _logger;

    public SignalRSeatMapNotifier(
        IHubContext<SeatMapHub> hubContext,
        ILogger<SignalRSeatMapNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifySeatsStatusChangedAsync(
        Guid eventId,
        IEnumerable<Guid> seatIds,
        string status,
        CancellationToken cancellationToken = default)
    {
        var groupName = $"event:{eventId}";
        var payload = new
        {
            EventId = eventId,
            SeatIds = seatIds,
            Status = status,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients.Group(groupName).SendAsync("SeatStatusChanged", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast seat status change for event {EventId}", eventId);
        }
    }
}
