namespace TicketWorld.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Application.Features.Events.Commands;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

[ApiController]
[Route("api/organizer")]
[Authorize(Roles = "Organizer,Admin")]
public class OrganizerController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public OrganizerController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetMyEvents(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId!.Value;
        var organizerProfile = await _context.OrganizerProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (organizerProfile == null && _currentUserService.Role != UserRole.Admin)
            return NotFound("Organizer profile not found for the current user.");

        var query = _context.Events.AsQueryable();
        if (_currentUserService.Role != UserRole.Admin && organizerProfile != null)
        {
            query = query.Where(e => e.OrganizerId == organizerProfile.Id);
        }

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Slug,
                e.Category,
                Status = e.Status.ToString(),
                e.StartAt,
                e.EndAt,
                e.TotalSeats,
                e.SoldSeats,
                e.HeldSeats
            })
            .ToListAsync(cancellationToken);

        return Ok(events);
    }

    [HttpPost("events")]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetMyEvents), new { id = result }, new { id = result });
    }

    [HttpPut("events/{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route ID does not match command ID.");

        await Mediator.Send(command);
        return NoContent();
    }

    [HttpPost("events/{id:guid}/submit-approval")]
    public async Task<IActionResult> SubmitForApproval(Guid id, CancellationToken cancellationToken)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (ev == null) return NotFound();

        ev.SubmitForApproval();
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Event submitted for admin approval.", status = ev.Status.ToString() });
    }

    [HttpGet("events/{id:guid}/sales")]
    public async Task<IActionResult> GetEventSalesAnalytics(Guid id, CancellationToken cancellationToken)
    {
        var ev = await _context.Events
            .Include(e => e.TicketTiers)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (ev == null) return NotFound();

        var bookings = await _context.Bookings
            .Where(b => b.EventId == id && b.Status == BookingStatus.Confirmed)
            .ToListAsync(cancellationToken);

        var totalRevenue = bookings.Sum(b => b.FinalAmount);

        var tierMetrics = ev.TicketTiers.Select(t => new
        {
            t.Id,
            t.Name,
            t.Price,
            t.TotalQuota,
            t.AvailableQuota,
            t.HeldQuota,
            t.SoldQuota,
            Revenue = t.SoldQuota * t.Price
        });

        return Ok(new
        {
            EventId = ev.Id,
            Title = ev.Title,
            Status = ev.Status.ToString(),
            TotalRevenue = totalRevenue,
            TotalSeats = ev.TotalSeats,
            SoldSeats = ev.SoldSeats,
            HeldSeats = ev.HeldSeats,
            Tiers = tierMetrics
        });
    }
}
