namespace TicketWorld.WebAPI.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;

public sealed record RejectEventRequest(string Reason);

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ApiControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AdminController(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    [HttpGet("events/pending")]
    public async Task<IActionResult> GetPendingEvents(CancellationToken cancellationToken)
    {
        var pendingEvents = await _context.Events
            .Include(e => e.Organizer)
            .Where(e => e.Status == EventStatus.PendingApproval)
            .OrderBy(e => e.CreatedAt)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Category,
                e.StartAt,
                e.EndAt,
                OrganizerCompany = e.Organizer.CompanyName,
                e.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(pendingEvents);
    }

    [HttpPost("events/{id:guid}/approve")]
    public async Task<IActionResult> ApproveEvent(Guid id, CancellationToken cancellationToken)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (ev == null) return NotFound();

        ev.Approve();
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Event approved successfully.", status = ev.Status.ToString() });
    }

    [HttpPost("events/{id:guid}/reject")]
    public async Task<IActionResult> RejectEvent(Guid id, [FromBody] RejectEventRequest request, CancellationToken cancellationToken)
    {
        var ev = await _context.Events.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (ev == null) return NotFound();

        ev.Reject(request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Event rejected.", status = ev.Status.ToString(), reason = ev.RejectionReason });
    }

    [HttpGet("organizers/pending")]
    public async Task<IActionResult> GetPendingOrganizers(CancellationToken cancellationToken)
    {
        var pending = await _context.OrganizerProfiles
            .Include(p => p.User)
            .Where(p => !p.IsApproved)
            .Select(p => new
            {
                p.Id,
                p.UserId,
                UserEmail = p.User.Email,
                p.CompanyName,
                p.TaxId,
                p.Website,
                p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(pending);
    }

    [HttpPost("organizers/{id:guid}/approve")]
    public async Task<IActionResult> ApproveOrganizer(Guid id, CancellationToken cancellationToken)
    {
        var profile = await _context.OrganizerProfiles.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (profile == null) return NotFound();

        var adminId = _currentUserService.UserId!.Value;
        profile.Approve(adminId);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = "Organizer profile approved." });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetPlatformStats(CancellationToken cancellationToken)
    {
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalEvents = await _context.Events.CountAsync(cancellationToken);
        var totalBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed, cancellationToken);
        var totalRevenue = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .SumAsync(b => b.FinalAmount, cancellationToken);

        return Ok(new
        {
            TotalUsers = totalUsers,
            TotalEvents = totalEvents,
            TotalConfirmedBookings = totalBookings,
            TotalPlatformRevenue = totalRevenue
        });
    }
}
