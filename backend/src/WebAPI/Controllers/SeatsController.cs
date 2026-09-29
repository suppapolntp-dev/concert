namespace TicketWorld.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketWorld.Application.Features.Seats.Commands;
using TicketWorld.Application.Features.Seats.Queries;

public class SeatsController : ApiControllerBase
{
    [HttpGet("event/{eventId:guid}")]
    public async Task<ActionResult<List<SeatStatusDto>>> GetEventSeats(Guid eventId)
    {
        var response = await Mediator.Send(new GetEventSeatsQuery(eventId));
        return Ok(response);
    }

    [Authorize]
    [HttpPost("hold")]
    public async Task<ActionResult<HoldSeatsResponse>> HoldSeats([FromBody] HoldSeatsCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [Authorize]
    [HttpPost("release")]
    public async Task<IActionResult> ReleaseSeats([FromBody] ReleaseSeatsCommand command)
    {
        await Mediator.Send(command);
        return NoContent();
    }
}
