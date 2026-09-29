namespace TicketWorld.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketWorld.Application.Features.Bookings.Commands;
using TicketWorld.Application.Features.Bookings.Queries;

[Authorize]
public class BookingsController : ApiControllerBase
{
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutBookingResponse>> Checkout([FromBody] CheckoutBookingCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelBooking(Guid id, [FromBody] CancelBookingCommand? command = null)
    {
        var cmd = command != null && command.BookingId == id
            ? command
            : new CancelBookingCommand(id);

        await Mediator.Send(cmd);
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDetailDto>> GetBookingById(Guid id)
    {
        var response = await Mediator.Send(new GetBookingByIdQuery(id));
        return Ok(response);
    }

    [HttpGet("my-tickets")]
    public async Task<ActionResult<List<UserTicketDto>>> GetMyTickets()
    {
        var response = await Mediator.Send(new GetMyTicketsQuery());
        return Ok(response);
    }
}
