namespace TicketWorld.Application.Features.Events.Commands;

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TicketWorld.Application.Common.Interfaces;
using TicketWorld.Domain.Enums;
using TicketWorld.Domain.Exceptions;

public sealed record UpdateEventCommand(
    Guid Id,
    string Title,
    string? Description,
    EventCategory Category,
    DateTime StartAt,
    DateTime EndAt,
    DateTime SalesStartAt,
    DateTime SalesEndAt,
    int MaxTicketsPerUser,
    bool HasWaitingRoom,
    string? PosterImageUrl = null,
    string? CoverImageUrl = null,
    DateTime? DoorsOpenAt = null) : IRequest;

public class UpdateEventCommandValidator : AbstractValidator<UpdateEventCommand>
{
    public UpdateEventCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(300);
        RuleFor(v => v.StartAt).LessThan(v => v.EndAt);
        RuleFor(v => v.SalesStartAt).LessThan(v => v.SalesEndAt);
        RuleFor(v => v.MaxTicketsPerUser).GreaterThan(0).LessThanOrEqualTo(10);
    }
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateEventCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new ForbiddenException("Unauthenticated request.");

        var ev = await _context.Events
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (ev == null)
            throw new NotFoundException($"Event with ID {request.Id} was not found.");

        var isOwner = ev.Organizer.UserId == _currentUserService.UserId.Value;
        var isAdmin = _currentUserService.Role == UserRole.Admin;

        if (!isOwner && !isAdmin)
            throw new ForbiddenException("You do not have permission to modify this event.");

        ev.UpdateDetails(
            title: request.Title,
            description: request.Description,
            category: request.Category,
            startAt: request.StartAt,
            endAt: request.EndAt,
            salesStartAt: request.SalesStartAt,
            salesEndAt: request.SalesEndAt,
            maxTicketsPerUser: request.MaxTicketsPerUser,
            hasWaitingRoom: request.HasWaitingRoom,
            posterImageUrl: request.PosterImageUrl,
            coverImageUrl: request.CoverImageUrl,
            doorsOpenAt: request.DoorsOpenAt);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
