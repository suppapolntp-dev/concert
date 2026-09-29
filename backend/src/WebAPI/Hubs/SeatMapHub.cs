namespace TicketWorld.WebAPI.Hubs;

using Microsoft.AspNetCore.SignalR;

/// <summary>
/// SignalR hub providing real-time seat lock and release broadcasting per concert event.
/// </summary>
public class SeatMapHub : Hub
{
    public async Task JoinEventSeatMap(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(eventId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"event:{eventId}");
        }
    }

    public async Task LeaveEventSeatMap(string eventId)
    {
        if (!string.IsNullOrWhiteSpace(eventId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"event:{eventId}");
        }
    }
}
