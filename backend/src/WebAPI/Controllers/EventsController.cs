namespace TicketWorld.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketWorld.Application.Common.Models;
using TicketWorld.Application.Features.Events.Commands;
using TicketWorld.Application.Features.Events.Queries;
using TicketWorld.Domain.Enums;

public class EventsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<EventSummaryDto>>> GetEvents(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] EventCategory? category = null)
    {
        var response = await Mediator.Send(new GetEventsQuery(pageNumber, pageSize, search, category));
        return Ok(response);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<EventDetailDto>> GetEventBySlug(string slug)
    {
        var response = await Mediator.Send(new GetEventBySlugQuery(slug));
        return Ok(response);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateEvent([FromBody] CreateEventCommand command)
    {
        var eventId = await Mediator.Send(command);
        return Ok(eventId);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route ID and payload ID must match.");

        await Mediator.Send(command);
        return NoContent();
    }
}
