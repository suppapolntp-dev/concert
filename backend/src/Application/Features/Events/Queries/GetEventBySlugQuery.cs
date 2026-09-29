namespace TicketWorld.Application.Features.Events.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record EventDetailDto(
    Guid Id,
    string Title,
    string Slug,
    string? Description,
    EventCategory Category,
    EventStatus Status,
    DateTime StartAt,
    DateTime EndAt,
    DateTime? DoorsOpenAt,
    DateTime SalesStartAt,
    DateTime SalesEndAt,
    int MaxTicketsPerUser,
    bool HasWaitingRoom,
    string? PosterImageUrl,
    string? CoverImageUrl,
    VenueDto Venue,
    OrganizerDto Organizer,
    List<TicketTierDto> Tiers);

public sealed record VenueDto(
    Guid Id,
    string Name,
    string Address,
    string City,
    int TotalCapacity);

public sealed record OrganizerDto(
    Guid Id,
    string CompanyName,
    string? CompanyLogoUrl,
    string? Website);

public sealed record TicketTierDto(
    Guid Id,
    string Name,
    TicketTierType TierType,
    decimal Price,
    string ColorHex,
    string? Description,
    int AvailableQuota,
    int TotalQuota,
    string? EntryGate);

public sealed record GetEventBySlugQuery(string Slug) : IRequest<EventDetailDto>;

public class GetEventBySlugQueryHandler : IRequestHandler<GetEventBySlugQuery, EventDetailDto>
{
    private readonly IApplicationDbContext _context;

    public GetEventBySlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EventDetailDto> Handle(GetEventBySlugQuery request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().ToLowerInvariant();

        var ev = await _context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.Organizer)
            .Include(e => e.TicketTiers)
            .FirstOrDefaultAsync(e => e.Slug == normalizedSlug, cancellationToken);

        if (ev == null)
            throw new NotFoundException($"Event with slug '{request.Slug}' was not found.");

        var venueDto = new VenueDto(ev.Venue.Id, ev.Venue.Name, ev.Venue.Address, ev.Venue.City, ev.Venue.TotalCapacity);
        var organizerDto = new OrganizerDto(ev.Organizer.Id, ev.Organizer.CompanyName, ev.Organizer.CompanyLogoUrl, ev.Organizer.Website);
        var tiersDto = ev.TicketTiers
            .OrderBy(t => t.Price)
            .Select(t => new TicketTierDto(
                t.Id,
                t.Name,
                t.TierType,
                t.Price,
                t.ColorHex,
                t.Description,
                t.AvailableQuota,
                t.TotalQuota,
                t.EntryGate))
            .ToList();

        return new EventDetailDto(
            ev.Id,
            ev.Title,
            ev.Slug,
            ev.Description,
            ev.Category,
            ev.Status,
            ev.StartAt,
            ev.EndAt,
            ev.DoorsOpenAt,
            ev.SalesStartAt,
            ev.SalesEndAt,
            ev.MaxTicketsPerUser,
            ev.HasWaitingRoom,
            ev.PosterImageUrl,
            ev.CoverImageUrl,
            venueDto,
            organizerDto,
            tiersDto);
    }
}
