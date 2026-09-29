namespace TicketWorld.Application.Features.Events.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Entities;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record CreateTicketTierInput(
    string Name,
    TicketTierType TierType,
    decimal Price,
    int TotalQuota,
    string ColorHex = "#3B82F6",
    string? Description = null,
    string? EntryGate = null);

public sealed record CreateEventCommand(
    Guid VenueId,
    string Title,
    DateTime StartAt,
    DateTime EndAt,
    DateTime SalesStartAt,
    DateTime SalesEndAt,
    EventCategory Category = EventCategory.Concert,
    Guid? SeatLayoutId = null,
    string? Description = null,
    string? PosterImageUrl = null,
    string? CoverImageUrl = null,
    DateTime? DoorsOpenAt = null,
    int MaxTicketsPerUser = 4,
    bool HasWaitingRoom = false,
    List<CreateTicketTierInput>? Tiers = null) : IRequest<Guid>;

public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    public CreateEventCommandValidator()
    {
        RuleFor(v => v.VenueId).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(300);
        RuleFor(v => v.StartAt).LessThan(v => v.EndAt).WithMessage("Start time must be before end time.");
        RuleFor(v => v.SalesStartAt).LessThan(v => v.SalesEndAt).WithMessage("Sales start time must be before sales end time.");
        RuleFor(v => v.MaxTicketsPerUser).GreaterThan(0).LessThanOrEqualTo(10);
    }
}

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateEventCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var userId = _currentUserService.UserId.Value;

        var organizerProfile = await _context.OrganizerProfiles
            .FirstOrDefaultAsync(o => o.UserId == userId, cancellationToken);

        if (organizerProfile == null)
            throw new ForbiddenException("Only registered organizers can create events.");

        if (!organizerProfile.IsApproved)
            throw new ForbiddenException("Your organizer profile is pending admin approval.");

        var venueExists = await _context.Venues.AnyAsync(v => v.Id == request.VenueId, cancellationToken);
        if (!venueExists)
            throw new NotFoundException($"Venue with ID {request.VenueId} was not found.");

        var newEvent = Event.Create(
            organizerId: organizerProfile.Id,
            venueId: request.VenueId,
            title: request.Title,
            startAt: request.StartAt,
            endAt: request.EndAt,
            salesStartAt: request.SalesStartAt,
            salesEndAt: request.SalesEndAt,
            category: request.Category,
            seatLayoutId: request.SeatLayoutId,
            description: request.Description,
            posterImageUrl: request.PosterImageUrl,
            coverImageUrl: request.CoverImageUrl,
            doorsOpenAt: request.DoorsOpenAt,
            maxTicketsPerUser: request.MaxTicketsPerUser,
            hasWaitingRoom: request.HasWaitingRoom);

        _context.Events.Add(newEvent);

        if (request.Tiers != null && request.Tiers.Count != 0)
        {
            var totalSeats = 0;
            foreach (var tierInput in request.Tiers)
            {
                var tier = TicketTier.Create(
                    eventId: newEvent.Id,
                    name: tierInput.Name,
                    price: tierInput.Price,
                    totalQuota: tierInput.TotalQuota,
                    tierType: tierInput.TierType,
                    colorHex: tierInput.ColorHex,
                    description: tierInput.Description,
                    entryGate: tierInput.EntryGate);

                _context.TicketTiers.Add(tier);
                totalSeats += tierInput.TotalQuota;
            }
            newEvent.SetSeatCapacity(totalSeats);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return newEvent.Id;
    }
}
