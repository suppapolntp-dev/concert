namespace TicketWorld.Application.Features.Events.Queries;

using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Application.Common.Models;
using TicketWorld.Domain.Enums;

public sealed record EventSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    EventCategory Category,
    EventStatus Status,
    DateTime StartAt,
    DateTime EndAt,
    string VenueName,
    string City,
    string? PosterImageUrl,
    decimal LowestPrice,
    bool HasWaitingRoom);

public sealed record GetEventsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    EventCategory? Category = null) : IRequest<PaginatedList<EventSummaryDto>>;

public class GetEventsQueryHandler : IRequestHandler<GetEventsQuery, PaginatedList<EventSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<EventSummaryDto>> Handle(GetEventsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Events
            .AsNoTracking()
            .Include(e => e.Venue)
            .Include(e => e.TicketTiers)
            .Where(e => e.Status == EventStatus.Published || e.Status == EventStatus.OnSale || e.Status == EventStatus.SoldOut);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLowerInvariant();
            query = query.Where(e => e.Title.ToLower().Contains(search) || e.Venue.Name.ToLower().Contains(search));
        }

        if (request.Category.HasValue)
        {
            query = query.Where(e => e.Category == request.Category.Value);
        }

        var projected = query
            .OrderBy(e => e.StartAt)
            .Select(e => new EventSummaryDto(
                e.Id,
                e.Title,
                e.Slug,
                e.Category,
                e.Status,
                e.StartAt,
                e.EndAt,
                e.Venue.Name,
                e.Venue.City,
                e.PosterImageUrl,
                e.TicketTiers.Any() ? e.TicketTiers.Min(t => t.Price) : 0m,
                e.HasWaitingRoom));

        return await PaginatedList<EventSummaryDto>.CreateAsync(
            projected,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
