namespace TicketWorld.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketWorld.Application.Features.Auth.Commands;
using TicketWorld.Application.Features.Auth.Queries;

public class AuthController : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var response = await Mediator.Send(command);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var response = await Mediator.Send(new GetCurrentUserQuery());
        return Ok(response);
    }
}
